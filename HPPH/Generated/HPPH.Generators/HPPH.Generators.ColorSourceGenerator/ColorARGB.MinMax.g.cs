#nullable enable

namespace HPPH;

public readonly partial struct ColorARGB
{
    #region Methods

    static ColorMinMax IColor<ColorARGB>.ToColorMinMax(Generic3ByteMinMax data) => throw new NotSupportedException();
    static ColorMinMax IColor<ColorARGB>.ToColorMinMax(Generic4ByteMinMax data) => new(data.B2Min, data.B2Max, data.B3Min, data.B3Max, data.B4Min, data.B4Max, data.B1Min, data.B1Max);

    #endregion
}