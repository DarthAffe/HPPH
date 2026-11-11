#nullable enable

namespace HPPH;

public sealed partial class ColorFormatABGR
{
    #region Methods

    ColorSum IColorFormat.ToColorSum(Generic4LongData data) => new(data.L4, data.L3, data.L2, data.L1);

    #endregion
}