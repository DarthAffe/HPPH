namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static ColorMinMax MinMax(IImage image)
    {
        byte minR = byte.MaxValue, minG = byte.MaxValue, minB = byte.MaxValue, minA = byte.MaxValue;
        byte maxR = byte.MinValue, maxG = byte.MinValue, maxB = byte.MinValue, maxA = byte.MinValue;

        foreach (IColor color in image)
        {
            minR = Math.Min(minR, color.R);
            minG = Math.Min(minG, color.G);
            minB = Math.Min(minB, color.B);
            minA = Math.Min(minA, color.A);

            maxR = Math.Max(maxR, color.R);
            maxG = Math.Max(maxG, color.G);
            maxB = Math.Max(maxB, color.B);
            maxA = Math.Max(maxA, color.A);
        }

        return new ColorMinMax(minR, maxR, minG, maxG, minB, maxB, minA, maxA);
    }

    public static ColorMinMax MinMax<T>(RefImage<T> image)
        where T : unmanaged, IColor
    {
        byte minR = byte.MaxValue, minG = byte.MaxValue, minB = byte.MaxValue, minA = byte.MaxValue;
        byte maxR = byte.MinValue, maxG = byte.MinValue, maxB = byte.MinValue, maxA = byte.MinValue;

        foreach (T color in image)
        {
            minR = Math.Min(minR, color.R);
            minG = Math.Min(minG, color.G);
            minB = Math.Min(minB, color.B);
            minA = Math.Min(minA, color.A);

            maxR = Math.Max(maxR, color.R);
            maxG = Math.Max(maxG, color.G);
            maxB = Math.Max(maxB, color.B);
            maxA = Math.Max(maxA, color.A);
        }

        return new ColorMinMax(minR, maxR, minG, maxG, minB, maxB, minA, maxA);
    }

    public static ColorMinMax MinMax<T>(Span<T> colors)
        where T : unmanaged, IColor
    {
        byte minR = byte.MaxValue, minG = byte.MaxValue, minB = byte.MaxValue, minA = byte.MaxValue;
        byte maxR = byte.MinValue, maxG = byte.MinValue, maxB = byte.MinValue, maxA = byte.MinValue;

        foreach (T color in colors)
        {
            minR = Math.Min(minR, color.R);
            minG = Math.Min(minG, color.G);
            minB = Math.Min(minB, color.B);
            minA = Math.Min(minA, color.A);

            maxR = Math.Max(maxR, color.R);
            maxG = Math.Max(maxG, color.G);
            maxB = Math.Max(maxB, color.B);
            maxA = Math.Max(maxA, color.A);
        }

        return new ColorMinMax(minR, maxR, minG, maxG, minB, maxB, minA, maxA);
    }

    public static ColorMinMax MinMax<T>(ReadOnlySpan<T> colors)
        where T : unmanaged, IColor
    {
        byte minR = byte.MaxValue, minG = byte.MaxValue, minB = byte.MaxValue, minA = byte.MaxValue;
        byte maxR = byte.MinValue, maxG = byte.MinValue, maxB = byte.MinValue, maxA = byte.MinValue;

        foreach (T color in colors)
        {
            minR = Math.Min(minR, color.R);
            minG = Math.Min(minG, color.G);
            minB = Math.Min(minB, color.B);
            minA = Math.Min(minA, color.A);

            maxR = Math.Max(maxR, color.R);
            maxG = Math.Max(maxG, color.G);
            maxB = Math.Max(maxB, color.B);
            maxA = Math.Max(maxA, color.A);
        }

        return new ColorMinMax(minR, maxR, minG, maxG, minB, maxB, minA, maxA);
    }

    #endregion
}
