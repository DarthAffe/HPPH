using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatRGBA
{
    #region Methods

    unsafe IMinMax IColorFormat.MinMax(ReadOnlySpan<byte> data) => PixelHelper.MinMax<ColorRGBA, MinMaxRGBA>(MemoryMarshal.Cast<byte, ColorRGBA>(data));

    #endregion
}