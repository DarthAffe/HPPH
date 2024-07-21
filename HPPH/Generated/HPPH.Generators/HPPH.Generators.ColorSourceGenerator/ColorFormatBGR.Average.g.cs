#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatBGR
{
    #region Methods

    unsafe IColor IColorFormat.Average(ReadOnlySpan<byte> data) => PixelHelper.Average(MemoryMarshal.Cast<byte, ColorBGR>(data));

    #endregion
}