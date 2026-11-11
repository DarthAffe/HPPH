#nullable enable

namespace HPPH;

public sealed partial class ColorFormatARGB
{
    #region Methods

    ColorSum IColorFormat.ToColorSum(Generic4LongData data) => new(data.L2, data.L3, data.L4, data.L1);

    #endregion
}