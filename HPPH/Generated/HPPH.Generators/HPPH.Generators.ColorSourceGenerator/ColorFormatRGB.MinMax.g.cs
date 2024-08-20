#nullable enable

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatRGB
{
    #region Methods

    unsafe IMinMax IColorFormat.ToMinMax(Generic3ByteMinMax data) => Unsafe.BitCast<Generic3ByteMinMax, MinMaxRGB>(data);
    unsafe IMinMax IColorFormat.ToMinMax(Generic4ByteMinMax data) => throw new NotSupportedException();

    #endregion
}