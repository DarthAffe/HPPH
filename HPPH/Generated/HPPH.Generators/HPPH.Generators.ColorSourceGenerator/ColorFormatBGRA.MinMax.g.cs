#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatBGRA
{
    #region Methods

    unsafe IMinMax IColorFormat.MinMax(ReadOnlySpan<byte> data) => PixelHelper.MinMax<ColorBGRA, MinMaxBGRA>(MemoryMarshal.Cast<byte, ColorBGRA>(data));

    #endregion
}