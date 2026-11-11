#nullable enable

namespace HPPH;

public sealed partial class ColorFormatABGR
{
    #region Methods

    IColor[] IColorFormat.CreateWuColorPalette(IImage image, int paletteSize)
    {
        ColorABGR[] colors = PixelHelper.CreateWuColorPalette(image.AsRefImage<ColorABGR>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    IColor[] IColorFormat.CreateMedianCutColorPalette(IImage image, int paletteSize)
    {
        ColorABGR[] colors = PixelHelper.CreateMedianCutColorPalette(image.AsRefImage<ColorABGR>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}