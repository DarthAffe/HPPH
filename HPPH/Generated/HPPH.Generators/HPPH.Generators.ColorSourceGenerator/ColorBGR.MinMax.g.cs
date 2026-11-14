#nullable enable

namespace HPPH;

public readonly partial struct ColorBGR
{
    #region Methods

    static ColorMinMax IColor<ColorBGR>.ToColorMinMax(Generic3ByteMinMax data) => new(data.B3Min, data.B3Max, data.B2Min, data.B2Max, data.B1Min, data.B1Max, byte.MaxValue, byte.MaxValue);
    static ColorMinMax IColor<ColorBGR>.ToColorMinMax(Generic4ByteMinMax data) => throw new NotSupportedException();

    #endregion
}