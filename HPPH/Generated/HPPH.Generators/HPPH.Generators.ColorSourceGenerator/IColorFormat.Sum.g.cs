#nullable enable

namespace HPPH;

public partial interface IColorFormat
{
    internal ColorSum Sum(IImage image);
    internal IColor Average(IImage image);

    internal ColorSum ToColorSum(Generic4LongData data);
}