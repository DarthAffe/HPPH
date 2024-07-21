#nullable enable

namespace HPPH;

public partial interface IColorFormat
{
    internal IMinMax MinMax(ReadOnlySpan<byte> data);
}