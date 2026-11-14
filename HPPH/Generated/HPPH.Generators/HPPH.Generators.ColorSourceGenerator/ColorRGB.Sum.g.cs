#nullable enable

namespace HPPH;

public readonly partial struct ColorRGB
{
    #region Methods

    static ColorSum IColor<ColorRGB>.ToColorSum(Generic4LongData data) => new(data.L1, data.L2, data.L3, data.L4);

    #endregion
}