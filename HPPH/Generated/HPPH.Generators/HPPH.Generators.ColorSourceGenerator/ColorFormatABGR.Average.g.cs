using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatABGR
{
    #region Methods

    unsafe IColor IColorFormat.Average(ReadOnlySpan<byte> data) => PixelHelper.Average(MemoryMarshal.Cast<byte, ColorABGR>(data));

    #endregion
}