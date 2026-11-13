#nullable enable

namespace HPPH;

public readonly partial struct ColorRGBA
{
    #region Methods

    static ColorMinMax IColor<ColorRGBA>.ToColorMinMax(Generic3ByteMinMax data) => throw new NotSupportedException();
    static ColorMinMax IColor<ColorRGBA>.ToColorMinMax(Generic4ByteMinMax data) => new(data.B1Min, data.B1Max, data.B2Min, data.B2Max, data.B3Min, data.B3Max, data.B4Min, data.B4Max);

    #endregion
}