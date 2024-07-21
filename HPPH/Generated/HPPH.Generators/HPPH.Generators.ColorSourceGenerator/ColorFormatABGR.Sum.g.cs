#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatABGR
{
    #region Methods

    unsafe ISum IColorFormat.Sum(ReadOnlySpan<byte> data) => PixelHelper.Sum<ColorABGR, SumABGR>(MemoryMarshal.Cast<byte, ColorABGR>(data));

    #endregion
}