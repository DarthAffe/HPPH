#nullable enable

namespace HPPH;

public partial interface IColorFormat
{
    internal Generic4LongData Sum(ReadOnlySpan<byte> data);
    internal ISum ToSum(Generic4LongData data);
}