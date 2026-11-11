#nullable enable

namespace HPPH;

public sealed partial class ColorFormatARGB
{
    #region Methods

    IColor[] IColorFormat.CreateWuColorPalette(IImage image, int paletteSize)
    {
        ColorARGB[] colors = PixelHelper.CreateWuColorPalette(image.AsRefImage<ColorARGB>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    IColor[] IColorFormat.CreateMedianCutColorPalette(IImage image, int paletteSize)
    {
        ColorARGB[] colors = PixelHelper.CreateMedianCutColorPalette(image.AsRefImage<ColorARGB>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}