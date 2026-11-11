#nullable enable

namespace HPPH;

public readonly partial struct ColorBGR
{
    #region Methods

    static ColorSum IColor<ColorBGR>.ToColorSum(Generic4LongData data) => new(data.L3, data.L2, data.L1, data.L4);

    #endregion
}