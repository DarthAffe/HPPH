#nullable enable

namespace HPPH;

public sealed partial class ColorFormatBGR
{
    #region Methods

    IColor[] IColorFormat.CreateWuColorPalette(IImage image, int paletteSize)
    {
        ColorBGR[] colors = PixelHelper.CreateWuColorPalette(image.AsRefImage<ColorBGR>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    IColor[] IColorFormat.CreateMedianCutColorPalette(IImage image, int paletteSize)
    {
        ColorBGR[] colors = PixelHelper.CreateMedianCutColorPalette(image.AsRefImage<ColorBGR>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}