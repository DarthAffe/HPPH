#nullable enable

namespace HPPH;

public readonly partial struct ColorBGRA
{
    #region Methods

    static ColorSum IColor<ColorBGRA>.ToColorSum(Generic4LongData data) => new(data.L3, data.L2, data.L1, data.L4);

    #endregion
}