using System.Runtime.InteropServices;

namespace HPPH;

public static partial class PixelHelper
{
    #region Methods

    /// <summary>
    /// Creates a palette of up to <paramref name="paletteSize"/> colors using the default algorithm (currently <see cref="CreateWuColorPalette(IImage, int, Span{long})"/>).
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
    public static IColor[] CreateColorPalette(this IImage image, int paletteSize, Span<long> pixelCounts = default)
        => image.CreateWuColorPalette(paletteSize, pixelCounts);

    /// <inheritdoc cref="CreateColorPalette(IImage, int, Span{long})"/>
    public static T[] CreateColorPalette<T>(this Image<T> image, int paletteSize, Span<long> pixelCounts = default)
        where T : unmanaged, IColor<T>
        => image.CreateWuColorPalette(paletteSize, pixelCounts);

    /// <inheritdoc cref="CreateColorPalette(IImage, int, Span{long})"/>
    public static T[] CreateColorPalette<T>(this RefImage<T> image, int paletteSize, Span<long> pixelCounts = default)
        where T : unmanaged, IColor<T>
        => image.CreateWuColorPalette(paletteSize, pixelCounts);

    /// <inheritdoc cref="CreateColorPalette(IImage, int, Span{long})"/>
    public static T[] CreateColorPalette<T>(this ReadOnlySpan<T> colors, int paletteSize, Span<long> pixelCounts = default)
        where T : unmanaged, IColor<T>
        => colors.CreateWuColorPalette(paletteSize, pixelCounts);

    private static void ValidatePaletteArguments(int paletteSize, Span<long> pixelCounts)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(paletteSize);
        if (!pixelCounts.IsEmpty && (pixelCounts.Length < paletteSize)) throw new ArgumentException($"PixelCounts needs room for the whole palette. {pixelCounts.Length} < {paletteSize}", nameof(pixelCounts));
    }

    private static T[] GetDistinctColors<T>(ReadOnlySpan<T> colors, Span<long> pixelCounts)
        where T : unmanaged, IColor<T>
        => ToPalette(colors, [], pixelCounts);

    internal static T[] ToPalette<T>(ReadOnlySpan<T> colors, ReadOnlySpan<long> populations, Span<long> pixelCounts)
        where T : unmanaged, IColor<T>
    {
        T[] distinct = new T[colors.Length];
        long[] distinctPopulations = new long[colors.Length];
        Dictionary<T, int> indices = new(colors.Length);

        int count = 0;
        for (int i = 0; i < colors.Length; i++)
        {
            ref int index = ref CollectionsMarshal.GetValueRefOrAddDefault(indices, colors[i], out bool exists);
            if (!exists)
            {
                index = count++;
                distinct[index] = colors[i];
            }

            distinctPopulations[index] += populations.IsEmpty ? 1 : populations[i];
        }

        int[] order = new int[count];
        for (int i = 0; i < order.Length; i++)
            order[i] = i;

        Array.Sort(order, (a, b) =>
        {
            int result = distinctPopulations[b].CompareTo(distinctPopulations[a]);
            return result != 0 ? result : a.CompareTo(b);
        });

        T[] palette = new T[count];
        for (int i = 0; i < palette.Length; i++)
            palette[i] = distinct[order[i]];

        if (!pixelCounts.IsEmpty)
            for (int i = 0; i < palette.Length; i++)
                pixelCounts[i] = distinctPopulations[order[i]];

        return palette;
    }

    #endregion
}
