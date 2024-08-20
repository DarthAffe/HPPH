using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

public static unsafe partial class PixelHelper
{
    #region Methods

    public static IMinMax MinMax(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        IColorFormat colorFormat = image.ColorFormat;

        if (colorFormat.BytesPerPixel == 3)
        {
            if (image.Height == 0) return colorFormat.ToMinMax(new Generic3ByteMinMax(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue));
            if (image.Height == 1) return colorFormat.ToMinMax(MinMax(MemoryMarshal.Cast<byte, Generic3ByteData>(image.Rows[0].AsByteSpan())));

            Generic3ByteMinMax result = new(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue);
            for (int y = 0; y < image.Height; y++)
                result = MinMax(MemoryMarshal.Cast<byte, Generic3ByteData>(image.Rows[y].AsByteSpan()),
                                result.B1Min, result.B1Max,
                                result.B2Min, result.B2Max,
                                result.B3Min, result.B3Max);

            return colorFormat.ToMinMax(result);
        }

        if (colorFormat.BytesPerPixel == 4)
        {
            if (image.Height == 0) return colorFormat.ToMinMax(new Generic4ByteMinMax(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue));
            if (image.Height == 1) return colorFormat.ToMinMax(MinMax(MemoryMarshal.Cast<byte, Generic4ByteData>(image.Rows[0].AsByteSpan())));

            Generic4ByteMinMax result = new(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue);
            for (int y = 0; y < image.Height; y++)
                result = MinMax(MemoryMarshal.Cast<byte, Generic4ByteData>(image.Rows[y].AsByteSpan()),
                                result.B1Min, result.B1Max,
                                result.B2Min, result.B2Max,
                                result.B3Min, result.B3Max,
                                result.B4Min, result.B4Max);

            return colorFormat.ToMinMax(result);
        }

        throw new NotSupportedException("Data is not of a supported valid color-type.");
    }

