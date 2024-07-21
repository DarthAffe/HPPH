namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static IColor Average(IImage image)
    {
        float count = image.Width * image.Height;

        ISum sum = Sum(image);
        return new ColorRGBA((byte)MathF.Round(sum.R / count),
                             (byte)MathF.Round(sum.G / count),
                             (byte)MathF.Round(sum.B / count),
                             (byte)MathF.Round(sum.A / count));
    }

    public static T Average<T>(IImage<T> image)
        where T : struct, IColor
    {
        float count = image.Width * image.Height;

        ISum sum = Sum(image);
        return (T)T.Create((byte)MathF.Round(sum.R / count),
                           (byte)MathF.Round(sum.G / count),
                           (byte)MathF.Round(sum.B / count),
                           (byte)MathF.Round(sum.A / count));
    }

    public static T Average<T>(RefImage<T> image)
        where T : struct, IColor
    {
        float count = image.Width * image.Height;

        ISum sum = Sum(image);
        return (T)T.Create((byte)MathF.Round(sum.R / count),
                           (byte)MathF.Round(sum.G / count),
                           (byte)MathF.Round(sum.B / count),
                           (byte)MathF.Round(sum.A / count));
    }

    public static T Average<T>(Span<T> colors)
        where T : struct, IColor
    {
        float count = colors.Length;

        ISum sum = Sum(colors);
        return (T)T.Create((byte)MathF.Round(sum.R / count),
                           (byte)MathF.Round(sum.G / count),
                           (byte)MathF.Round(sum.B / count),
                           (byte)MathF.Round(sum.A / count));
    }

    public static T Average<T>(ReadOnlySpan<T> colors)
        where T : struct, IColor
    {
        float count = colors.Length;

        ISum sum = Sum(colors);
        return (T)T.Create((byte)MathF.Round(sum.R / count),
                           (byte)MathF.Round(sum.G / count),
                           (byte)MathF.Round(sum.B / count),
                           (byte)MathF.Round(sum.A / count));
    }

    #endregion
}
