#nullable enable

namespace HPPH;

public sealed partial class ColorFormatBGR
{
    #region Methods

    ColorMinMax IColorFormat.MinMax(IImage image) => PixelHelper.MinMax(image.AsRefImage<ColorBGR>());

    ColorMinMax IColorFormat.ToColorMinMax(Generic3ByteMinMax data) => new(data.B3Min, data.B3Max, data.B2Min, data.B2Max, data.B1Min, data.B1Max, byte.MaxValue, byte.MaxValue);
    ColorMinMax IColorFormat.ToColorMinMax(Generic4ByteMinMax data) => throw new NotSupportedException();

    #endregion
}