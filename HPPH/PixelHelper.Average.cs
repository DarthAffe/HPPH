using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

public static partial class PixelHelper
{
    #region Methods

    public static IColor Average(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        int dataLength = image.SizeInBytes;
        byte[] array = ArrayPool<byte>.Shared.Rent(dataLength);
        Span<byte> buffer = array.AsSpan()[..dataLength];
        try
        {
            image.CopyTo(buffer);
            return image.ColorFormat.Average(buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public static T Average<T>(this IImage<T> image)
        where T : struct, IColor
        => image.AsRefImage().Average();

    public static T Average<T>(this RefImage<T> image)
        where T : struct, IColor
    {
        int dataLength = image.Width * image.Height;
        T[] array = ArrayPool<T>.Shared.Rent(dataLength);
        Span<T> buffer = array.AsSpan()[..(dataLength)];
        try
        {
            image.CopyTo(buffer);
            return Average(buffer);
        }
        finally
        {
            ArrayPool<T>.Shared.Return(array);
        }
    }

    public static T Average<T>(this Span<T> colors)
        where T : struct, IColor
    {
        if (colors == null) throw new ArgumentNullException(nameof(colors));

        return T.ColorFormat.BytesPerPixel switch
        {
            3 => Unsafe.BitCast<Generic3ByteData, T>(Average(MemoryMarshal.Cast<T, Generic3ByteData>(colors))),
            4 => Unsafe.BitCast<Generic4ByteData, T>(Average(MemoryMarshal.Cast<T, Generic4ByteData>(colors))),
            _ => throw new NotSupportedException("Data is not of a supported valid color-type.")
        };
    }

    public static T Average<T>(this ReadOnlySpan<T> colors)
        where T : struct, IColor
    {
        if (colors == null) throw new ArgumentNullException(nameof(colors));

        return T.ColorFormat.BytesPerPixel switch
        {
            3 => Unsafe.BitCast<Generic3ByteData, T>(Average(MemoryMarshal.Cast<T, Generic3ByteData>(colors))),
            4 => Unsafe.BitCast<Generic4ByteData, T>(Average(MemoryMarshal.Cast<T, Generic4ByteData>(colors))),
            _ => throw new NotSupportedException("Data is not of a supported valid color-type.")
        };
    }

    private static Generic3ByteData Average(ReadOnlySpan<Generic3ByteData> data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) return default;
        if (data.Length == 1) return data[0];

        Generic4LongData sum = Sum(data);

        float count = data.Length;
        return new Generic3ByteData((byte)(sum.L1 / count),
                                    (byte)(sum.L2 / count),
                                    (byte)(sum.L3 / count));
    }

    private static Generic4ByteData Average(ReadOnlySpan<Generic4ByteData> data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) return default;
        if (data.Length == 1) return data[0];

        Generic4LongData sum = Sum(data);

        float count = data.Length;
        return new Generic4ByteData((byte)(sum.L1 / count),
                                    (byte)(sum.L2 / count),
                                    (byte)(sum.L3 / count),
                                    (byte)(sum.L4 / count));
    }

    #endregion
}
