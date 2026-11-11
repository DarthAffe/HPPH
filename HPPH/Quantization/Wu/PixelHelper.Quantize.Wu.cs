namespace HPPH;

public static partial class PixelHelper
{
    #region Methods

    /// <summary>
    /// Creates a palette of up to <paramref name="paletteSize"/> colors using Wu's quantizer.
    /// </summary>
    /// <remarks>
    /// The palette is ordered by the number of pixels each color represents, most common first.
    /// Fewer colors are returned if the image doesn't have enough distinct colors to fill the palette.
    /// </remarks>
    public static IColor[] CreateWuColorPalette(this IImage image, int paletteSize)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<WuPaletteOperation, IColor[]>(new WuPaletteOperation(paletteSize));
    }

    /// <inheritdoc cref="CreateWuColorPalette(IImage, int)"/>
    public static T[] CreateWuColorPalette<T>(this Image<T> image, int paletteSize)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().CreateWuColorPalette(paletteSize);
    }

    /// <inheritdoc cref="CreateWuColorPalette(IImage, int)"/>
    public static unsafe T[] CreateWuColorPalette<T>(this RefImage<T> image, int paletteSize)
        where T : unmanaged, IColor<T>
    {
        if (paletteSize < 0) throw new ArgumentException("PaletteSize can't be < 0", nameof(paletteSize));
        if ((paletteSize == 0) || (image.Width == 0) || (image.Height == 0)) return [];

        if ((image.Width * image.Height) <= paletteSize)
            return GetDistinctColors(image.ToArray());

        GetRows(image, out nuint rowBytes, out nuint stride, out nuint rows);
        fixed (byte* ptr = image)
            return WuQuantizer.CreateColorPalette<T>(ptr, rowBytes / (nuint)sizeof(T), stride, rows, paletteSize);
    }

    /// <inheritdoc cref="CreateWuColorPalette(IImage, int)"/>
    public static unsafe T[] CreateWuColorPalette<T>(this ReadOnlySpan<T> colors, int paletteSize)
        where T : unmanaged, IColor<T>
    {
        if (paletteSize < 0) throw new ArgumentException("PaletteSize can't be < 0", nameof(paletteSize));
        if ((paletteSize == 0) || colors.IsEmpty) return [];

        if (colors.Length <= paletteSize)
            return GetDistinctColors(colors);

        nuint length = (nuint)colors.Length;
        fixed (T* ptr = colors)
            return WuQuantizer.CreateColorPalette<T>((byte*)ptr, length, length * (nuint)sizeof(T), 1, paletteSize);
    }

    #endregion

    private readonly struct WuPaletteOperation(int paletteSize) : IImageOperation<IColor[]>
    {
        public IColor[] Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => ToIColors(image.CreateWuColorPalette(paletteSize));
    }
}
