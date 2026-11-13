namespace HPPH;

// ReSharper disable once RedundantUnsafeContext
public static unsafe partial class PixelHelper
{
    #region Methods

    /// <summary>
    /// Sorts <paramref name="colors"/> in place by their red-component.
    /// </summary>
    /// <param name="colors">The colors to sort.</param>
    [ColorSortGenerator("T", "R")]
    public static partial void SortByRed<T>(this Span<T> colors)
        where T : unmanaged, IColor<T>;

    /// <summary>
    /// Sorts <paramref name="colors"/> in place by their green-component.
    /// </summary>
    /// <param name="colors">The colors to sort.</param>
    [ColorSortGenerator("T", "G")]
    public static partial void SortByGreen<T>(this Span<T> colors)
        where T : unmanaged, IColor<T>;

    /// <summary>
    /// Sorts <paramref name="colors"/> in place by their blue-component.
    /// </summary>
    /// <param name="colors">The colors to sort.</param>
    [ColorSortGenerator("T", "B")]
    public static partial void SortByBlue<T>(this Span<T> colors)
        where T : unmanaged, IColor<T>;

    /// <summary>
    /// Sorts <paramref name="colors"/> in place by their alpha-component.
    /// </summary>
    /// <param name="colors">The colors to sort.</param>
    [ColorSortGenerator("T", "A")]
    public static partial void SortByAlpha<T>(this Span<T> colors)
        where T : unmanaged, IColor<T>;

    //TODO DarthAffe 21.07.2024: Add CIE-Sorting

    #endregion
}
