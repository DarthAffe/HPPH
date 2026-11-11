using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HPPH;

public static partial class PixelHelper
{
    #region Constants

    private static readonly Generic3ByteMinMax EMPTY_3 = new(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue);
    private static readonly Generic4ByteMinMax EMPTY_4 = new(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue);

    #endregion

    #region Properties & Fields

    // 0x80 x 16, 0..15, 0x80 x 16. Loaded at 16 + k it shifts down by k, at 16 - k it shifts up by k.
    private static ReadOnlySpan<byte> ShiftTable =>
    [
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80
    ];

    // 0xFF x 16, 0 x 16, 0xFF x 16, matching the 0 bytes of the shifts above.
    private static ReadOnlySpan<byte> FillTable =>
    [
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255
    ];

    #endregion

    #region Methods

    /// <summary>
    /// Calculates the per-channel minimum and maximum of the pixels of <paramref name="image"/>.
    /// </summary>
    /// <param name="image">The image to calculate the minimum and maximum for.</param>
    /// <returns>The minimum and maximum of each channel.</returns>
    public static ColorMinMax MinMax(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<MinMaxOperation, ColorMinMax>(default);
    }

    /// <inheritdoc cref="MinMax(IImage)"/>
    public static ColorMinMax MinMax<T>(this Image<T> image)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().MinMax();
    }

    /// <inheritdoc cref="MinMax(IImage)"/>
    public static ColorMinMax MinMax<T>(this RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        bool empty = (image.Width == 0) || (image.Height == 0);

        GetRows(image, out nuint rowBytes, out nuint stride, out nuint rows);
        return T.ColorFormat.BytesPerPixel switch
        {
            3 => T.ToColorMinMax(empty ? EMPTY_3 : MinMax3Bytes(ref Unsafe.AsRef(in image.GetPinnableReference()), rowBytes, stride, rows)),
            4 => T.ToColorMinMax(empty ? EMPTY_4 : MinMax4Bytes(ref Unsafe.AsRef(in image.GetPinnableReference()), rowBytes, stride, rows)),
            _ => throw new NotSupportedException("Data is not of a supported valid color-type.")
        };
    }

    /// <summary>
    /// Calculates the per-channel minimum and maximum of <paramref name="colors"/>.
    /// </summary>
    /// <param name="colors">The colors to calculate the minimum and maximum for.</param>
    /// <returns>The minimum and maximum of each channel.</returns>
    public static ColorMinMax MinMax<T>(this ReadOnlySpan<T> colors)
        where T : unmanaged, IColor<T>
    {
        ReadOnlySpan<byte> data = MemoryMarshal.AsBytes(colors);
        nuint length = (nuint)data.Length;

        return T.ColorFormat.BytesPerPixel switch
        {
            3 => T.ToColorMinMax(length == 0 ? EMPTY_3 : MinMax3Bytes(ref MemoryMarshal.GetReference(data), length, length, 1)),
            4 => T.ToColorMinMax(length == 0 ? EMPTY_4 : MinMax4Bytes(ref MemoryMarshal.GetReference(data), length, length, 1)),
            _ => throw new NotSupportedException("Data is not of a supported valid color-type.")
        };
    }

    private static Generic3ByteMinMax MinMax3Bytes(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        if (Vector512.IsHardwareAccelerated && (rowBytes >= 192)) return MinMax3Bytes512(ref data, rowBytes, stride, rows);
        if (Vector256.IsHardwareAccelerated && (rowBytes >= 96)) return MinMax3Bytes256(ref data, rowBytes, stride, rows);
        if (Vector128.IsHardwareAccelerated && (rowBytes >= 48)) return MinMax3Bytes128(ref data, rowBytes, stride, rows);
        return MinMax3BytesScalar(ref data, rowBytes, stride, rows);
    }

    private static Generic3ByteMinMax MinMax3Bytes512(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        Vector512<byte> min0 = Vector512<byte>.AllBitsSet;
        Vector512<byte> min1 = Vector512<byte>.AllBitsSet;
        Vector512<byte> min2 = Vector512<byte>.AllBitsSet;

        Vector512<byte> max0 = Vector512<byte>.Zero;
        Vector512<byte> max1 = Vector512<byte>.Zero;
        Vector512<byte> max2 = Vector512<byte>.Zero;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);

            for (nuint i = 0; i < rowBytes;)
            {
                // The last block might overlap.
                nuint offset = (i + 192) <= rowBytes ? i : rowBytes - 192;
                i = offset + 192;

                Vector512<byte> v0 = Vector512.LoadUnsafe(ref rowStart, offset);
                Vector512<byte> v1 = Vector512.LoadUnsafe(ref rowStart, offset + 64);
                Vector512<byte> v2 = Vector512.LoadUnsafe(ref rowStart, offset + 128);

                min0 = Vector512.Min(min0, v0);
                min1 = Vector512.Min(min1, v1);
                min2 = Vector512.Min(min2, v2);

                max0 = Vector512.Max(max0, v0);
                max1 = Vector512.Max(max1, v1);
                max2 = Vector512.Max(max2, v2);
            }
        }

        // Slot k starts at byte 64k, its 16-byte quarter q at (k + q) % 3.
        Vector128<byte> min = FoldMin3(Vector128.Min(Vector128.Min(Quarter(min0, 0), Quarter(min0, 3)), Vector128.Min(Quarter(min1, 2), Quarter(min2, 1))),
                                       Vector128.Min(Vector128.Min(Quarter(min0, 1), Quarter(min1, 0)), Vector128.Min(Quarter(min1, 3), Quarter(min2, 2))),
                                       Vector128.Min(Vector128.Min(Quarter(min0, 2), Quarter(min1, 1)), Vector128.Min(Quarter(min2, 0), Quarter(min2, 3))));

        Vector128<byte> max = FoldMax3(Vector128.Max(Vector128.Max(Quarter(max0, 0), Quarter(max0, 3)), Vector128.Max(Quarter(max1, 2), Quarter(max2, 1))),
                                       Vector128.Max(Vector128.Max(Quarter(max0, 1), Quarter(max1, 0)), Vector128.Max(Quarter(max1, 3), Quarter(max2, 2))),
                                       Vector128.Max(Vector128.Max(Quarter(max0, 2), Quarter(max1, 1)), Vector128.Max(Quarter(max2, 0), Quarter(max2, 3))));

        return new Generic3ByteMinMax(min[0], max[0], min[1], max[1], min[2], max[2]);
    }

    private static Generic3ByteMinMax MinMax3Bytes256(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        Vector256<byte> min0 = Vector256<byte>.AllBitsSet;
        Vector256<byte> min1 = Vector256<byte>.AllBitsSet;
        Vector256<byte> min2 = Vector256<byte>.AllBitsSet;

        Vector256<byte> max0 = Vector256<byte>.Zero;
        Vector256<byte> max1 = Vector256<byte>.Zero;
        Vector256<byte> max2 = Vector256<byte>.Zero;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);

            for (nuint i = 0; i < rowBytes;)
            {
                // The last block might overlap.
                nuint offset = (i + 96) <= rowBytes ? i : rowBytes - 96;
                i = offset + 96;

                Vector256<byte> v0 = Vector256.LoadUnsafe(ref rowStart, offset);
                Vector256<byte> v1 = Vector256.LoadUnsafe(ref rowStart, offset + 32);
                Vector256<byte> v2 = Vector256.LoadUnsafe(ref rowStart, offset + 64);

                min0 = Vector256.Min(min0, v0);
                min1 = Vector256.Min(min1, v1);
                min2 = Vector256.Min(min2, v2);

                max0 = Vector256.Max(max0, v0);
                max1 = Vector256.Max(max1, v1);
                max2 = Vector256.Max(max2, v2);
            }
        }

        // Slot k starts at byte 32k, so its 16-byte halves start at channel (32k) % 3 and (32k + 16) % 3.
        Vector128<byte> min = FoldMin3(Vector128.Min(min0.GetLower(), min1.GetUpper()),
                                       Vector128.Min(min0.GetUpper(), min2.GetLower()),
                                       Vector128.Min(min1.GetLower(), min2.GetUpper()));
        Vector128<byte> max = FoldMax3(Vector128.Max(max0.GetLower(), max1.GetUpper()),
                                       Vector128.Max(max0.GetUpper(), max2.GetLower()),
                                       Vector128.Max(max1.GetLower(), max2.GetUpper()));

        return new Generic3ByteMinMax(min[0], max[0], min[1], max[1], min[2], max[2]);
    }

    private static Generic3ByteMinMax MinMax3Bytes128(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        Vector128<byte> min0 = Vector128<byte>.AllBitsSet;
        Vector128<byte> min1 = Vector128<byte>.AllBitsSet;
        Vector128<byte> min2 = Vector128<byte>.AllBitsSet;

        Vector128<byte> max0 = Vector128<byte>.Zero;
        Vector128<byte> max1 = Vector128<byte>.Zero;
        Vector128<byte> max2 = Vector128<byte>.Zero;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);

            for (nuint i = 0; i < rowBytes;)
            {
                // The last block might overlap.
                nuint offset = (i + 48) <= rowBytes ? i : rowBytes - 48;
                i = offset + 48;

                Vector128<byte> v0 = Vector128.LoadUnsafe(ref rowStart, offset);
                Vector128<byte> v1 = Vector128.LoadUnsafe(ref rowStart, offset + 16);
                Vector128<byte> v2 = Vector128.LoadUnsafe(ref rowStart, offset + 32);

                min0 = Vector128.Min(min0, v0);
                min1 = Vector128.Min(min1, v1);
                min2 = Vector128.Min(min2, v2);

                max0 = Vector128.Max(max0, v0);
                max1 = Vector128.Max(max1, v1);
                max2 = Vector128.Max(max2, v2);
            }
        }

        Vector128<byte> min = FoldMin3(min0, min1, min2);
        Vector128<byte> max = FoldMax3(max0, max1, max2);

        return new Generic3ByteMinMax(min[0], max[0], min[1], max[1], min[2], max[2]);
    }

    private static Generic3ByteMinMax MinMax3BytesScalar(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        byte minB1 = byte.MaxValue;
        byte minB2 = byte.MaxValue;
        byte minB3 = byte.MaxValue;

        byte maxB1 = byte.MinValue;
        byte maxB2 = byte.MinValue;
        byte maxB3 = byte.MinValue;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);
            for (nuint i = 0; i < rowBytes; i += 3)
            {
                byte b1 = Unsafe.Add(ref rowStart, i);
                byte b2 = Unsafe.Add(ref rowStart, i + 1);
                byte b3 = Unsafe.Add(ref rowStart, i + 2);

                minB1 = Math.Min(minB1, b1);
                minB2 = Math.Min(minB2, b2);
                minB3 = Math.Min(minB3, b3);

                maxB1 = Math.Max(maxB1, b1);
                maxB2 = Math.Max(maxB2, b2);
                maxB3 = Math.Max(maxB3, b3);
            }
        }

        return new Generic3ByteMinMax(minB1, maxB1, minB2, maxB2, minB3, maxB3);
    }

    private static Generic4ByteMinMax MinMax4Bytes(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        if (Vector512.IsHardwareAccelerated && (rowBytes >= 64)) return MinMax4Bytes512(ref data, rowBytes, stride, rows);
        if (Vector256.IsHardwareAccelerated && (rowBytes >= 32)) return MinMax4Bytes256(ref data, rowBytes, stride, rows);
        if (Vector128.IsHardwareAccelerated && (rowBytes >= 16)) return MinMax4Bytes128(ref data, rowBytes, stride, rows);
        return MinMax4BytesScalar(ref data, rowBytes, stride, rows);
    }

    private static Generic4ByteMinMax MinMax4Bytes512(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        nuint size = (nuint)Vector512<byte>.Count;

        Vector512<byte> min = Vector512<byte>.AllBitsSet;
        Vector512<byte> max = Vector512<byte>.Zero;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);
            nuint i = 0;

            for (; (i + (4 * size)) <= rowBytes; i += 4 * size)
            {
                Vector512<byte> v0 = Vector512.LoadUnsafe(ref rowStart, i);
                Vector512<byte> v1 = Vector512.LoadUnsafe(ref rowStart, i + size);
                Vector512<byte> v2 = Vector512.LoadUnsafe(ref rowStart, i + (2 * size));
                Vector512<byte> v3 = Vector512.LoadUnsafe(ref rowStart, i + (3 * size));

                min = Vector512.Min(min, Vector512.Min(Vector512.Min(v0, v1), Vector512.Min(v2, v3)));
                max = Vector512.Max(max, Vector512.Max(Vector512.Max(v0, v1), Vector512.Max(v2, v3)));
            }

            while (i < rowBytes)
            {
                // The last block might overlap.
                nuint offset = (i + size) <= rowBytes ? i : rowBytes - size;
                i = offset + size;

                Vector512<byte> vector = Vector512.LoadUnsafe(ref rowStart, offset);
                min = Vector512.Min(min, vector);
                max = Vector512.Max(max, vector);
            }
        }

        Vector256<byte> min256 = Vector256.Min(min.GetLower(), min.GetUpper());
        Vector256<byte> max256 = Vector256.Max(max.GetLower(), max.GetUpper());

        return Fold4(Vector128.Min(min256.GetLower(), min256.GetUpper()), Vector128.Max(max256.GetLower(), max256.GetUpper()));
    }

    private static Generic4ByteMinMax MinMax4Bytes256(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        nuint size = (nuint)Vector256<byte>.Count;

        Vector256<byte> min = Vector256<byte>.AllBitsSet;
        Vector256<byte> max = Vector256<byte>.Zero;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);
            nuint i = 0;

            for (; (i + (4 * size)) <= rowBytes; i += 4 * size)
            {
                Vector256<byte> v0 = Vector256.LoadUnsafe(ref rowStart, i);
                Vector256<byte> v1 = Vector256.LoadUnsafe(ref rowStart, i + size);
                Vector256<byte> v2 = Vector256.LoadUnsafe(ref rowStart, i + (2 * size));
                Vector256<byte> v3 = Vector256.LoadUnsafe(ref rowStart, i + (3 * size));

                min = Vector256.Min(min, Vector256.Min(Vector256.Min(v0, v1), Vector256.Min(v2, v3)));
                max = Vector256.Max(max, Vector256.Max(Vector256.Max(v0, v1), Vector256.Max(v2, v3)));
            }

            while (i < rowBytes)
            {
                // The last block might overlap.
                nuint offset = (i + size) <= rowBytes ? i : rowBytes - size;
                i = offset + size;

                Vector256<byte> vector = Vector256.LoadUnsafe(ref rowStart, offset);
                min = Vector256.Min(min, vector);
                max = Vector256.Max(max, vector);
            }
        }

        return Fold4(Vector128.Min(min.GetLower(), min.GetUpper()), Vector128.Max(max.GetLower(), max.GetUpper()));
    }

    private static Generic4ByteMinMax MinMax4Bytes128(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        nuint size = (nuint)Vector128<byte>.Count;

        Vector128<byte> min = Vector128<byte>.AllBitsSet;
        Vector128<byte> max = Vector128<byte>.Zero;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);
            nuint i = 0;

            for (; (i + (4 * size)) <= rowBytes; i += 4 * size)
            {
                Vector128<byte> v0 = Vector128.LoadUnsafe(ref rowStart, i);
                Vector128<byte> v1 = Vector128.LoadUnsafe(ref rowStart, i + size);
                Vector128<byte> v2 = Vector128.LoadUnsafe(ref rowStart, i + (2 * size));
                Vector128<byte> v3 = Vector128.LoadUnsafe(ref rowStart, i + (3 * size));

                min = Vector128.Min(min, Vector128.Min(Vector128.Min(v0, v1), Vector128.Min(v2, v3)));
                max = Vector128.Max(max, Vector128.Max(Vector128.Max(v0, v1), Vector128.Max(v2, v3)));
            }

            while (i < rowBytes)
            {
                // The last block might overlap.
                nuint offset = (i + size) <= rowBytes ? i : rowBytes - size;
                i = offset + size;

                Vector128<byte> vector = Vector128.LoadUnsafe(ref rowStart, offset);
                min = Vector128.Min(min, vector);
                max = Vector128.Max(max, vector);
            }
        }

        return Fold4(min, max);
    }

    private static Generic4ByteMinMax MinMax4BytesScalar(ref byte data, nuint rowBytes, nuint stride, nuint rows)
    {
        byte minB1 = byte.MaxValue;
        byte minB2 = byte.MaxValue;
        byte minB3 = byte.MaxValue;
        byte minB4 = byte.MaxValue;

        byte maxB1 = byte.MinValue;
        byte maxB2 = byte.MinValue;
        byte maxB3 = byte.MinValue;
        byte maxB4 = byte.MinValue;

        for (nuint row = 0; row < rows; row++)
        {
            ref byte rowStart = ref Unsafe.Add(ref data, row * stride);
            for (nuint i = 0; i < rowBytes; i += 4)
            {
                byte b1 = Unsafe.Add(ref rowStart, i), b2 = Unsafe.Add(ref rowStart, i + 1), b3 = Unsafe.Add(ref rowStart, i + 2), b4 = Unsafe.Add(ref rowStart, i + 3);
                minB1 = Math.Min(minB1, b1);
                minB2 = Math.Min(minB2, b2);
                minB3 = Math.Min(minB3, b3);
                minB4 = Math.Min(minB4, b4);
                maxB1 = Math.Max(maxB1, b1);
                maxB2 = Math.Max(maxB2, b2);
                maxB3 = Math.Max(maxB3, b3);
                maxB4 = Math.Max(maxB4, b4);
            }
        }

        return new Generic4ByteMinMax(minB1, maxB1, minB2, maxB2, minB3, maxB3, minB4, maxB4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Quarter(Vector512<byte> vector, int quarter)
        => quarter switch
        {
            0 => vector.GetLower().GetLower(),
            1 => vector.GetLower().GetUpper(),
            2 => vector.GetUpper().GetLower(),
            _ => vector.GetUpper().GetUpper()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Generic4ByteMinMax Fold4(Vector128<byte> min, Vector128<byte> max)
    {
        min = Vector128.Min(min, Vector128.Shuffle(min.AsUInt32(), Vector128.Create(2u, 3, 0, 1)).AsByte());
        max = Vector128.Max(max, Vector128.Shuffle(max.AsUInt32(), Vector128.Create(2u, 3, 0, 1)).AsByte());
        min = Vector128.Min(min, Vector128.Shuffle(min.AsUInt32(), Vector128.Create(1u, 0, 3, 2)).AsByte());
        max = Vector128.Max(max, Vector128.Shuffle(max.AsUInt32(), Vector128.Create(1u, 0, 3, 2)).AsByte());

        return new Generic4ByteMinMax(min[0], max[0], min[1], max[1], min[2], max[2], min[3], max[3]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> FoldMin3(Vector128<byte> phase0, Vector128<byte> phase1, Vector128<byte> phase2)
    {
        Vector128<byte> m = Vector128.Min(phase0, Vector128.Min(ShiftDown(phase1, 2) | FillLast(2), ShiftUp(phase1, 1) | FillFirst(1)));
        m = Vector128.Min(m, Vector128.Min(ShiftDown(phase2, 1) | FillLast(1), ShiftUp(phase2, 2) | FillFirst(2)));

        // Now byte j belongs to channel j % 3: fold j + 3, j + 6, j + 12 into j.
        m = Vector128.Min(m, ShiftDown(m, 3) | FillLast(3));
        m = Vector128.Min(m, ShiftDown(m, 6) | FillLast(6));

        return Vector128.Min(m, ShiftDown(m, 12) | FillLast(12));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> FoldMax3(Vector128<byte> phase0, Vector128<byte> phase1, Vector128<byte> phase2)
    {
        Vector128<byte> m = Vector128.Max(phase0, Vector128.Max(ShiftDown(phase1, 2), ShiftUp(phase1, 1)));
        m = Vector128.Max(m, Vector128.Max(ShiftDown(phase2, 1), ShiftUp(phase2, 2)));

        m = Vector128.Max(m, ShiftDown(m, 3));
        m = Vector128.Max(m, ShiftDown(m, 6));

        return Vector128.Max(m, ShiftDown(m, 12));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> ShiftDown(Vector128<byte> vector, int count)
        => Vector128.ShuffleNative(vector, Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(ShiftTable), (nuint)(16 + count)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> ShiftUp(Vector128<byte> vector, int count)
        => Vector128.ShuffleNative(vector, Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(ShiftTable), (nuint)(16 - count)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> FillLast(int count)
        => Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(FillTable), (nuint)(16 + count));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> FillFirst(int count)
        => Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(FillTable), (nuint)(16 - count));

    #endregion

    private readonly struct MinMaxOperation : IImageOperation<ColorMinMax>
    {
        public ColorMinMax Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image.MinMax();
    }
}
