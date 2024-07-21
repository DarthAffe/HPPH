#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatBGRA
{
    #region Methods

    unsafe ISum IColorFormat.Sum(ReadOnlySpan<byte> data) => PixelHelper.Sum<ColorBGRA, SumBGRA>(MemoryMarshal.Cast<byte, ColorBGRA>(data));

    #endregion
}