#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatRGB
{
    #region Methods

    unsafe ISum IColorFormat.Sum(ReadOnlySpan<byte> data) => PixelHelper.Sum<ColorRGB, SumRGB>(MemoryMarshal.Cast<byte, ColorRGB>(data));

    #endregion
}