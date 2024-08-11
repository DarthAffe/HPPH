using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HPPH;

public static unsafe partial class PixelHelper
{
    #region Constants

    private const int MIN_BATCH_SIZE = 8;

    #endregion

    #region Methods

    public static Span<TTarget> ConvertInPlace<TSource, TTarget>(this Span<TSource> colors)
        where TSource : struct, IColor
        where TTarget : struct, IColor
    {
        if (colors == null) throw new ArgumentNullException(nameof(colors));
        if (colors.Length == 0) return MemoryMarshal.Cast<TSource, TTarget>(colors);

        IColorFormat sourceFormat = TSource.ColorFormat;
        IColorFormat targetFormat = TTarget.ColorFormat;

        if (sourceFormat == targetFormat) return MemoryMarshal.Cast<TSource, TTarget>(colors);
        if (sourceFormat.BytesPerPixel != targetFormat.BytesPerPixel) throw new NotSupportedException("In-place conversion requires the same BPP for source and target.");

        Span<byte> data = MemoryMarshal.AsBytes(colors);
        Convert(data, data, sourceFormat, targetFormat);

        return MemoryMarshal.Cast<byte, TTarget>(data);
    }

    public static TTarget[] Convert<TSource, TTarget>(this Span<TSource> colors)
        where TSource : struct, IColor
        where TTarget : struct, IColor
    {
        if (colors == null) throw new ArgumentNullException(nameof(colors));

        TTarget[] buffer = new TTarget[colors.Length];
        Convert(colors, buffer.AsSpan());
        return buffer;
    }

    public static TTarget[] Convert<TSource, TTarget>(this ReadOnlySpan<TSource> colors)
        where TSource : struct, IColor
        where TTarget : struct, IColor
    {
        if (colors == null) throw new ArgumentNullException(nameof(colors));

        TTarget[] buffer = new TTarget[colors.Length];
        Convert(colors, buffer.AsSpan());
        return buffer;
    }

    public static void Convert<TSource, TTarget>(this Span<TSource> source, Span<TTarget> target)
        where TSource : struct, IColor
        where TTarget : struct, IColor
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (target.Length < source.Length) throw new ArgumentException($"Target-buffer is not big enough. {target.Length} < {source.Length}", nameof(target));

