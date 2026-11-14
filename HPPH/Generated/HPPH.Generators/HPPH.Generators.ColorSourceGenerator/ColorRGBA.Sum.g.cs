#nullable enable

namespace HPPH;

public readonly partial struct ColorRGBA
{
    #region Methods

    static ColorSum IColor<ColorRGBA>.ToColorSum(Generic4LongData data) => new(data.L1, data.L2, data.L3, data.L4);

    #endregion
}