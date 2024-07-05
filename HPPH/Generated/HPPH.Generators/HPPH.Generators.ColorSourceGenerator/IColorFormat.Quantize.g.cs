namespace HPPH;

public partial interface IColorFormat
{
    internal IColor[] CreateColorPalette(ReadOnlySpan<byte> data, int paletteSize);
}