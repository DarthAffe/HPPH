#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatBGR
{
    #region Methods

    unsafe ISum IColorFormat.Sum(ReadOnlySpan<byte> data) => PixelHelper.Sum<ColorBGR, SumBGR>(MemoryMarshal.Cast<byte, ColorBGR>(data));

    #endregion
}