#nullable enable

namespace HPPH;

public sealed partial class ColorFormatRGBA
{
    #region Methods

    ColorSum IColorFormat.Sum(IImage image) => PixelHelper.Sum(image.AsRefImage<ColorRGBA>());
    IColor IColorFormat.Average(IImage image) => PixelHelper.Average(image.AsRefImage<ColorRGBA>());

    ColorSum IColorFormat.ToColorSum(Generic4LongData data) => new(data.L1, data.L2, data.L3, data.L4);

    #endregion
}