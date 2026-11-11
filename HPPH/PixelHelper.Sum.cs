using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace HPPH;

public static partial class PixelHelper
{
    #region Constants

    private const int SUM_FLUSH_INTERVAL = 256;

    #endregion

    #region Properties & Fields

    /// <summary>
    /// 64 zero bytes followed by 64 0xFF bytes: a vector loaded at offset 64 - k clears the first k bytes.
    /// </summary>
    private static ReadOnlySpan<byte> TailMask =>
    [
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255
    ];

    #endregion

    #region Methods

    public static ColorSum Sum(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<SumOperation, ColorSum>(default);
    }

    public static ColorSum Sum<T>(this IImage<T> image)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().Sum();
    }

    public static ColorSum Sum<T>(this RefImage<T> image)
        where T : unmanaged, IColor<T>
        => T.ToColorSum(SumBytes(image));

    public static ColorSum Sum<T>(this ReadOnlySpan<T> colors)
        where T : unmanaged, IColor<T>
        => T.ToColorSum(SumBytes<T>(MemoryMarshal.AsBytes(colors)));

    internal static Generic4LongData SumBytes<T>(RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        if ((image.Width == 0) || (image.Height == 0)) return default;

        GetRows(image, out nuint rowBytes, out nuint stride, out nuint rows);
        return T.ColorFormat.BytesPerPixel switch
        {
            3 => Sum3Bytes(ref Unsafe.AsRef(in image.GetPinnableReference()), rowBytes, stride, rows),
            4 => Sum4Bytes(ref Unsafe.AsRef(in image.GetPinnableReference()), rowBytes, stride, rows),
            _ => throw new NotSupportedException("Data is not of a supported valid color-type.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Generic4LongData SumBytes<T>(ReadOnlySpan<byte> data)
        where T : unmanaged, IColor<T>
        => T.ColorFormat.BytesPerPixel switch
        {
            3 => Sum(MemoryMarshal.Cast<byte, Generic3ByteData>(data)),
            4 => Sum(MemoryMarshal.Cast<byte, Generic4ByteData>(data)),
            _ => throw new NotSupportedException("Data is not of a supported valid color-type.")
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GetRows<T>(RefImage<T> image, out nuint rowBytes, out nuint stride, out nuint rows)
        where T : unmanaged, IColor<T>
    {
        rowBytes = (nuint)image.Width * (nuint)T.ColorFormat.BytesPerPixel;
        stride = (nuint)image.Stride;
        rows = (nuint)image.Height;

        // Contiguous images become one row.
        if (image.IsContiguous)
        {
            rowBytes *= rows;
            stride = rowBytes;
            rows = 1;
        }
    }

    internal static Generic4LongData Sum(ReadOnlySpan<Generic3ByteData> data)
    {
        if (data.Length == 0) return default;

        nuint length = (nuint)data.Length * 3;
        return Sum3Bytes(ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(data)), length, length, 1);
    }

    internal static Generic4LongData Sum(ReadOnlySpan<Generic4ByteData> data)
    {
        if (data.Length == 0) return default;

        nuint length = (nuint)data.Length * 4;
        return Sum4Bytes(ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(data)), length, length, 1);
    }

    private static Generic4LongData Sum3Bytes(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        // Avx512 measured noticeably slower on Zen4 thatn Avx2, for Avx we don't have a good portable option
        if (Avx2.IsSupported && (rowBytes >= 96))
            return Sum3Bytes256(ref data, rowBytes, stride, rows);

        return Sum3BytesScalar(ref data, rowBytes, stride, rows);
    }

    private static Generic4LongData Sum3Bytes256(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        const int block = 96;

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

        Vector256<byte> mask1 = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(blendMask1));
        Vector256<byte> mask2 = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(blendMask2));
        Vector256<byte> mask3 = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(blendMask3));

        Vector256<long> b1SumVector = Vector256<long>.Zero;
        Vector256<long> b2SumVector = Vector256<long>.Zero;
        Vector256<long> b3SumVector = Vector256<long>.Zero;

        long b1Sum = 0;
        long b2Sum = 0;
        long b3Sum = 0;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);

            Vector256<byte> data1;
            Vector256<byte> data2;
            Vector256<byte> data3;

            nuint i = 0;
            for (; (i + block) <= rowBytes; i += block)
            {
                data1 = Vector256.LoadUnsafe(ref rowStart, i);
                data2 = Vector256.LoadUnsafe(ref rowStart, i + 32);
                data3 = Vector256.LoadUnsafe(ref rowStart, i + 64);

                b1SumVector += SumChannel3(data1, data2, data3, mask1, mask3);
                b2SumVector += SumChannel3(data1, data2, data3, mask2, mask1);
                b3SumVector += SumChannel3(data1, data2, data3, mask3, mask2);
            }

            if (i < rowBytes)
            {
                // The last block of the row, without the bytes the loop already counted. It starts on a pixel since rowBytes and the block are multiples of 3.
                nuint start = rowBytes - block;
                nint counted = (nint)(i - start);
                data1 = Vector256.LoadUnsafe(ref rowStart, start) & GetTailMask256(counted);
                data2 = Vector256.LoadUnsafe(ref rowStart, start + 32) & GetTailMask256(counted - 32);
                data3 = Vector256.LoadUnsafe(ref rowStart, start + 64) & GetTailMask256(counted - 64);

                b1SumVector += SumChannel3(data1, data2, data3, mask1, mask3);
                b2SumVector += SumChannel3(data1, data2, data3, mask2, mask1);
                b3SumVector += SumChannel3(data1, data2, data3, mask3, mask2);
            }
        }

        b1Sum = Vector256.Sum(b1SumVector);
        b2Sum = Vector256.Sum(b2SumVector);
        b3Sum = Vector256.Sum(b3SumVector);

        return new Generic4LongData(b1Sum, b2Sum, b3Sum, (long)(rows * (rowBytes / 3)) * byte.MaxValue);
    }

    private static Generic4LongData Sum3BytesScalar(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        long b1Sum = 0;
        long b2Sum = 0;
        long b3Sum = 0;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);
            for (nuint i = 0; i < rowBytes; i += 3)
            {
                b1Sum += Unsafe.Add(ref rowStart, i);
                b2Sum += Unsafe.Add(ref rowStart, i + 1);
                b3Sum += Unsafe.Add(ref rowStart, i + 2);
            }
        }

        return new Generic4LongData(b1Sum, b2Sum, b3Sum, (long)(rows * (rowBytes / 3)) * byte.MaxValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<long> SumChannel3(Vector256<byte> data1, Vector256<byte> data2, Vector256<byte> data3, Vector256<byte> fromData1, Vector256<byte> fromData3)
        => Avx2.SumAbsoluteDifferences(Avx2.BlendVariable(Avx2.BlendVariable(data2, data1, fromData1), data3, fromData3), Vector256<byte>.Zero).AsInt64();

    private static Generic4LongData Sum4Bytes(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        if (Vector512.IsHardwareAccelerated && (rowBytes >= 64)) return Sum4Bytes512(ref data, rowBytes, stride, rows);
        if (Vector256.IsHardwareAccelerated && (rowBytes >= 32)) return Sum4Bytes256(ref data, rowBytes, stride, rows);
        if (Vector128.IsHardwareAccelerated && (rowBytes >= 16)) return Sum4Bytes128(ref data, rowBytes, stride, rows);

        return Sum4BytesScalar(ref data, rowBytes, stride, rows);
    }

    private static Generic4LongData Sum4Bytes512(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        nuint size = (nuint)Vector512<byte>.Count;

        Vector512<ushort> lowBytes = Vector512.Create((ushort)0x00FF);

        Vector512<ushort> sum13 = Vector512<ushort>.Zero;
        Vector512<ushort> sum24 = Vector512<ushort>.Zero;

        Vector512<ulong> total1 = Vector512<ulong>.Zero;
        Vector512<ulong> total2 = Vector512<ulong>.Zero;
        Vector512<ulong> total3 = Vector512<ulong>.Zero;
        Vector512<ulong> total4 = Vector512<ulong>.Zero;

        int pending = 0;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);
            nuint i = 0;

            // Blocks of 4 vectors, so the flush check runs once per block.
            for (; (i + (4 * size)) <= rowBytes; i += 4 * size)
            {
                if (pending > (SUM_FLUSH_INTERVAL - 4))
                {
                    Flush(ref sum13, ref sum24, ref total1, ref total2, ref total3, ref total4);
                    pending = 0;
                }

                Vector512<ushort> v0 = Vector512.LoadUnsafe(ref rowStart, i).AsUInt16();
                Vector512<ushort> v1 = Vector512.LoadUnsafe(ref rowStart, i + size).AsUInt16();
                Vector512<ushort> v2 = Vector512.LoadUnsafe(ref rowStart, i + (2 * size)).AsUInt16();
                Vector512<ushort> v3 = Vector512.LoadUnsafe(ref rowStart, i + (3 * size)).AsUInt16();

                sum13 += ((v0 & lowBytes) + (v1 & lowBytes)) + ((v2 & lowBytes) + (v3 & lowBytes));
                sum24 += ((v0 >>> 8) + (v1 >>> 8)) + ((v2 >>> 8) + (v3 >>> 8));
                pending += 4;
            }

            // Single vectors, then the row's last vector without the bytes already counted.
            while (i < rowBytes)
            {
                if (pending >= SUM_FLUSH_INTERVAL)
                {
                    Flush(ref sum13, ref sum24, ref total1, ref total2, ref total3, ref total4);
                    pending = 0;
                }

                Vector512<ushort> vector;
                if ((i + size) <= rowBytes)
                {
                    vector = Vector512.LoadUnsafe(ref rowStart, i).AsUInt16();
                    i += size;
                }
                else
                {
                    nuint start = rowBytes - size;
                    vector = (Vector512.LoadUnsafe(ref rowStart, start) & GetTailMask512((nint)(i - start))).AsUInt16();
                    i = rowBytes;
                }

                sum13 += vector & lowBytes;
                sum24 += vector >>> 8;
                pending++;
            }
        }

        Flush(ref sum13, ref sum24, ref total1, ref total2, ref total3, ref total4);

        return new Generic4LongData((long)Vector512.Sum(total1), (long)Vector512.Sum(total2), (long)Vector512.Sum(total3), (long)Vector512.Sum(total4));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Flush(ref Vector512<ushort> sum13, ref Vector512<ushort> sum24,
                              ref Vector512<ulong> total1, ref Vector512<ulong> total2,
                              ref Vector512<ulong> total3, ref Vector512<ulong> total4)
    {
        Vector512<uint> lowHalf = Vector512.Create(0xFFFFu);

        Vector512<uint> s13 = sum13.AsUInt32();
        Vector512<uint> s24 = sum24.AsUInt32();

        Vector512<uint> c1 = s13 & lowHalf;
        Vector512<uint> c3 = s13 >>> 16;
        Vector512<uint> c2 = s24 & lowHalf;
        Vector512<uint> c4 = s24 >>> 16;

        total1 += Vector512.WidenLower(c1) + Vector512.WidenUpper(c1);
        total2 += Vector512.WidenLower(c2) + Vector512.WidenUpper(c2);
        total3 += Vector512.WidenLower(c3) + Vector512.WidenUpper(c3);
        total4 += Vector512.WidenLower(c4) + Vector512.WidenUpper(c4);

        sum13 = Vector512<ushort>.Zero;
        sum24 = Vector512<ushort>.Zero;
    }

    private static Generic4LongData Sum4Bytes256(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        nuint size = (nuint)Vector256<byte>.Count;

        Vector256<ushort> lowBytes = Vector256.Create((ushort)0x00FF);

        Vector256<ushort> sum13 = Vector256<ushort>.Zero;
        Vector256<ushort> sum24 = Vector256<ushort>.Zero;

        Vector256<ulong> total1 = Vector256<ulong>.Zero;
        Vector256<ulong> total2 = Vector256<ulong>.Zero;
        Vector256<ulong> total3 = Vector256<ulong>.Zero;
        Vector256<ulong> total4 = Vector256<ulong>.Zero;

        int pending = 0;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);
            nuint i = 0;

            // Blocks of 4 vectors, so the flush check runs once per block.
            for (; (i + (4 * size)) <= rowBytes; i += 4 * size)
            {
                if (pending > (SUM_FLUSH_INTERVAL - 4))
                {
                    Flush(ref sum13, ref sum24, ref total1, ref total2, ref total3, ref total4);
                    pending = 0;
                }

                Vector256<ushort> v0 = Vector256.LoadUnsafe(ref rowStart, i).AsUInt16();
                Vector256<ushort> v1 = Vector256.LoadUnsafe(ref rowStart, i + size).AsUInt16();
                Vector256<ushort> v2 = Vector256.LoadUnsafe(ref rowStart, i + (2 * size)).AsUInt16();
                Vector256<ushort> v3 = Vector256.LoadUnsafe(ref rowStart, i + (3 * size)).AsUInt16();

                sum13 += ((v0 & lowBytes) + (v1 & lowBytes)) + ((v2 & lowBytes) + (v3 & lowBytes));
                sum24 += ((v0 >>> 8) + (v1 >>> 8)) + ((v2 >>> 8) + (v3 >>> 8));
                pending += 4;
            }

            // Single vectors, then the row's last vector without the bytes already counted.
            while (i < rowBytes)
            {
                if (pending >= SUM_FLUSH_INTERVAL)
                {
                    Flush(ref sum13, ref sum24, ref total1, ref total2, ref total3, ref total4);
                    pending = 0;
                }

                Vector256<ushort> vector;
                if ((i + size) <= rowBytes)
                {
                    vector = Vector256.LoadUnsafe(ref rowStart, i).AsUInt16();
                    i += size;
                }
                else
                {
                    nuint start = rowBytes - size;
                    vector = (Vector256.LoadUnsafe(ref rowStart, start) & GetTailMask256((nint)(i - start))).AsUInt16();
                    i = rowBytes;
                }

                sum13 += vector & lowBytes;
                sum24 += vector >>> 8;
                pending++;
            }
        }

        Flush(ref sum13, ref sum24, ref total1, ref total2, ref total3, ref total4);

        return new Generic4LongData((long)Vector256.Sum(total1), (long)Vector256.Sum(total2), (long)Vector256.Sum(total3), (long)Vector256.Sum(total4));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Flush(ref Vector256<ushort> sum13, ref Vector256<ushort> sum24,
                              ref Vector256<ulong> total1, ref Vector256<ulong> total2,
                              ref Vector256<ulong> total3, ref Vector256<ulong> total4)
    {
        Vector256<uint> lowHalf = Vector256.Create(0xFFFFu);

        Vector256<uint> s13 = sum13.AsUInt32();
        Vector256<uint> s24 = sum24.AsUInt32();

        Vector256<uint> c1 = s13 & lowHalf;
        Vector256<uint> c3 = s13 >>> 16;
        Vector256<uint> c2 = s24 & lowHalf;
        Vector256<uint> c4 = s24 >>> 16;

        total1 += Vector256.WidenLower(c1) + Vector256.WidenUpper(c1);
        total2 += Vector256.WidenLower(c2) + Vector256.WidenUpper(c2);
        total3 += Vector256.WidenLower(c3) + Vector256.WidenUpper(c3);
        total4 += Vector256.WidenLower(c4) + Vector256.WidenUpper(c4);

        sum13 = Vector256<ushort>.Zero;
        sum24 = Vector256<ushort>.Zero;
    }

    private static Generic4LongData Sum4Bytes128(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        nuint size = (nuint)Vector128<byte>.Count;

        Vector128<ushort> lowBytes = Vector128.Create((ushort)0x00FF);

        Vector128<ushort> sum13 = Vector128<ushort>.Zero;
        Vector128<ushort> sum24 = Vector128<ushort>.Zero;

        Vector128<ulong> total1 = Vector128<ulong>.Zero;
        Vector128<ulong> total2 = Vector128<ulong>.Zero;
        Vector128<ulong> total3 = Vector128<ulong>.Zero;
        Vector128<ulong> total4 = Vector128<ulong>.Zero;

        int pending = 0;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);
            nuint i = 0;

            // Blocks of 4 vectors, so the flush check runs once per block.
            for (; (i + (4 * size)) <= rowBytes; i += 4 * size)
            {
                if (pending > (SUM_FLUSH_INTERVAL - 4))
                {
                    Flush(ref sum13, ref sum24, ref total1, ref total2, ref total3, ref total4);
                    pending = 0;
                }

                Vector128<ushort> v0 = Vector128.LoadUnsafe(ref rowStart, i).AsUInt16();
                Vector128<ushort> v1 = Vector128.LoadUnsafe(ref rowStart, i + size).AsUInt16();
                Vector128<ushort> v2 = Vector128.LoadUnsafe(ref rowStart, i + (2 * size)).AsUInt16();
                Vector128<ushort> v3 = Vector128.LoadUnsafe(ref rowStart, i + (3 * size)).AsUInt16();

                sum13 += ((v0 & lowBytes) + (v1 & lowBytes)) + ((v2 & lowBytes) + (v3 & lowBytes));
                sum24 += ((v0 >>> 8) + (v1 >>> 8)) + ((v2 >>> 8) + (v3 >>> 8));
                pending += 4;
            }

            // Single vectors, then the row's last vector without the bytes already counted.
            while (i < rowBytes)
            {
                if (pending >= SUM_FLUSH_INTERVAL)
                {
                    Flush(ref sum13, ref sum24, ref total1, ref total2, ref total3, ref total4);
                    pending = 0;
                }

                Vector128<ushort> vector;
                if ((i + size) <= rowBytes)
                {
                    vector = Vector128.LoadUnsafe(ref rowStart, i).AsUInt16();
                    i += size;
                }
                else
                {
                    nuint start = rowBytes - size;
                    vector = (Vector128.LoadUnsafe(ref rowStart, start) & GetTailMask128((nint)(i - start))).AsUInt16();
                    i = rowBytes;
                }

                sum13 += vector & lowBytes;
                sum24 += vector >>> 8;
                pending++;
            }
        }

        Flush(ref sum13, ref sum24, ref total1, ref total2, ref total3, ref total4);

        return new Generic4LongData((long)Vector128.Sum(total1), (long)Vector128.Sum(total2), (long)Vector128.Sum(total3), (long)Vector128.Sum(total4));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Flush(ref Vector128<ushort> sum13, ref Vector128<ushort> sum24,
                              ref Vector128<ulong> total1, ref Vector128<ulong> total2,
                              ref Vector128<ulong> total3, ref Vector128<ulong> total4)
    {
        Vector128<uint> lowHalf = Vector128.Create(0xFFFFu);
        Vector128<uint> s13 = sum13.AsUInt32(), s24 = sum24.AsUInt32();
        Vector128<uint> c1 = s13 & lowHalf, c3 = s13 >>> 16, c2 = s24 & lowHalf, c4 = s24 >>> 16;

        total1 += Vector128.WidenLower(c1) + Vector128.WidenUpper(c1);
        total2 += Vector128.WidenLower(c2) + Vector128.WidenUpper(c2);
        total3 += Vector128.WidenLower(c3) + Vector128.WidenUpper(c3);
        total4 += Vector128.WidenLower(c4) + Vector128.WidenUpper(c4);

        sum13 = Vector128<ushort>.Zero;
        sum24 = Vector128<ushort>.Zero;
    }

    private static Generic4LongData Sum4BytesScalar(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        long b1Sum = 0;
        long b2Sum = 0;
        long b3Sum = 0;
        long b4Sum = 0;
        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);
            for (nuint i = 0; i < rowBytes; i += 4)
            {
                b1Sum += Unsafe.Add(ref rowStart, i);
                b2Sum += Unsafe.Add(ref rowStart, i + 1);
                b3Sum += Unsafe.Add(ref rowStart, i + 2);
                b4Sum += Unsafe.Add(ref rowStart, i + 3);
            }
        }

        return new Generic4LongData(b1Sum, b2Sum, b3Sum, b4Sum);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<byte> GetTailMask512(nint cleared)
        => Vector512.LoadUnsafe(ref MemoryMarshal.GetReference(TailMask), (nuint)(64 - Math.Clamp(cleared, 0, 64)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<byte> GetTailMask256(nint cleared)
        => Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(TailMask), (nuint)(64 - Math.Clamp(cleared, 0, 64)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> GetTailMask128(nint cleared)
        => Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(TailMask), (nuint)(64 - Math.Clamp(cleared, 0, 64)));

    #endregion

    private readonly struct SumOperation : IImageOperation<ColorSum>
    {
        public ColorSum Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image.Sum();
    }
}
