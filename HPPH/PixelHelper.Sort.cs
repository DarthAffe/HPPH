namespace HPPH;

// ReSharper disable once RedundantUnsafeContext
public static unsafe partial class PixelHelper
{
    #region Methods

    [ColorSortGenerator("T", "R")]
    public static partial void SortByRed<T>(this Span<T> colors)
        where T : unmanaged, IColor;

    [ColorSortGenerator("T", "G")]
    public static partial void SortByGreen<T>(this Span<T> colors)
        where T : unmanaged, IColor;

    [ColorSortGenerator("T", "B")]
    public static partial void SortByBlue<T>(this Span<T> colors)
        where T : unmanaged, IColor;

    [ColorSortGenerator("T", "A")]
    public static partial void SortByAlpha<T>(this Span<T> colors)
        where T : unmanaged, IColor;

    //TODO DarthAffe 21.07.2024: Add CIE-Sorting

    #endregion
}
