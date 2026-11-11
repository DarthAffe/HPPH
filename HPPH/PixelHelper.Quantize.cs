namespace HPPH;

public static partial class PixelHelper
{
    #region Methods

    /// <summary>
    /// Creates a palette of up to <paramref name="paletteSize"/> colors using the default algorithm (currently <see cref="CreateWuColorPalette(IImage, int)"/>).
    /// </summary>
    public static IColor[] CreateColorPalette(this IImage image, int paletteSize)
        => image.CreateWuColorPalette(paletteSize);

    /// <inheritdoc cref="CreateColorPalette(IImage, int)"/>
    public static T[] CreateColorPalette<T>(this IImage<T> image, int paletteSize)
        where T : unmanaged, IColor
        => image.CreateWuColorPalette(paletteSize);

    /// <inheritdoc cref="CreateColorPalette(IImage, int)"/>
    public static T[] CreateColorPalette<T>(this RefImage<T> image, int paletteSize)
        where T : unmanaged, IColor
        => image.CreateWuColorPalette(paletteSize);

    /// <inheritdoc cref="CreateColorPalette(IImage, int)"/>
    public static T[] CreateColorPalette<T>(this ReadOnlySpan<T> colors, int paletteSize)
        where T : unmanaged, IColor
        => colors.CreateWuColorPalette(paletteSize);
    
    private static T[] GetDistinctColors<T>(ReadOnlySpan<T> colors)
        where T : unmanaged, IColor
    {
        HashSet<T> seen = new(colors.Length);
        T[] result = new T[colors.Length];

        int count = 0;
        foreach (T color in colors)
            if (seen.Add(color))
                result[count++] = color;

        return count == result.Length ? result : result[..count];
    }

    #endregion
}
