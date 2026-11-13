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
    /// <param name="image">The image to create the palette for.</param>
    /// <param name="paletteSize">The maximum number of colors in the palette.</param>
    /// <param name="pixelCounts">
    /// Optional. Receives the number of pixels each palette color represents, at the same index as the color.
    /// Needs at least <paramref name="paletteSize"/> elements, only the first <c>palette.Length</c> are written.
    /// </param>
    /// <returns>The palette.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="paletteSize"/> is negative.</exception>
    /// <exception cref="ArgumentException"><paramref name="pixelCounts"/> is not empty but shorter than <paramref name="paletteSize"/>.</exception>
    public static IColor[] CreateWuColorPalette(this IImage image, int paletteSize, Span<long> pixelCounts = default)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<WuPaletteOperation, IColor[]>(new WuPaletteOperation(paletteSize, pixelCounts));
    }

    /// <inheritdoc cref="CreateWuColorPalette(IImage, int, Span{long})"/>
    public static T[] CreateWuColorPalette<T>(this Image<T> image, int paletteSize, Span<long> pixelCounts = default)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().CreateWuColorPalette(paletteSize, pixelCounts);
    }

    /// <inheritdoc cref="CreateWuColorPalette(IImage, int, Span{long})"/>
    public static unsafe T[] CreateWuColorPalette<T>(this RefImage<T> image, int paletteSize, Span<long> pixelCounts = default)
        where T : unmanaged, IColor<T>
    {
        ValidatePaletteArguments(paletteSize, pixelCounts);
        if ((paletteSize == 0) || (image.Width == 0) || (image.Height == 0)) return [];

        if ((image.Width * image.Height) <= paletteSize)
            return GetDistinctColors(image.ToArray(), pixelCounts);

        GetRows(image, out nuint rowBytes, out nuint stride, out nuint rows);
        fixed (byte* ptr = image)
            return WuQuantizer.CreateColorPalette<T>(ptr, rowBytes / (nuint)sizeof(T), stride, rows, paletteSize, pixelCounts);
    }

    /// <inheritdoc cref="CreateWuColorPalette(IImage, int, Span{long})"/>
    public static unsafe T[] CreateWuColorPalette<T>(this ReadOnlySpan<T> colors, int paletteSize, Span<long> pixelCounts = default)
        where T : unmanaged, IColor<T>
    {
        ValidatePaletteArguments(paletteSize, pixelCounts);
        if ((paletteSize == 0) || colors.IsEmpty) return [];

        if (colors.Length <= paletteSize)
            return GetDistinctColors(colors, pixelCounts);

        nuint length = (nuint)colors.Length;
        fixed (T* ptr = colors)
            return WuQuantizer.CreateColorPalette<T>((byte*)ptr, length, length * (nuint)sizeof(T), 1, paletteSize, pixelCounts);
    }

    #endregion

    private readonly ref struct WuPaletteOperation : IImageOperation<IColor[]>
    {
        private readonly int _paletteSize;
        private readonly Span<long> _pixelCounts;

        public WuPaletteOperation(int paletteSize, Span<long> pixelCounts = default)
        {
            this._paletteSize = paletteSize;
            this._pixelCounts = pixelCounts;
        }

        public IColor[] Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => ToIColors(image.CreateWuColorPalette(_paletteSize, _pixelCounts));
    }
}
