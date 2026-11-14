#nullable enable

namespace HPPH;

public partial interface IColor<out TSelf>
{
    internal static abstract ColorMinMax ToColorMinMax(Generic3ByteMinMax data);
    internal static abstract ColorMinMax ToColorMinMax(Generic4ByteMinMax data);
}