namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static IColor Average(IImage image)
    {
        long count = (long)image.Width * image.Height;

        ColorSum sum = Sum(image);
        return new ColorRGBA(Round(sum.R, count),
                             Round(sum.G, count),
                             Round(sum.B, count),
                             Round(sum.A, count));
    }

    public static T Average<T>(Image<T> image)
        where T : unmanaged, IColor<T>
    {
        long count = (long)image.Width * image.Height;

        ColorSum sum = Sum(image);
        return T.Create(Round(sum.R, count),
                        Round(sum.G, count),
                        Round(sum.B, count),
                        Round(sum.A, count));
    }

    public static T Average<T>(RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        long count = (long)image.Width * image.Height;

        ColorSum sum = Sum(image);
        return T.Create(Round(sum.R, count),
                        Round(sum.G, count),
                        Round(sum.B, count),
                        Round(sum.A, count));
    }

    public static T Average<T>(Span<T> colors)
        where T : unmanaged, IColor<T>
    {
        long count = colors.Length;

        ColorSum sum = Sum(colors);
        return T.Create(Round(sum.R, count),
                        Round(sum.G, count),
                        Round(sum.B, count),
                        Round(sum.A, count));
    }

    public static T Average<T>(ReadOnlySpan<T> colors)
        where T : unmanaged, IColor<T>
    {
        long count = colors.Length;

        ColorSum sum = Sum(colors);
        return T.Create(Round(sum.R, count),
                        Round(sum.G, count),
                        Round(sum.B, count),
                        Round(sum.A, count));
    }

    internal static byte Round(long sum, long count) => (byte)Math.Round(sum / (double)count, MidpointRounding.ToEven);

    #endregion
}
