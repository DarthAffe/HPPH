#nullable enable

namespace HPPH;

public sealed partial class ColorFormatBGR
{
    #region Methods

    ColorSum IColorFormat.ToColorSum(Generic4LongData data) => new(data.L3, data.L2, data.L1, data.L4);

    #endregion
}