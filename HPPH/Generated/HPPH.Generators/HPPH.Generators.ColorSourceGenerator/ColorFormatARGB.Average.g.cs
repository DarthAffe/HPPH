using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatARGB
{
    #region Methods

    unsafe IColor IColorFormat.Average(ReadOnlySpan<byte> data) => PixelHelper.Average(MemoryMarshal.Cast<byte, ColorARGB>(data));

    #endregion
}