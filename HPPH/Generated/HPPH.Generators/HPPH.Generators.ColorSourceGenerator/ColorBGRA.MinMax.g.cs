#nullable enable

namespace HPPH;

public readonly partial struct ColorBGRA
{
    #region Methods

    static ColorMinMax IColor<ColorBGRA>.ToColorMinMax(Generic3ByteMinMax data) => throw new NotSupportedException();
    static ColorMinMax IColor<ColorBGRA>.ToColorMinMax(Generic4ByteMinMax data) => new(data.B3Min, data.B3Max, data.B2Min, data.B2Max, data.B1Min, data.B1Max, data.B4Min, data.B4Max);

    #endregion
}