    public static IMinMax MinMax<T>(this IImage<T> image)
        where T : struct, IColor
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));

        return image.AsRefImage().MinMax();
    }

    public static IMinMax MinMax<T>(this RefImage<T> image)
        where T : struct, IColor
    {
        IColorFormat colorFormat = T.ColorFormat;

        if (colorFormat.BytesPerPixel == 3)
        {
            if (image.Height == 0) return colorFormat.ToMinMax(new Generic3ByteMinMax(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue));
            if (image.Height == 1) return colorFormat.ToMinMax(MinMax(MemoryMarshal.Cast<byte, Generic3ByteData>(image.Rows[0].AsByteSpan())));

            Generic3ByteMinMax result = new(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue);
            for (int y = 0; y < image.Height; y++)
                result = MinMax(MemoryMarshal.Cast<byte, Generic3ByteData>(image.Rows[y].AsByteSpan()),
                                result.B1Min, result.B1Max,
                                result.B2Min, result.B2Max,
                                result.B3Min, result.B3Max);

            return colorFormat.ToMinMax(result);
        }

        if (colorFormat.BytesPerPixel == 4)
        {
            if (image.Height == 0) return colorFormat.ToMinMax(new Generic4ByteMinMax(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue));
            if (image.Height == 1) return colorFormat.ToMinMax(MinMax(MemoryMarshal.Cast<byte, Generic4ByteData>(image.Rows[0].AsByteSpan())));

            Generic4ByteMinMax result = new(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue);
            for (int y = 0; y < image.Height; y++)
                result = MinMax(MemoryMarshal.Cast<byte, Generic4ByteData>(image.Rows[y].AsByteSpan()),
                                result.B1Min, result.B1Max,
                                result.B2Min, result.B2Max,
                                result.B3Min, result.B3Max,
                                result.B4Min, result.B4Max);

            return colorFormat.ToMinMax(result);
        }

        throw new NotSupportedException("Data is not of a supported valid color-type.");
    }

    public static IMinMax MinMax<T>(this Span<T> colors)
        where T : struct, IColor
    {
        if (colors == null) throw new ArgumentNullException(nameof(colors));

        IColorFormat colorFormat = T.ColorFormat;
        return colorFormat.BytesPerPixel switch
        {
            3 => colorFormat.ToMinMax(MinMax(MemoryMarshal.Cast<T, Generic3ByteData>(colors))),
            4 => colorFormat.ToMinMax(MinMax(MemoryMarshal.Cast<T, Generic4ByteData>(colors))),
            _ => throw new NotSupportedException("Data is not of a supported valid color-type.")
        };
    }

    public static IMinMax MinMax<T>(this ReadOnlySpan<T> colors)
        where T : struct, IColor
    {
        if (colors == null) throw new ArgumentNullException(nameof(colors));

        IColorFormat colorFormat = T.ColorFormat;
        return colorFormat.BytesPerPixel switch
        {
            3 => colorFormat.ToMinMax(MinMax(MemoryMarshal.Cast<T, Generic3ByteData>(colors))),
            4 => colorFormat.ToMinMax(MinMax(MemoryMarshal.Cast<T, Generic4ByteData>(colors))),
            _ => throw new NotSupportedException("Data is not of a supported valid color-type.")
        };
    }

    private static Generic3ByteMinMax MinMax(ReadOnlySpan<Generic3ByteData> data) => MinMax(data, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Generic3ByteMinMax MinMax(ReadOnlySpan<Generic3ByteData> data, byte minB1, byte maxB1, byte minB2, byte maxB2, byte minB3, byte maxB3)
    {
        const int BYTES_PER_COLOR = 3;
        int elementsPerVector = Vector<byte>.Count / BYTES_PER_COLOR;

        int chunks;
        if (Vector.IsHardwareAccelerated && ((chunks = data.Length / elementsPerVector) > 1))
        {
            int bytesPerVector = elementsPerVector * BYTES_PER_COLOR;
            int missingElements = data.Length - (chunks * elementsPerVector);

            Vector<byte> max = Vector<byte>.Zero;
            Vector<byte> min = new(byte.MaxValue);

            ReadOnlySpan<byte> colorBytes = MemoryMarshal.AsBytes(data);
            fixed (byte* colorPtr = colorBytes)
            {
                for (int i = 0; i < chunks; i++)
                {
                    Vector<byte> vector = Vector.Load(colorPtr + (i * bytesPerVector));

                    max = Vector.Max(max, vector);
                    min = Vector.Min(min, vector);
                }
            }

            for (int i = 0; i < bytesPerVector; i += BYTES_PER_COLOR)
            {
                minB1 = Math.Min(minB1, min[i]);
                minB2 = Math.Min(minB2, min[i + 1]);
                minB3 = Math.Min(minB3, min[i + 2]);

                maxB1 = Math.Max(maxB1, max[i]);
                maxB2 = Math.Max(maxB2, max[i + 1]);
                maxB3 = Math.Max(maxB3, max[i + 2]);
            }

            for (int i = 0; i < missingElements; i++)
            {
                Generic3ByteData d = data[^(i + 1)];

                minB1 = Math.Min(minB1, d.B1);
                minB2 = Math.Min(minB2, d.B2);
                minB3 = Math.Min(minB3, d.B3);

                maxB1 = Math.Max(maxB1, d.B1);
                maxB2 = Math.Max(maxB2, d.B2);
                maxB3 = Math.Max(maxB3, d.B3);
            }
        }
        else
        {
            foreach (Generic3ByteData d in data)
            {
                minB1 = Math.Min(minB1, d.B1);
                minB2 = Math.Min(minB2, d.B2);
                minB3 = Math.Min(minB3, d.B3);

                maxB1 = Math.Max(maxB1, d.B1);
                maxB2 = Math.Max(maxB2, d.B2);
                maxB3 = Math.Max(maxB3, d.B3);
            }
        }

        return new Generic3ByteMinMax(minB1, maxB1, minB2, maxB2, minB3, maxB3);
    }


    private static Generic4ByteMinMax MinMax(ReadOnlySpan<Generic4ByteData> data) => MinMax(data, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Generic4ByteMinMax MinMax(ReadOnlySpan<Generic4ByteData> data, byte minB1, byte maxB1, byte minB2, byte maxB2, byte minB3, byte maxB3, byte minB4, byte maxB4)
    {
        const int BYTES_PER_COLOR = 4;
        int elementsPerVector = Vector<byte>.Count / BYTES_PER_COLOR;

        int chunks;
        if (Vector.IsHardwareAccelerated && ((chunks = data.Length / elementsPerVector) > 1))
        {
            int bytesPerVector = elementsPerVector * BYTES_PER_COLOR;
            int missingElements = data.Length - (chunks * elementsPerVector);

            Vector<byte> max = Vector<byte>.Zero;
            Vector<byte> min = new(byte.MaxValue);

            ReadOnlySpan<byte> colorBytes = MemoryMarshal.AsBytes(data);
            fixed (byte* colorPtr = colorBytes)
            {
                for (int i = 0; i < chunks; i++)
                {
                    Vector<byte> vector = Vector.Load(colorPtr + (i * bytesPerVector));

                    max = Vector.Max(max, vector);
                    min = Vector.Min(min, vector);
                }
            }

            for (int i = 0; i < bytesPerVector; i += BYTES_PER_COLOR)
            {
                minB1 = Math.Min(minB1, min[i]);
                minB2 = Math.Min(minB2, min[i + 1]);
                minB3 = Math.Min(minB3, min[i + 2]);
                minB4 = Math.Min(minB4, min[i + 3]);

                maxB1 = Math.Max(maxB1, max[i]);
                maxB2 = Math.Max(maxB2, max[i + 1]);
                maxB3 = Math.Max(maxB3, max[i + 2]);
                maxB4 = Math.Max(maxB4, max[i + 3]);
            }

            for (int i = 0; i < missingElements; i++)
            {
                Generic4ByteData d = data[^(i + 1)];

                minB1 = Math.Min(minB1, d.B1);
                minB2 = Math.Min(minB2, d.B2);
                minB3 = Math.Min(minB3, d.B3);
                minB4 = Math.Min(minB4, d.B4);

                maxB1 = Math.Max(maxB1, d.B1);
                maxB2 = Math.Max(maxB2, d.B2);
                maxB3 = Math.Max(maxB3, d.B3);
                maxB4 = Math.Max(maxB4, d.B4);
            }
        }
        else
        {
            foreach (Generic4ByteData d in data)
            {
                minB1 = Math.Min(minB1, d.B1);
                minB2 = Math.Min(minB2, d.B2);
                minB3 = Math.Min(minB3, d.B3);
                minB4 = Math.Min(minB4, d.B4);

                maxB1 = Math.Max(maxB1, d.B1);
                maxB2 = Math.Max(maxB2, d.B2);
                maxB3 = Math.Max(maxB3, d.B3);
                maxB4 = Math.Max(maxB4, d.B4);
            }
        }

        return new Generic4ByteMinMax(minB1, maxB1, minB2, maxB2, minB3, maxB3, minB4, maxB4);
    }

    #endregion
}
