namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static IColor Average(IImage image)
    {
        float count = image.Width * image.Height;

        ISum sum = Sum(image);
        return new ColorRGBA((sum.R / count).GetByteValueFromPercentage(),
                             (sum.G / count).GetByteValueFromPercentage(),
                             (sum.B / count).GetByteValueFromPercentage(),
                             (sum.A / count).GetByteValueFromPercentage());
    }

    public static T Average<T>(RefImage<T> image)
        where T : struct, IColor
    {
        float count = image.Width * image.Height;

        ISum sum = Sum(image);
        return (T)T.Create((sum.R / count).GetByteValueFromPercentage(),
                           (sum.G / count).GetByteValueFromPercentage(),
                           (sum.B / count).GetByteValueFromPercentage(),
                           (sum.A / count).GetByteValueFromPercentage());
    }

    public static T Average<T>(ReadOnlySpan<T> colors)
        where T : struct, IColor
    {
        float count = colors.Length;

        ISum sum = Sum(colors);
        return (T)T.Create((sum.R / count).GetByteValueFromPercentage(),
                           (sum.G / count).GetByteValueFromPercentage(),
                           (sum.B / count).GetByteValueFromPercentage(),
                           (sum.A / count).GetByteValueFromPercentage());
    }

    #endregion
}
