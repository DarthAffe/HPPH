using System.Runtime.InteropServices;

namespace HPPH;

public static partial class PixelHelper
{
    #region Methods

    /// <summary>
    /// Creates a palette of up to <paramref name="paletteSize"/> colors using the default algorithm (currently <see cref="CreateWuColorPalette(IImage, int)"/>).
    /// </summary>
    /// <remarks>
    /// The palette is ordered by the number of pixels each color represents, most common first.
    /// Fewer colors are returned if the image doesn't have enough distinct colors to fill the palette.
    /// </remarks>
    public static IColor[] CreateColorPalette(this IImage image, int paletteSize)
        => image.CreateWuColorPalette(paletteSize);

    /// <inheritdoc cref="CreateColorPalette(IImage, int)"/>
    public static T[] CreateColorPalette<T>(this Image<T> image, int paletteSize)
        where T : unmanaged, IColor<T>
        => image.CreateWuColorPalette(paletteSize);

    /// <inheritdoc cref="CreateColorPalette(IImage, int)"/>
    public static T[] CreateColorPalette<T>(this RefImage<T> image, int paletteSize)
        where T : unmanaged, IColor<T>
        => image.CreateWuColorPalette(paletteSize);

    /// <inheritdoc cref="CreateColorPalette(IImage, int)"/>
    public static T[] CreateColorPalette<T>(this ReadOnlySpan<T> colors, int paletteSize)
        where T : unmanaged, IColor<T>
        => colors.CreateWuColorPalette(paletteSize);
    
    private static T[] GetDistinctColors<T>(ReadOnlySpan<T> colors)
        where T : unmanaged, IColor<T>
        => ToPalette(colors, []);

    internal static T[] ToPalette<T>(ReadOnlySpan<T> colors, ReadOnlySpan<long> populations)
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

        return palette;
    }

    #endregion
}
