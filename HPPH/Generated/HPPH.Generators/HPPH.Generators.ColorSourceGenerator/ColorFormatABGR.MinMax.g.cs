﻿#nullable enable

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatABGR
{
    #region Methods

    unsafe IMinMax IColorFormat.ToMinMax(Generic3ByteMinMax data) => throw new NotSupportedException();
    unsafe IMinMax IColorFormat.ToMinMax(Generic4ByteMinMax data) => Unsafe.BitCast<Generic4ByteMinMax, MinMaxABGR>(data);

    #endregion
}