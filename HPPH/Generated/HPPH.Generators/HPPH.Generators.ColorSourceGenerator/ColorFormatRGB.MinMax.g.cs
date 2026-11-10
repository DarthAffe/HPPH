#nullable enable

namespace HPPH;

public sealed partial class ColorFormatRGB
{
    #region Methods

    ColorMinMax IColorFormat.MinMax(IImage image) => PixelHelper.MinMax(image.AsRefImage<ColorRGB>());

    ColorMinMax IColorFormat.ToColorMinMax(Generic3ByteMinMax data) => new(data.B1Min, data.B1Max, data.B2Min, data.B2Max, data.B3Min, data.B3Max, byte.MaxValue, byte.MaxValue);
    ColorMinMax IColorFormat.ToColorMinMax(Generic4ByteMinMax data) => throw new NotSupportedException();

    #endregion
}