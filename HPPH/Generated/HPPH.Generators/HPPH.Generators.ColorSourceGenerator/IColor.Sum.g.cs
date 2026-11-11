#nullable enable

namespace HPPH;

public partial interface IColor<out TSelf>
{
    internal static abstract ColorSum ToColorSum(Generic4LongData data);
}