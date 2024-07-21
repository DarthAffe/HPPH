#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatBGR
{
    #region Methods

    unsafe IMinMax IColorFormat.MinMax(ReadOnlySpan<byte> data) => PixelHelper.MinMax<ColorBGR, MinMaxBGR>(MemoryMarshal.Cast<byte, ColorBGR>(data));

    #endregion
}