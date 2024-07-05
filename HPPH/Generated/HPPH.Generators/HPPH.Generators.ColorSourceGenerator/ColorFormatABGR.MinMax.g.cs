using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatABGR
{
    #region Methods

    unsafe IMinMax IColorFormat.MinMax(ReadOnlySpan<byte> data) => PixelHelper.MinMax<ColorABGR, MinMaxABGR>(MemoryMarshal.Cast<byte, ColorABGR>(data));

    #endregion
}