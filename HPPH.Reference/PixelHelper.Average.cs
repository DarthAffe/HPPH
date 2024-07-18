namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static IColor Average(IImage image)
    {
        float count = image.Width * image.Height;

        ISum sum = Sum(image);
        return new ColorRGBA((byte)(sum.R / count),
                             (byte)(sum.G / count),
                             (byte)(sum.B / count),
                             (byte)(sum.A / count));
    }

    public static T Average<T>(IImage<T> image)
        where T : struct, IColor
    {
        float count = image.Width * image.Height;

        ISum sum = Sum(image);
        return (T)T.Create((byte)(sum.R / count),
                           (byte)(sum.G / count),
                           (byte)(sum.B / count),
                           (byte)(sum.A / count));
    }

    public static T Average<T>(RefImage<T> image)
        where T : struct, IColor
    {
        float count = image.Width * image.Height;

        ISum sum = Sum(image);
        return (T)T.Create((byte)(sum.R / count),
                           (byte)(sum.G / count),
                           (byte)(sum.B / count),
                           (byte)(sum.A / count));
    }

    public static T Average<T>(Span<T> colors)
        where T : struct, IColor
    {
        float count = colors.Length;

        ISum sum = Sum(colors);
        return (T)T.Create((byte)(sum.R / count),
                           (byte)(sum.G / count),
                           (byte)(sum.B / count),
                           (byte)(sum.A / count));
    }

    public static T Average<T>(ReadOnlySpan<T> colors)
        where T : struct, IColor
    {
        float count = colors.Length;

        ISum sum = Sum(colors);
        return (T)T.Create((byte)(sum.R / count),
                           (byte)(sum.G / count),
                           (byte)(sum.B / count),
                           (byte)(sum.A / count));
    }

    #endregion
}
