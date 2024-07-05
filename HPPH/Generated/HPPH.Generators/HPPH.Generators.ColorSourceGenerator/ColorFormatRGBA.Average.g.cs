using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatRGBA
{
    #region Methods

    unsafe IColor IColorFormat.Average(ReadOnlySpan<byte> data) => PixelHelper.Average(MemoryMarshal.Cast<byte, ColorRGBA>(data));

    #endregion
}