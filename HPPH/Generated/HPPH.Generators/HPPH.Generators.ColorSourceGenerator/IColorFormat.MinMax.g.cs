#nullable enable

namespace HPPH;

public partial interface IColorFormat
{
    internal IMinMax ToMinMax(Generic3ByteMinMax data);
    internal IMinMax ToMinMax(Generic4ByteMinMax data);
}