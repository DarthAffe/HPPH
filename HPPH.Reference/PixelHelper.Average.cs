namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static IColor Average(IImage image)
    {
        float count = image.Width * image.Height;

        ColorSum sum = Sum(image);
        return new ColorRGBA((byte)MathF.Round(sum.R / count),
                             (byte)MathF.Round(sum.G / count),
                             (byte)MathF.Round(sum.B / count),
                             (byte)MathF.Round(sum.A / count));
    }

    public static T Average<T>(Image<T> image)
        where T : unmanaged, IColor<T>
    {
        float count = image.Width * image.Height;

        ColorSum sum = Sum(image);
        return T.Create((byte)MathF.Round(sum.R / count),
                        (byte)MathF.Round(sum.G / count),
                        (byte)MathF.Round(sum.B / count),
                        (byte)MathF.Round(sum.A / count));
    }

    public static T Average<T>(RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        float count = image.Width * image.Height;

        ColorSum sum = Sum(image);
        return T.Create((byte)MathF.Round(sum.R / count),
                        (byte)MathF.Round(sum.G / count),
                        (byte)MathF.Round(sum.B / count),
                        (byte)MathF.Round(sum.A / count));
    }

    public static T Average<T>(Span<T> colors)
        where T : unmanaged, IColor<T>
    {
        float count = colors.Length;

        ColorSum sum = Sum(colors);
        return T.Create((byte)MathF.Round(sum.R / count),
                        (byte)MathF.Round(sum.G / count),
                        (byte)MathF.Round(sum.B / count),
                        (byte)MathF.Round(sum.A / count));
    }

    public static T Average<T>(ReadOnlySpan<T> colors)
        where T : unmanaged, IColor<T>
    {
        float count = colors.Length;

        ColorSum sum = Sum(colors);
        return T.Create((byte)MathF.Round(sum.R / count),
                        (byte)MathF.Round(sum.G / count),
                        (byte)MathF.Round(sum.B / count),
                        (byte)MathF.Round(sum.A / count));
    }

    #endregion
}
