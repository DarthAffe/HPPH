#nullable enable

namespace HPPH;

public sealed partial class ColorFormatARGB
{
    #region Methods

    ColorMinMax IColorFormat.ToColorMinMax(Generic3ByteMinMax data) => throw new NotSupportedException();
    ColorMinMax IColorFormat.ToColorMinMax(Generic4ByteMinMax data) => new(data.B2Min, data.B2Max, data.B3Min, data.B3Max, data.B4Min, data.B4Max, data.B1Min, data.B1Max);

    #endregion
}