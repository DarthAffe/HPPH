#nullable enable

namespace HPPH;

public partial interface IColorFormat
{
    internal IColor[] CreateColorPalette(ReadOnlySpan<byte> data, int paletteSize);
    internal IColor[] CreateSimpleColorPalette(ReadOnlySpan<byte> data, int paletteSize);
}