#nullable enable

namespace HPPH;

public readonly partial struct ColorARGB
{
    #region Methods

    static ColorSum IColor<ColorARGB>.ToColorSum(Generic4LongData data) => new(data.L2, data.L3, data.L4, data.L1);

    #endregion
}