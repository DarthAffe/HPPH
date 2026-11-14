#nullable enable

namespace HPPH;

public readonly partial struct ColorRGB
{
    #region Methods

    static ColorMinMax IColor<ColorRGB>.ToColorMinMax(Generic3ByteMinMax data) => new(data.B1Min, data.B1Max, data.B2Min, data.B2Max, data.B3Min, data.B3Max, byte.MaxValue, byte.MaxValue);
    static ColorMinMax IColor<ColorRGB>.ToColorMinMax(Generic4ByteMinMax data) => throw new NotSupportedException();

    #endregion
}