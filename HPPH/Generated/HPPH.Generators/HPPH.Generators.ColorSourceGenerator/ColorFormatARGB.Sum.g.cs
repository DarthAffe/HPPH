#nullable enable

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatARGB
{
    #region Methods

    unsafe Generic4LongData IColorFormat.Sum(ReadOnlySpan<byte> data) => PixelHelper.Sum(MemoryMarshal.Cast<byte, Generic4ByteData>(data));
    unsafe ISum IColorFormat.ToSum(Generic4LongData data) => Unsafe.BitCast<Generic4LongData, SumARGB>(data);

    #endregion
}