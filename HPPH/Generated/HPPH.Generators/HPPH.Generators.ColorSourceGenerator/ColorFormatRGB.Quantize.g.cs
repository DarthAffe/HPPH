#nullable enable

namespace HPPH;

public sealed partial class ColorFormatRGB
{
    #region Methods

    IColor[] IColorFormat.CreateWuColorPalette(IImage image, int paletteSize)
    {
        ColorRGB[] colors = PixelHelper.CreateWuColorPalette(image.AsRefImage<ColorRGB>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    IColor[] IColorFormat.CreateMedianCutColorPalette(IImage image, int paletteSize)
    {
        ColorRGB[] colors = PixelHelper.CreateMedianCutColorPalette(image.AsRefImage<ColorRGB>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}