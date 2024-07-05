using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatRGB
{
    #region Methods

    unsafe IMinMax IColorFormat.MinMax(ReadOnlySpan<byte> data) => PixelHelper.MinMax<ColorRGB, MinMaxRGB>(MemoryMarshal.Cast<byte, ColorRGB>(data));

    #endregion
}