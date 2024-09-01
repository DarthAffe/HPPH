using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

public static partial class PixelHelper
{
    #region Methods

    public static IColor Average(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        float count = image.Width * image.Height;

        ISum sum = Sum(image);

        return image.ColorFormat.CreateColor((byte)MathF.Round(sum.R / count),
                                             (byte)MathF.Round(sum.G / count),
                                             (byte)MathF.Round(sum.B / count),
                                             (byte)MathF.Round(sum.A / count));
    }

    public static T Average<T>(this IImage<T> image)
        where T : struct, IColor
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));

        return image.AsRefImage().Average();
    }

    public static T Average<T>(this RefImage<T> image)
        where T : struct, IColor
    {
        float count = image.Width * image.Height;

        ISum sum = Sum(image);
        return (T)T.Create((byte)MathF.Round(sum.R / count),
                           (byte)MathF.Round(sum.G / count),
                           (byte)MathF.Round(sum.B / count),
                           (byte)MathF.Round(sum.A / count));
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
        return new Generic3ByteData((byte)MathF.Round(sum.L1 / count),
                                    (byte)MathF.Round(sum.L2 / count),
                                    (byte)MathF.Round(sum.L3 / count));
    }

    private static Generic4ByteData Average(ReadOnlySpan<Generic4ByteData> data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) return default;
        if (data.Length == 1) return data[0];

        Generic4LongData sum = Sum(data);

        float count = data.Length;
        return new Generic4ByteData((byte)MathF.Round(sum.L1 / count),
                                    (byte)MathF.Round(sum.L2 / count),
                                    (byte)MathF.Round(sum.L3 / count),
                                    (byte)MathF.Round(sum.L4 / count));
    }

    #endregion
}
