namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static ColorSum Sum(IImage image)
    {
        long sumR = 0, sumG = 0, sumB = 0, sumA = 0;

        foreach (IColor color in image)
        {
            sumR += color.R;
            sumG += color.G;
            sumB += color.B;
            sumA += color.A;
        }

        return new ColorSum(sumR, sumG, sumB, sumA);
    }

    public static ColorSum Sum<T>(RefImage<T> image)
        where T : unmanaged, IColor
    {
        long sumR = 0, sumG = 0, sumB = 0, sumA = 0;

        foreach (T color in image)
        {
            sumR += color.R;
            sumG += color.G;
            sumB += color.B;
            sumA += color.A;
        }

        return new ColorSum(sumR, sumG, sumB, sumA);
    }

    public static ColorSum Sum<T>(Span<T> colors)
        where T : unmanaged, IColor
    {
        long sumR = 0, sumG = 0, sumB = 0, sumA = 0;

        foreach (T color in colors)
        {
            sumR += color.R;
            sumG += color.G;
            sumB += color.B;
            sumA += color.A;
        }

        return new ColorSum(sumR, sumG, sumB, sumA);
    }

    public static ColorSum Sum<T>(ReadOnlySpan<T> colors)
        where T : unmanaged, IColor
    {
        long sumR = 0, sumG = 0, sumB = 0, sumA = 0;

        foreach (T color in colors)
        {
            sumR += color.R;
            sumG += color.G;
            sumB += color.B;
            sumA += color.A;
        }

        return new ColorSum(sumR, sumG, sumB, sumA);
    }

    #endregion
}
