using System.Buffers;
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

        int dataLength = image.SizeInBytes;
        byte[] array = ArrayPool<byte>.Shared.Rent(dataLength);
        Span<byte> buffer = array.AsSpan()[..dataLength];
        try
        {
            image.CopyTo(buffer);
            return image.ColorFormat.MinMax(buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public static IMinMax MinMax<T>(this RefImage<T> image)
        where T : struct, IColor
    {
        int dataLength = image.Width * image.Height;
        T[] array = ArrayPool<T>.Shared.Rent(dataLength);
        Span<T> buffer = array.AsSpan()[..(dataLength)];
        try
        {
            image.CopyTo(buffer);
            return MinMax<T>(buffer);
        }
        finally
        {
            ArrayPool<T>.Shared.Return(array);
        }
    }
    public static IMinMax MinMax<T>(this Span<T> colors)
        where T : struct, IColor
        => T.ColorFormat.MinMax(MemoryMarshal.AsBytes(colors));

    public static IMinMax MinMax<T>(this ReadOnlySpan<T> colors)
        where T : struct, IColor
        => T.ColorFormat.MinMax(MemoryMarshal.AsBytes(colors));

    internal static IMinMax MinMax<T, TMinMax>(ReadOnlySpan<T> colors)
        where T : struct, IColor
        where TMinMax : struct, IMinMax
    {
        if (colors == null) throw new ArgumentNullException(nameof(colors));

        return T.ColorFormat.BytesPerPixel switch
        {
            3 => Unsafe.BitCast<Generic3ByteMinMax, TMinMax>(MinMax(MemoryMarshal.Cast<T, Generic3ByteData>(colors))),
            4 => Unsafe.BitCast<Generic4ByteMinMax, TMinMax>(MinMax(MemoryMarshal.Cast<T, Generic4ByteData>(colors))),
            _ => throw new NotSupportedException("Data is not of a supported valid color-type.")
        };
    }

    private static Generic3ByteMinMax MinMax(ReadOnlySpan<Generic3ByteData> data)
    {
        byte minB1 = byte.MaxValue, minB2 = byte.MaxValue, minB3 = byte.MaxValue;
        byte maxB1 = byte.MinValue, maxB2 = byte.MinValue, maxB3 = byte.MinValue;

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

    private static Generic4ByteMinMax MinMax(ReadOnlySpan<Generic4ByteData> data)
    {
        byte minB1 = byte.MaxValue, minB2 = byte.MaxValue, minB3 = byte.MaxValue, minB4 = byte.MaxValue;
        byte maxB1 = byte.MinValue, maxB2 = byte.MinValue, maxB3 = byte.MinValue, maxB4 = byte.MinValue;

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
