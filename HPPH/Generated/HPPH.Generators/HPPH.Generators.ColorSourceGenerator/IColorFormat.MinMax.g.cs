#nullable enable

namespace HPPH;

public partial interface IColorFormat
{
    internal ColorMinMax MinMax(IImage image);

    internal ColorMinMax ToColorMinMax(Generic3ByteMinMax data);
    internal ColorMinMax ToColorMinMax(Generic4ByteMinMax data);
}