namespace HPPH;

public static unsafe partial class PixelHelper
{
    #region Methods

    [ColorSortGenerator("T", "R")]
    public static partial void SortByRed<T>(Span<T> colors)
        where T : unmanaged, IColor;

    [ColorSortGenerator("T", "G")]
    public static partial void SortByGreen<T>(Span<T> colors)
        where T : unmanaged, IColor;

    [ColorSortGenerator("T", "B")]
    public static partial void SortByBlue<T>(Span<T> colors)
        where T : unmanaged, IColor;

    [ColorSortGenerator("T", "A")]
    public static partial void SortByAlpha<T>(Span<T> colors)
        where T : unmanaged, IColor;

    [ColorSortGenerator("Generic3ByteData", "B1")]
    internal static partial void SortB1(Span<Generic3ByteData> colors);

    [ColorSortGenerator("Generic3ByteData", "B2")]
    internal static partial void SortB2(Span<Generic3ByteData> colors);

    [ColorSortGenerator("Generic3ByteData", "B3")]
    internal static partial void SortB3(Span<Generic3ByteData> colors);

    [ColorSortGenerator("Generic4ByteData", "B1")]
    internal static partial void SortB1(Span<Generic4ByteData> colors);

    [ColorSortGenerator("Generic4ByteData", "B2")]
    internal static partial void SortB2(Span<Generic4ByteData> colors);

    [ColorSortGenerator("Generic4ByteData", "B3")]
    internal static partial void SortB3(Span<Generic4ByteData> colors);

    [ColorSortGenerator("Generic4ByteData", "B4")]
    internal static partial void SortB4(Span<Generic4ByteData> colors);

    #endregion
}
