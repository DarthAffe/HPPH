namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    // DarthAffe 05.07.2024: LINQ OrderBy uses a stable sorting algorithm -> it's a good reference as the optimized sort is supposed to be stable.

    public static void SortByRed<T>(Span<T> colors)
        where T : unmanaged, IColor
        => colors.ToArray().OrderBy(x => x.R).ToArray().AsSpan().CopyTo(colors);

    public static void SortByGreen<T>(Span<T> colors)
        where T : unmanaged, IColor
        => colors.ToArray().OrderBy(x => x.G).ToArray().AsSpan().CopyTo(colors);

    public static void SortByBlue<T>(Span<T> colors)
        where T : unmanaged, IColor
        => colors.ToArray().OrderBy(x => x.B).ToArray().AsSpan().CopyTo(colors);

    public static void SortByAlpha<T>(Span<T> colors)
        where T : unmanaged, IColor
        => colors.ToArray().OrderBy(x => x.A).ToArray().AsSpan().CopyTo(colors);

    #endregion
}
