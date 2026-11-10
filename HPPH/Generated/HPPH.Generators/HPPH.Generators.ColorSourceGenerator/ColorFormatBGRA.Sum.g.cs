#nullable enable

namespace HPPH;

public sealed partial class ColorFormatBGRA
{
    #region Methods

    ColorSum IColorFormat.Sum(IImage image) => PixelHelper.Sum(image.AsRefImage<ColorBGRA>());
    IColor IColorFormat.Average(IImage image) => PixelHelper.Average(image.AsRefImage<ColorBGRA>());

    ColorSum IColorFormat.ToColorSum(Generic4LongData data) => new(data.L3, data.L2, data.L1, data.L4);

    #endregion
}