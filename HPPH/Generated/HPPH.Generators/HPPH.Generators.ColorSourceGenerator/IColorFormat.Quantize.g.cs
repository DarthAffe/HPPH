#nullable enable

namespace HPPH;

public partial interface IColorFormat
{
    internal IColor[] CreateWuColorPalette(IImage image, int paletteSize);
    internal IColor[] CreateMedianCutColorPalette(IImage image, int paletteSize);
}