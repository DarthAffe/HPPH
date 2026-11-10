#nullable enable

namespace HPPH;

public sealed partial class ColorFormatABGR
{
    #region Methods

    ColorSum IColorFormat.Sum(IImage image) => PixelHelper.Sum(image.AsRefImage<ColorABGR>());
    IColor IColorFormat.Average(IImage image) => PixelHelper.Average(image.AsRefImage<ColorABGR>());

    ColorSum IColorFormat.ToColorSum(Generic4LongData data) => new(data.L4, data.L3, data.L2, data.L1);

    #endregion
}