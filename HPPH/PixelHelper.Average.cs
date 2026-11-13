using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

public static partial class PixelHelper
{
    #region Methods

    /// <summary>
    /// Calculates the average color of the pixels of <paramref name="image"/>.
    /// </summary>
    /// <param name="image">The image to calculate the average for.</param>
    /// <returns>The average color.</returns>
    public static IColor Average(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<AverageOperation, IColor>(default);
    }

    /// <inheritdoc cref="Average(IImage)"/>
    public static T Average<T>(this Image<T> image)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().Average();
    }

    /// <inheritdoc cref="Average(IImage)"/>
    public static T Average<T>(this RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        long count = (long)image.Width * image.Height;

        ColorSum sum = image.Sum();
        return T.Create(RoundedAverage(sum.R, count),
                        RoundedAverage(sum.G, count),
                        RoundedAverage(sum.B, count),
                        RoundedAverage(sum.A, count));
    }

    /// <summary>
    /// Calculates the average color of <paramref name="colors"/>.
    /// </summary>
    /// <param name="colors">The colors to calculate the average for.</param>
    /// <returns>The average color.</returns>
    public static T Average<T>(this ReadOnlySpan<T> colors)
        where T : unmanaged, IColor<T>
    {
        return T.ColorFormat.BytesPerPixel switch
        {
            3 => Unsafe.BitCast<Generic3ByteData, T>(Average(MemoryMarshal.Cast<T, Generic3ByteData>(colors))),
            4 => Unsafe.BitCast<Generic4ByteData, T>(Average(MemoryMarshal.Cast<T, Generic4ByteData>(colors))),
            _ => throw new NotSupportedException("Data is not of a supported valid color-type.")
        };
    }

    private static Generic3ByteData Average(ReadOnlySpan<Generic3ByteData> data)
    {
        if (data.Length == 0) return default;
        if (data.Length == 1) return data[0];

        Generic4LongData sum = Sum(data);

        long count = data.Length;
        return new Generic3ByteData(RoundedAverage(sum.L1, count),
                                    RoundedAverage(sum.L2, count),
                                    RoundedAverage(sum.L3, count));
    }

    private static Generic4ByteData Average(ReadOnlySpan<Generic4ByteData> data)
    {
        if (data.Length == 0) return default;
        if (data.Length == 1) return data[0];

        Generic4LongData sum = Sum(data);

        long count = data.Length;
        return new Generic4ByteData(RoundedAverage(sum.L1, count),
                                    RoundedAverage(sum.L2, count),
                                    RoundedAverage(sum.L3, count),
                                    RoundedAverage(sum.L4, count));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte RoundedAverage(long sum, long count) => (byte)Math.Round(sum / (double)count, MidpointRounding.ToEven);

    #endregion

    private readonly struct AverageOperation : IImageOperation<IColor>
    {
        public IColor Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image.Average();
    }
}
