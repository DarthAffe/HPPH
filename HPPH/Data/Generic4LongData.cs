using System.Runtime.CompilerServices;

namespace HPPH;

[SkipLocalsInit]
internal readonly struct Generic4LongData(long l1, long l2, long l3, long l4)
{
    public readonly long L1 = l1;
    public readonly long L2 = l2;
    public readonly long L3 = l3;
    public readonly long L4 = l4;
}