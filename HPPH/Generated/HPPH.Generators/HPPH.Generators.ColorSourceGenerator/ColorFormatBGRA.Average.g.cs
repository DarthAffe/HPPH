#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatBGRA
{
    #region Methods

    unsafe IColor IColorFormat.Average(ReadOnlySpan<byte> data) => PixelHelper.Average(MemoryMarshal.Cast<byte, ColorBGRA>(data));

    #endregion
}