using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatARGB
{
    #region Methods

    unsafe ISum IColorFormat.Sum(ReadOnlySpan<byte> data) => PixelHelper.Sum<ColorARGB, SumARGB>(MemoryMarshal.Cast<byte, ColorARGB>(data));

    #endregion
}