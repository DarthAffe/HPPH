using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatRGBA
{
    #region Methods

    unsafe ISum IColorFormat.Sum(ReadOnlySpan<byte> data) => PixelHelper.Sum<ColorRGBA, SumRGBA>(MemoryMarshal.Cast<byte, ColorRGBA>(data));

    #endregion
}