using System.Runtime.CompilerServices;

namespace HPPH;

[SkipLocalsInit]
internal readonly struct Generic4ByteData(byte b1, byte b2, byte b3, byte b4)
{
    public readonly byte B1 = b1;
    public readonly byte B2 = b2;
    public readonly byte B3 = b3;
    public readonly byte B4 = b4;
}