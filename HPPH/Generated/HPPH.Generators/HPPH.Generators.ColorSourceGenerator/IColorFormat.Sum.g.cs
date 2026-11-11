#nullable enable

namespace HPPH;

public partial interface IColorFormat
{
    internal ColorSum ToColorSum(Generic4LongData data);
}