        Convert(MemoryMarshal.AsBytes(source), MemoryMarshal.AsBytes(target), TSource.ColorFormat, TTarget.ColorFormat);
    }

    public static void Convert<TSource, TTarget>(this ReadOnlySpan<TSource> source, Span<TTarget> target)
        where TSource : struct, IColor
        where TTarget : struct, IColor
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (target.Length < source.Length) throw new ArgumentException($"Target-buffer is not big enough. {target.Length} < {source.Length}", nameof(target));

        Convert(MemoryMarshal.AsBytes(source), MemoryMarshal.AsBytes(target), TSource.ColorFormat, TTarget.ColorFormat);
    }

    private static void Convert(ReadOnlySpan<byte> source, Span<byte> target, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        if (source.Length == 0) return;

        if (sourceFormat == targetFormat)
        {
            source.CopyTo(target);
            return;
        }

        switch (sourceFormat.BytesPerPixel)
        {
            case 3 when (targetFormat.BytesPerPixel == 3):
                Convert3Bytes(source, target, sourceFormat, targetFormat);
                break;

            case 4 when (targetFormat.BytesPerPixel == 4):
                Convert4Bytes(source, target, sourceFormat, targetFormat);
                break;

            case 3 when (targetFormat.BytesPerPixel == 4):
                ConvertWiden3To4Bytes(source, target, sourceFormat, targetFormat);
                break;

            case 4 when (targetFormat.BytesPerPixel == 3):
                ConvertNarrow4To3Bytes(source, target, sourceFormat, targetFormat);
                break;

            default:
                throw new NotSupportedException("Data is not of a supported valid color-type.");
        }
    }

    private static void Convert3Bytes(ReadOnlySpan<byte> source, Span<byte> target, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        const int BPP = 3;

        ReadOnlySpan<byte> sourceMapping = sourceFormat.ByteMapping;
        ReadOnlySpan<byte> targetMapping = targetFormat.ByteMapping;

        ReadOnlySpan<byte> mapping = [sourceMapping[targetMapping[0]], sourceMapping[targetMapping[1]], sourceMapping[targetMapping[2]]];
        ReadOnlySpan<byte> mask =
        [
            mapping[0],
            mapping[1],
            mapping[2],

            (byte)(mapping[0] + 3),
            (byte)(mapping[1] + 3),
            (byte)(mapping[2] + 3),

            (byte)(mapping[0] + 6),
            (byte)(mapping[1] + 6),
            (byte)(mapping[2] + 6),

            (byte)(mapping[0] + 9),
            (byte)(mapping[1] + 9),
            (byte)(mapping[2] + 9),

            (byte)(mapping[0] + 12),
            (byte)(mapping[1] + 12),
            (byte)(mapping[2] + 12),

            15
        ];
        Vector128<byte> maskVector = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(mask));

        int elements = source.Length / BPP;
        int elementsPerVector = Vector128<byte>.Count / BPP;
        int bytesPerVector = elementsPerVector * BPP;

        int chunks = elements / elementsPerVector;
        int batches = Math.Max(1, Math.Min(chunks / MIN_BATCH_SIZE, Environment.ProcessorCount));
        int batchSize = elements / batches;

        fixed (byte* fixedSourcePtr = source)
        fixed (byte* fixedTargetPtr = target)
        {
            byte* sourcePtr = fixedSourcePtr;
            byte* targetPtr = fixedTargetPtr;

            if (batches == 1)
            {
                byte* src = sourcePtr;
                byte* tar = targetPtr;

                int chunkCount = Math.Max(0, (batchSize / elementsPerVector) - 1);
                int missingElements = batchSize - (chunkCount * elementsPerVector);

                for (int i = 0; i < chunkCount; i++)
                {
                    Vector128<byte> vector = Vector128.Load(src);
                    Vector128.Shuffle(vector, maskVector).Store(tar);

                    src += bytesPerVector;
                    tar += bytesPerVector;
                }

                Span<byte> buffer = stackalloc byte[missingElements * BPP];
                for (int i = 0; i < missingElements; i++)
                {
                    buffer[(i * BPP) + 0] = src[(i * BPP) + maskVector[0]];
                    buffer[(i * BPP) + 1] = src[(i * BPP) + maskVector[1]];
                    buffer[(i * BPP) + 2] = src[(i * BPP) + maskVector[2]];
                }
                buffer.CopyTo(new Span<byte>(tar, buffer.Length));
            }
            else
            {
                Parallel.For(0, batches, Process);

                int missing = elements - (batchSize * batches);
                if (missing > 0)
                {
                    byte* missingSrc = sourcePtr + (batches * batchSize * BPP);
                    byte* missingTar = targetPtr + (batches * batchSize * BPP);

                    Span<byte> buffer = stackalloc byte[missing * BPP];
                    for (int i = 0; i < missing; i++)
                    {
                        buffer[(i * BPP) + 0] = missingSrc[(i * BPP) + maskVector[0]];
                        buffer[(i * BPP) + 1] = missingSrc[(i * BPP) + maskVector[1]];
                        buffer[(i * BPP) + 2] = missingSrc[(i * BPP) + maskVector[2]];
                    }
                    buffer.CopyTo(new Span<byte>(missingTar, buffer.Length));
                }

                void Process(int index)
                {
                    int offset = index * batchSize;
                    byte* src = sourcePtr + (offset * BPP);
                    byte* tar = targetPtr + (offset * BPP);

                    int chunkCount = Math.Max(0, (batchSize / elementsPerVector) - 1);
                    int missingElements = batchSize - (chunkCount * elementsPerVector);

                    for (int i = 0; i < chunkCount; i++)
                    {
                        Vector128<byte> vector = Vector128.Load(src);
                        Vector128.Shuffle(vector, maskVector).Store(tar);

                        src += bytesPerVector;
                        tar += bytesPerVector;
                    }

                    Span<byte> buffer = stackalloc byte[missingElements * BPP];
                    for (int i = 0; i < missingElements; i++)
                    {
                        buffer[(i * BPP) + 0] = src[(i * BPP) + maskVector[0]];
                        buffer[(i * BPP) + 1] = src[(i * BPP) + maskVector[1]];
                        buffer[(i * BPP) + 2] = src[(i * BPP) + maskVector[2]];
                    }
                    buffer.CopyTo(new Span<byte>(tar, buffer.Length));
                }
            }
        }
    }

    private static void Convert4Bytes(ReadOnlySpan<byte> source, Span<byte> target, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        const int BPP = 4;

        ReadOnlySpan<byte> sourceMapping = sourceFormat.ByteMapping;
        ReadOnlySpan<byte> targetMapping = targetFormat.ByteMapping;

        ReadOnlySpan<byte> mapping = [sourceMapping[targetMapping[0]], sourceMapping[targetMapping[1]], sourceMapping[targetMapping[2]], sourceMapping[targetMapping[3]]];
        ReadOnlySpan<byte> mask =
        [
            mapping[0],
            mapping[1],
            mapping[2],
            mapping[3],

            (byte)(mapping[0] + 4),
            (byte)(mapping[1] + 4),
            (byte)(mapping[2] + 4),
            (byte)(mapping[3] + 4),

            (byte)(mapping[0] + 8),
            (byte)(mapping[1] + 8),
            (byte)(mapping[2] + 8),
            (byte)(mapping[3] + 8),

            (byte)(mapping[0] + 12),
            (byte)(mapping[1] + 12),
            (byte)(mapping[2] + 12),
            (byte)(mapping[3] + 12),
        ];

        Vector128<byte> maskVector = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(mask));

        int elements = source.Length / BPP;
        int elementsPerVector = Vector128<byte>.Count / BPP;
        int bytesPerVector = elementsPerVector * BPP;

        int chunks = elements / elementsPerVector;
        int batches = Math.Max(1, Math.Min(chunks / MIN_BATCH_SIZE, Environment.ProcessorCount));
        int batchSize = elements / batches;

        fixed (byte* fixedSourcePtr = source)
        fixed (byte* fixedTargetPtr = target)
        {
            byte* sourcePtr = fixedSourcePtr;
            byte* targetPtr = fixedTargetPtr;

            if (batches == 1)
            {
                byte* src = sourcePtr;
                byte* tar = targetPtr;

                int chunkCount = batchSize / elementsPerVector;
                int missingElements = batchSize - (chunkCount * elementsPerVector);

                for (int i = 0; i < chunkCount; i++)
                {
                    Vector128<byte> vector = Vector128.Load(src);
                    Vector128.Shuffle(vector, maskVector).Store(tar);

                    src += bytesPerVector;
                    tar += bytesPerVector;
                }

                Span<byte> buffer = stackalloc byte[missingElements * BPP];
                for (int i = 0; i < missingElements; i++)
                {
                    buffer[(i * BPP) + 0] = src[(i * BPP) + maskVector[0]];
                    buffer[(i * BPP) + 1] = src[(i * BPP) + maskVector[1]];
                    buffer[(i * BPP) + 2] = src[(i * BPP) + maskVector[2]];
                    buffer[(i * BPP) + 3] = src[(i * BPP) + maskVector[3]];
                }
                buffer.CopyTo(new Span<byte>(tar, buffer.Length));
            }
            else
            {
                Parallel.For(0, batches, Process);

                int missing = elements - (batchSize * batches);
                if (missing > 0)
                {
                    byte* missingSrc = sourcePtr + (batches * batchSize * BPP);
                    byte* missingTar = targetPtr + (batches * batchSize * BPP);

                    Span<byte> buffer = stackalloc byte[missing * BPP];
                    for (int i = 0; i < missing; i++)
                    {
                        buffer[(i * BPP) + 0] = missingSrc[(i * BPP) + maskVector[0]];
                        buffer[(i * BPP) + 1] = missingSrc[(i * BPP) + maskVector[1]];
                        buffer[(i * BPP) + 2] = missingSrc[(i * BPP) + maskVector[2]];
                        buffer[(i * BPP) + 3] = missingSrc[(i * BPP) + maskVector[3]];
                    }
                    buffer.CopyTo(new Span<byte>(missingTar, buffer.Length));
                }

                void Process(int index)
                {
                    int offset = index * batchSize;
                    byte* src = sourcePtr + (offset * BPP);
                    byte* tar = targetPtr + (offset * BPP);

                    int chunkCount = batchSize / elementsPerVector;
                    int missingElements = batchSize - (chunkCount * elementsPerVector);

                    for (int i = 0; i < chunkCount; i++)
                    {
                        Vector128<byte> vector = Vector128.Load(src);
                        Vector128.Shuffle(vector, maskVector).Store(tar);

                        src += bytesPerVector;
                        tar += bytesPerVector;
                    }

                    Span<byte> buffer = stackalloc byte[missingElements * BPP];
                    for (int i = 0; i < missingElements; i++)
                    {
                        buffer[(i * BPP) + 0] = src[(i * BPP) + maskVector[0]];
                        buffer[(i * BPP) + 1] = src[(i * BPP) + maskVector[1]];
                        buffer[(i * BPP) + 2] = src[(i * BPP) + maskVector[2]];
                        buffer[(i * BPP) + 3] = src[(i * BPP) + maskVector[3]];
                    }
                    buffer.CopyTo(new Span<byte>(tar, buffer.Length));
                }
            }
        }
    }

    private static void ConvertWiden3To4Bytes(ReadOnlySpan<byte> source, Span<byte> target, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        const int SOURCE_BPP = 3;
        const int TARGET_BPP = 4;

        ReadOnlySpan<byte> sourceMapping = sourceFormat.ByteMapping;
        ReadOnlySpan<byte> targetMapping = targetFormat.ByteMapping;

        // DarthAffe 08.07.2024: For now alpha is the only thing to be added
        byte[] isAlpha =
        [
            targetMapping[0] == Color.A ? byte.MaxValue : (byte)0,
            targetMapping[1] == Color.A ? byte.MaxValue : (byte)0,
            targetMapping[2] == Color.A ? byte.MaxValue : (byte)0,
            targetMapping[3] == Color.A ? byte.MaxValue : (byte)0,
        ];

        ReadOnlySpan<byte> mapping =
        [
            isAlpha[0] > 0 ? (byte)0 : sourceMapping[targetMapping[0]],
            isAlpha[1] > 0 ? (byte)0 : sourceMapping[targetMapping[1]],
            isAlpha[2] > 0 ? (byte)0 : sourceMapping[targetMapping[2]],
            isAlpha[3] > 0 ? (byte)0 : sourceMapping[targetMapping[3]]
        ];

        ReadOnlySpan<byte> mask =
        [
            mapping[0],
            mapping[1],
            mapping[2],
            mapping[3],

            (byte)(mapping[0] + 3),
            (byte)(mapping[1] + 3),
            (byte)(mapping[2] + 3),
            (byte)(mapping[3] + 3),

            (byte)(mapping[0] + 6),
            (byte)(mapping[1] + 6),
            (byte)(mapping[2] + 6),
            (byte)(mapping[3] + 6),

            (byte)(mapping[0] + 9),
            (byte)(mapping[1] + 9),
            (byte)(mapping[2] + 9),
            (byte)(mapping[3] + 9),
        ];

        ReadOnlySpan<byte> alphaMask =
        [
            isAlpha[0],
            isAlpha[1],
            isAlpha[2],
            isAlpha[3],

            isAlpha[0],
            isAlpha[1],
            isAlpha[2],
            isAlpha[3],

            isAlpha[0],
            isAlpha[1],
            isAlpha[2],
            isAlpha[3],

            isAlpha[0],
            isAlpha[1],
            isAlpha[2],
            isAlpha[3],
        ];

        Vector128<byte> maskVector = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(mask));
        Vector128<byte> alphaMaskVector = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(alphaMask));

        int elements = source.Length / SOURCE_BPP;
        int targetElementsPerVector = Vector128<byte>.Count / TARGET_BPP;
        int sourceBytesPerVector = targetElementsPerVector * SOURCE_BPP;
        int targetBytesPerVector = targetElementsPerVector * TARGET_BPP;

        int chunks = elements / targetElementsPerVector;
        int batches = Math.Max(1, Math.Min(chunks / MIN_BATCH_SIZE, Environment.ProcessorCount));
        int batchSize = elements / batches;

        fixed (byte* fixedSourcePtr = source)
        fixed (byte* fixedTargetPtr = target)
        {
            byte* sourcePtr = fixedSourcePtr;
            byte* targetPtr = fixedTargetPtr;

            if (batches == 1)
            {
                byte* src = sourcePtr;
                byte* tar = targetPtr;

                int chunkCount = batchSize / targetElementsPerVector;
                int missingElements = batchSize - (chunkCount * targetElementsPerVector);

                for (int i = 0; i < chunkCount; i++)
                {
                    Vector128<byte> vector = Vector128.Load(src);
                    Vector128<byte> shuffled = Vector128.Shuffle(vector, maskVector);
                    Vector128.BitwiseOr(shuffled, alphaMaskVector).Store(tar);

                    src += sourceBytesPerVector;
                    tar += targetBytesPerVector;
                }

                for (int i = 0; i < missingElements; i++)
                {
                    tar[(i * TARGET_BPP) + 0] = Math.Max(isAlpha[0], src[(i * SOURCE_BPP) + maskVector[0]]);
                    tar[(i * TARGET_BPP) + 1] = Math.Max(isAlpha[1], src[(i * SOURCE_BPP) + maskVector[1]]);
                    tar[(i * TARGET_BPP) + 2] = Math.Max(isAlpha[2], src[(i * SOURCE_BPP) + maskVector[2]]);
                    tar[(i * TARGET_BPP) + 3] = Math.Max(isAlpha[3], src[(i * SOURCE_BPP) + maskVector[3]]);
                }
            }
            else
            {
                Parallel.For(0, batches, Process);

                int missing = elements - (batchSize * batches);
                if (missing > 0)
                {
                    byte* missingSrc = sourcePtr + (batches * batchSize * SOURCE_BPP);
                    byte* missingTar = targetPtr + (batches * batchSize * TARGET_BPP);

                    for (int i = 0; i < missing; i++)
                    {
                        missingTar[(i * TARGET_BPP) + 0] = Math.Max(isAlpha[0], missingSrc[(i * SOURCE_BPP) + maskVector[0]]);
                        missingTar[(i * TARGET_BPP) + 1] = Math.Max(isAlpha[1], missingSrc[(i * SOURCE_BPP) + maskVector[1]]);
                        missingTar[(i * TARGET_BPP) + 2] = Math.Max(isAlpha[2], missingSrc[(i * SOURCE_BPP) + maskVector[2]]);
                        missingTar[(i * TARGET_BPP) + 3] = Math.Max(isAlpha[3], missingSrc[(i * SOURCE_BPP) + maskVector[3]]);
                    }
                }

                void Process(int index)
                {
                    int offset = index * batchSize;
                    byte* src = sourcePtr + (offset * SOURCE_BPP);
                    byte* tar = targetPtr + (offset * TARGET_BPP);

                    int chunkCount = batchSize / targetElementsPerVector;
                    int missingElements = batchSize - (chunkCount * targetElementsPerVector);

                    for (int i = 0; i < chunkCount; i++)
                    {
                        Vector128<byte> vector = Vector128.Load(src);
                        Vector128<byte> shuffled = Vector128.Shuffle(vector, maskVector);
                        Vector128.BitwiseOr(shuffled, alphaMaskVector).Store(tar);

                        src += sourceBytesPerVector;
                        tar += targetBytesPerVector;
                    }

                    for (int i = 0; i < missingElements; i++)
                    {
                        tar[(i * TARGET_BPP) + 0] = Math.Max(isAlpha[0], src[(i * SOURCE_BPP) + maskVector[0]]);
                        tar[(i * TARGET_BPP) + 1] = Math.Max(isAlpha[1], src[(i * SOURCE_BPP) + maskVector[1]]);
                        tar[(i * TARGET_BPP) + 2] = Math.Max(isAlpha[2], src[(i * SOURCE_BPP) + maskVector[2]]);
                        tar[(i * TARGET_BPP) + 3] = Math.Max(isAlpha[3], src[(i * SOURCE_BPP) + maskVector[3]]);
                    }
                }
            }
        }
    }

    private static void ConvertNarrow4To3Bytes(ReadOnlySpan<byte> source, Span<byte> target, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        const int SOURCE_BPP = 4;
        const int TARGET_BPP = 3;

        ReadOnlySpan<byte> sourceMapping = sourceFormat.ByteMapping;
        ReadOnlySpan<byte> targetMapping = targetFormat.ByteMapping;

        // DarthAffe 08.07.2024: For now alpha is the only thing to be narrowed away
        ReadOnlySpan<byte> mapping = [sourceMapping[targetMapping[0]], sourceMapping[targetMapping[1]], sourceMapping[targetMapping[2]]];

        ReadOnlySpan<byte> mask =
        [
            mapping[0],
            mapping[1],
            mapping[2],

            (byte)(mapping[0] + 4),
            (byte)(mapping[1] + 4),
            (byte)(mapping[2] + 4),

            (byte)(mapping[0] + 8),
            (byte)(mapping[1] + 8),
            (byte)(mapping[2] + 8),

            (byte)(mapping[0] + 12),
            (byte)(mapping[1] + 12),
            (byte)(mapping[2] + 12),

            12,
            13,
            14,
            15
        ];

        Vector128<byte> maskVector = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(mask));

        int elements = source.Length / SOURCE_BPP;
        int sourceElementsPerVector = Vector128<byte>.Count / SOURCE_BPP;
        int sourceBytesPerVector = sourceElementsPerVector * SOURCE_BPP;
        int targetBytesPerVector = sourceElementsPerVector * TARGET_BPP;

        int chunks = elements / sourceElementsPerVector;
        int batches = Math.Max(1, Math.Min(chunks / MIN_BATCH_SIZE, Environment.ProcessorCount));
        int batchSize = elements / batches;

        fixed (byte* fixedSourcePtr = source)
        fixed (byte* fixedTargetPtr = target)
        {
            byte* sourcePtr = fixedSourcePtr;
            byte* targetPtr = fixedTargetPtr;

            if (batches == 1)
            {
                byte* src = sourcePtr;
                byte* tar = targetPtr;

                int chunkCount = Math.Max(0, (batchSize / sourceElementsPerVector) - 1); // DarthAffe 08.07.2024: -1 since we don't have enough space to copy a full target vector for the last set
                int missingElements = batchSize - (chunkCount * sourceElementsPerVector);

                for (int i = 0; i < chunkCount; i++)
                {
                    Vector128<byte> vector = Vector128.Load(src);
                    Vector128.Shuffle(vector, maskVector).Store(tar);

                    src += sourceBytesPerVector;
                    tar += targetBytesPerVector;
                }

                for (int i = 0; i < missingElements; i++)
                {
                    tar[(i * TARGET_BPP) + 0] = src[(i * SOURCE_BPP) + mapping[0]];
                    tar[(i * TARGET_BPP) + 1] = src[(i * SOURCE_BPP) + mapping[1]];
                    tar[(i * TARGET_BPP) + 2] = src[(i * SOURCE_BPP) + mapping[2]];
                }
            }
            else
            {
                Parallel.For(0, batches, Process);

                int missing = elements - (batchSize * batches);
                if (missing > 0)
                {
                    byte* missingSrc = sourcePtr + (batches * batchSize * SOURCE_BPP);
                    byte* missingTar = targetPtr + (batches * batchSize * TARGET_BPP);

                    for (int i = 0; i < missing; i++)
                    {
                        missingTar[(i * TARGET_BPP) + 0] = missingSrc[(i * SOURCE_BPP) + maskVector[0]];
                        missingTar[(i * TARGET_BPP) + 1] = missingSrc[(i * SOURCE_BPP) + maskVector[1]];
                        missingTar[(i * TARGET_BPP) + 2] = missingSrc[(i * SOURCE_BPP) + maskVector[2]];
                    }
                }

                void Process(int index)
                {
                    int offset = index * batchSize;
                    byte* src = sourcePtr + (offset * SOURCE_BPP);
                    byte* tar = targetPtr + (offset * TARGET_BPP);

                    int chunkCount = Math.Max(0, (batchSize / sourceElementsPerVector) - 1); // DarthAffe 08.07.2024: -1 since we don't have enough space to copy a full target vector for the last set
                    int missingElements = batchSize - (chunkCount * sourceElementsPerVector);

                    for (int i = 0; i < chunkCount; i++)
                    {
                        Vector128<byte> vector = Vector128.Load(src);
                        Vector128.Shuffle(vector, maskVector).Store(tar);

                        src += sourceBytesPerVector;
                        tar += targetBytesPerVector;
                    }

                    for (int i = 0; i < missingElements; i++)
                    {
                        tar[(i * TARGET_BPP) + 0] = src[(i * SOURCE_BPP) + maskVector[0]];
                        tar[(i * TARGET_BPP) + 1] = src[(i * SOURCE_BPP) + maskVector[1]];
                        tar[(i * TARGET_BPP) + 2] = src[(i * SOURCE_BPP) + maskVector[2]];
                    }
                }
            }
        }
    }

    #endregion
}
