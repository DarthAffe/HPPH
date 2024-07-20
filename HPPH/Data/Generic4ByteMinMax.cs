using System.Runtime.CompilerServices;

namespace HPPH;

[SkipLocalsInit]
internal readonly struct Generic4ByteMinMax(byte b1Min, byte b1Max, byte b2Min, byte b2Max, byte b3Min, byte b3Max, byte b4Min, byte b4Max)
{
    public readonly byte B1Min = b1Min;
    public readonly byte B1Max = b1Max;

    public readonly byte B2Min = b2Min;
    public readonly byte B2Max = b2Max;

    public readonly byte B3Min = b3Min;
    public readonly byte B3Max = b3Max;

    public readonly byte B4Min = b4Min;
    public readonly byte B4Max = b4Max;
}
