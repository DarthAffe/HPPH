#nullable enable

namespace HPPH;

public sealed partial class ColorFormatRGB
{
    #region Methods

    ColorSum IColorFormat.Sum(IImage image) => PixelHelper.Sum(image.AsRefImage<ColorRGB>());
    IColor IColorFormat.Average(IImage image) => PixelHelper.Average(image.AsRefImage<ColorRGB>());

    ColorSum IColorFormat.ToColorSum(Generic4LongData data) => new(data.L1, data.L2, data.L3, data.L4);

    #endregion
}