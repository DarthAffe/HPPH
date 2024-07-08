using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HPPH;

public static unsafe partial class PixelHelper
{
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
        Convert<TSource, TTarget>(colors, buffer.AsSpan());
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

        ConvertSameBpp(source, target, mask, 3);
    }

    private static void Convert4Bytes(ReadOnlySpan<byte> source, Span<byte> target, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
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

        ConvertSameBpp(source, target, mask, 4);
    }

    private static void ConvertSameBpp(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> mask, int bpp)
    {
        int elementsPerVector = Vector128<byte>.Count / bpp;
        int bytesPerVector = elementsPerVector * bpp;

        int chunks = source.Length / bytesPerVector;
        Vector128<byte> maskVector = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(mask));

        int missingElements = (source.Length - (chunks * bytesPerVector)) / bpp;

        fixed (byte* sourcePtr = source)
        fixed (byte* targetPtr = target)
        {
            byte* src = sourcePtr;
            byte* tar = targetPtr;

            for (int i = 0; i < chunks; i++)
            {
                Vector128<byte> vector = Vector128.Load(src);
                Vector128.Shuffle(vector, maskVector).Store(tar);

                src += bytesPerVector;
                tar += bytesPerVector;
            }

            Span<byte> buffer = stackalloc byte[missingElements * bpp]; // DarthAffe 08.07.2024: This is fine as it's always < 16 bytes
            for (int j = 0; j < buffer.Length; j++)
                buffer[j] = src[mask[j]];

            buffer.CopyTo(new Span<byte>(tar, buffer.Length));
        }
    }

    private static void ConvertWiden3To4Bytes(ReadOnlySpan<byte> source, Span<byte> target, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        ReadOnlySpan<byte> sourceMapping = sourceFormat.ByteMapping;
        ReadOnlySpan<byte> targetMapping = targetFormat.ByteMapping;

        // DarthAffe 08.07.2024: For now alpha is the only thing to be added
        Span<byte> isAlpha =
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

        int sourceBpp = sourceFormat.BytesPerPixel;
        int targetBpp = targetFormat.BytesPerPixel;

        int targetElementsPerVector = Vector128<byte>.Count / targetBpp;
        int targetBytesPerVector = targetElementsPerVector * targetBpp;
        int sourceBytesPerVector = targetElementsPerVector * sourceBpp;

        int chunks = (source.Length / sourceBytesPerVector);
        Vector128<byte> maskVector = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(mask));
        Vector128<byte> alphaMaskVector = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(alphaMask));

        int missingElements = (source.Length - (chunks * sourceBytesPerVector)) / sourceBpp;

        fixed (byte* sourcePtr = source)
        fixed (byte* targetPtr = target)
        {
            byte* src = sourcePtr;
            byte* tar = targetPtr;

            for (int i = 0; i < chunks; i++)
            {
                Vector128<byte> vector = Vector128.Load(src);
                Vector128<byte> shuffled = Vector128.Shuffle(vector, maskVector);
                Vector128.BitwiseOr(shuffled, alphaMaskVector).Store(tar);

                src += sourceBytesPerVector;
                tar += targetBytesPerVector;
            }

            Span<byte> buffer = stackalloc byte[missingElements * targetBpp]; // DarthAffe 08.07.2024: This is fine as it's always < 16 bytes
            for (int i = 0; i < missingElements; i++)
                for (int j = 0; j < targetBpp; j++)
                    buffer[(i * targetBpp) + j] = Math.Max(isAlpha[j], src[(i * sourceBpp) + mask[j]]);

            buffer.CopyTo(new Span<byte>(tar, buffer.Length));
        }
    }

    private static void ConvertNarrow4To3Bytes(ReadOnlySpan<byte> source, Span<byte> target, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
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

        int sourceBpp = sourceFormat.BytesPerPixel;
        int targetBpp = targetFormat.BytesPerPixel;

        int sourceElementsPerVector = Vector128<byte>.Count / sourceBpp;
        int sourceBytesPerVector = sourceElementsPerVector * sourceBpp;
        int targetBytesPerVector = sourceElementsPerVector * targetBpp;

        int chunks = (source.Length / sourceBytesPerVector) - 1; // DarthAffe 08.07.2024: -1 since we don't have enough space to copy a full target vector for the last set
        Vector128<byte> maskVector = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(mask));

        int missingElements = (source.Length - (chunks * sourceBytesPerVector)) / sourceBpp;

        fixed (byte* sourcePtr = source)
        fixed (byte* targetPtr = target)
        {
            byte* src = sourcePtr;
            byte* tar = targetPtr;

            for (int i = 0; i < chunks; i++)
            {
                Vector128<byte> vector = Vector128.Load(src);
                Vector128.Shuffle(vector, maskVector).Store(tar);

                src += sourceBytesPerVector;
                tar += targetBytesPerVector;
            }

            Span<byte> buffer = stackalloc byte[missingElements * targetBpp]; // DarthAffe 08.07.2024: This is fine as it's always < 24 bytes
            for (int i = 0; i < missingElements; i++)
                for (int j = 0; j < targetBpp; j++)
                    buffer[(i * targetBpp) + j] = src[(i * sourceBpp) + mask[j]];

            buffer.CopyTo(new Span<byte>(tar, buffer.Length));
        }
    }

    #endregion
}
