#nullable enable

namespace HPPH;

public readonly partial struct ColorABGR
{
    #region Methods

    static ColorMinMax IColor<ColorABGR>.ToColorMinMax(Generic3ByteMinMax data) => throw new NotSupportedException();
    static ColorMinMax IColor<ColorABGR>.ToColorMinMax(Generic4ByteMinMax data) => new(data.B4Min, data.B4Max, data.B3Min, data.B3Max, data.B2Min, data.B2Max, data.B1Min, data.B1Max);

    #endregion
}