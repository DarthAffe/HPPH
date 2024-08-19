using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Runtime.CompilerServices;

namespace HPPH;

public static unsafe partial class PixelHelper
{
    #region Methods

    public static ISum Sum(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        IColorFormat colorFormat = image.ColorFormat;

        if (image.Height == 0) return colorFormat.ToSum(new Generic4LongData(0, 0, 0, 0));
        if (image.Height == 1) return colorFormat.ToSum(colorFormat.Sum(image.Rows[0].AsByteSpan()));

        Vector256<long> result = Vector256<long>.Zero;
        for (int y = 0; y < image.Height; y++)
        {
            Generic4LongData rowSum = colorFormat.Sum(image.Rows[y].AsByteSpan());
            Vector256<long> rowSumVector = Vector256.LoadUnsafe(ref Unsafe.As<Generic4LongData, long>(ref rowSum));
            result = Vector256.Add(result, rowSumVector);
        }

        return colorFormat.ToSum(Unsafe.BitCast<Vector256<long>, Generic4LongData>(result));
    }

    public static ISum Sum<T>(this IImage<T> image)
        where T : struct, IColor
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));

        return image.AsRefImage().Sum();
    }

    public static ISum Sum<T>(this RefImage<T> image)
        where T : struct, IColor
    {
        IColorFormat colorFormat = T.ColorFormat;

        if (image.Height == 0) return colorFormat.ToSum(new Generic4LongData(0, 0, 0, 0));
        if (image.Height == 1) return colorFormat.ToSum(colorFormat.Sum(image.Rows[0].AsByteSpan()));

        Vector256<long> result = Vector256<long>.Zero;
        for (int y = 0; y < image.Height; y++)
        {
            Generic4LongData rowSum = colorFormat.Sum(image.Rows[y].AsByteSpan());
            Vector256<long> rowSumVector = Vector256.LoadUnsafe(ref Unsafe.As<Generic4LongData, long>(ref rowSum));
            result = Vector256.Add(result, rowSumVector);
        }

        return colorFormat.ToSum(Unsafe.BitCast<Vector256<long>, Generic4LongData>(result));
    }

    public static ISum Sum<T>(this ReadOnlySpan<T> colors)
        where T : struct, IColor
    {
        IColorFormat colorFormat = T.ColorFormat;
        return colorFormat.ToSum(colorFormat.Sum(MemoryMarshal.AsBytes(colors)));
    }

    public static ISum Sum<T>(this Span<T> colors)
        where T : struct, IColor
    {
        IColorFormat colorFormat = T.ColorFormat;
        return colorFormat.ToSum(colorFormat.Sum(MemoryMarshal.AsBytes(colors)));
    }
    
    internal static Generic4LongData Sum(ReadOnlySpan<Generic3ByteData> data)
    {
        long b1Sum = 0, b2Sum = 0, b3Sum = 0;

        const int ELEMENTS_PER_VECTOR = 32;
        int chunks;
        if (Avx2.IsSupported && ((chunks = data.Length / ELEMENTS_PER_VECTOR) > 0))
        {
            ReadOnlySpan<byte> blendMask1 =
            [
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                255, 0
            ];

            ReadOnlySpan<byte> blendMask2 =
            [
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255
            ];

            ReadOnlySpan<byte> blendMask3 =
            [
                0, 0, 255,
                0, 0, 255,
                0, 0, 255,
                0, 0, 255,
                0, 0, 255,
                0, 0, 255,
                0, 0, 255,
                0, 0, 255,
                0, 0, 255,
                0, 0, 255,
                0, 0
            ];

            Vector256<byte> blend1MaskVector = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(blendMask1));
            Vector256<byte> blend2MaskVector = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(blendMask2));
            Vector256<byte> blend3MaskVector = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(blendMask3));

            Vector256<long> b1SumVector = Vector256<long>.Zero;
            Vector256<long> b2SumVector = Vector256<long>.Zero;
            Vector256<long> b3SumVector = Vector256<long>.Zero;

            int missingElements = data.Length - (chunks * ELEMENTS_PER_VECTOR);

            ReadOnlySpan<byte> dataBytes = MemoryMarshal.AsBytes(data);
            fixed (byte* bytePtr = dataBytes)
            {
                for (int i = 0; i < chunks; i++)
                {
                    byte* basePtr = bytePtr + (i * 96);
                    Vector256<byte> data1 = Vector256.Load(basePtr);
                    Vector256<byte> data2 = Vector256.Load(basePtr + 32);
                    Vector256<byte> data3 = Vector256.Load(basePtr + 64);

                    Vector256<byte> vectorB1Blend1 = Avx2.BlendVariable(data2, data1, blend1MaskVector);
                    Vector256<byte> vectorB2Blend1 = Avx2.BlendVariable(data2, data1, blend2MaskVector);
                    Vector256<byte> vectorB3Blend1 = Avx2.BlendVariable(data2, data1, blend3MaskVector);

                    Vector256<byte> vectorB1Blend2 = Avx2.BlendVariable(vectorB1Blend1, data3, blend3MaskVector);
                    Vector256<byte> vectorB2Blend2 = Avx2.BlendVariable(vectorB2Blend1, data3, blend1MaskVector);
                    Vector256<byte> vectorB3Blend2 = Avx2.BlendVariable(vectorB3Blend1, data3, blend2MaskVector);

                    Vector256<long> sumB1 = Avx2.SumAbsoluteDifferences(vectorB1Blend2, Vector256<byte>.Zero).AsInt64();
                    Vector256<long> sumB2 = Avx2.SumAbsoluteDifferences(vectorB2Blend2, Vector256<byte>.Zero).AsInt64();
                    Vector256<long> sumB3 = Avx2.SumAbsoluteDifferences(vectorB3Blend2, Vector256<byte>.Zero).AsInt64();

                    b1SumVector = Avx2.Add(b1SumVector, sumB1);
                    b2SumVector = Avx2.Add(b2SumVector, sumB2);
                    b3SumVector = Avx2.Add(b3SumVector, sumB3);
                }
            }

            b1Sum = b1SumVector[0] + b1SumVector[1] + b1SumVector[2] + b1SumVector[3];
            b2Sum = b2SumVector[0] + b2SumVector[1] + b2SumVector[2] + b2SumVector[3];
            b3Sum = b3SumVector[0] + b3SumVector[1] + b3SumVector[2] + b3SumVector[3];

            for (int i = 0; i < missingElements; i++)
            {
                Generic3ByteData d = data[^(i + 1)];
                b1Sum += d.B1;
                b2Sum += d.B2;
                b3Sum += d.B3;
            }
        }
        else
        {
            foreach (Generic3ByteData d in data)
            {
                b1Sum += d.B1;
                b2Sum += d.B2;
                b3Sum += d.B3;
            }
        }

        return new Generic4LongData(b1Sum, b2Sum, b3Sum, data.Length * 255);
    }

    internal static Generic4LongData Sum(ReadOnlySpan<Generic4ByteData> data)
    {
        long b1Sum, b2Sum, b3Sum, b4Sum;
        int i = 0;

        if (Avx2.IsSupported && (data.Length >= 8))
        {
            ReadOnlySpan<byte> avx2ShuffleMask =
            [
                // Byte 1
                15, 11, 7, 3,
                // Byte 2
                14, 10, 6, 2,
                // Byte 3
                13, 9, 5, 1,
                // Byte 4
                12, 8, 4, 0
            ];

            ReadOnlySpan<int> avx2ControlData =
            [
                // Byte 1
                7, 3,
                // Byte 2
                6, 2,
                // Byte 3
                5, 1,
                // Byte 4
                4, 0
            ];

            Vector256<int> controlVector = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(avx2ControlData));

            Vector256<long> rgbaSum64 = Vector256<long>.Zero;

            ReadOnlySpan<byte> dataBytes = MemoryMarshal.AsBytes(data);
            fixed (byte* bytePtr = dataBytes)
            fixed (byte* maskPtr = avx2ShuffleMask)
            {
                Vector256<byte> avx2ShuffleMaskVector = Avx2.BroadcastVector128ToVector256(maskPtr);

                for (int j = 0; j < (data.Length / 8); j++, i += 8)
                {
                    Vector256<byte> chunk = Vector256.Load(bytePtr + (i * 4));
                    Vector256<byte> deinterleaved = Avx2.Shuffle(chunk, avx2ShuffleMaskVector);
                    Vector256<int> deinterleaved2 = Avx2.PermuteVar8x32(deinterleaved.AsInt32(), controlVector);
                    Vector256<long> sum = Avx2.SumAbsoluteDifferences(deinterleaved2.AsByte(), Vector256<byte>.Zero).AsInt64();
                    rgbaSum64 = Avx2.Add(rgbaSum64, sum);
                }
            }

            Vector128<long> b1B2Sum = rgbaSum64.GetLower();
            Vector128<long> b3B4Sum = rgbaSum64.GetUpper();

            b1Sum = b1B2Sum.GetLower()[0];
            b2Sum = b1B2Sum.GetUpper()[0];
            b3Sum = b3B4Sum.GetLower()[0];
            b4Sum = b3B4Sum.GetUpper()[0];
        }
        else
        {
            b1Sum = b2Sum = b3Sum = b4Sum = 0;
        }

        for (; i < data.Length; i++)
        {
            b1Sum += data[i].B1;
            b2Sum += data[i].B2;
            b3Sum += data[i].B3;
            b4Sum += data[i].B4;
        }

        return new Generic4LongData(b1Sum, b2Sum, b3Sum, b4Sum);
    }

    #endregion
}
