#nullable enable

namespace HPPH;

public readonly partial struct ColorABGR
{
    #region Methods

    static ColorSum IColor<ColorABGR>.ToColorSum(Generic4LongData data) => new(data.L4, data.L3, data.L2, data.L1);

    #endregion
}