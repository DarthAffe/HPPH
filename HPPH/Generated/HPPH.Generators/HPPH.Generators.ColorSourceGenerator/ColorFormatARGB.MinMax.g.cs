#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatARGB
{
    #region Methods

    unsafe IMinMax IColorFormat.MinMax(ReadOnlySpan<byte> data) => PixelHelper.MinMax<ColorARGB, MinMaxARGB>(MemoryMarshal.Cast<byte, ColorARGB>(data));

    #endregion
}