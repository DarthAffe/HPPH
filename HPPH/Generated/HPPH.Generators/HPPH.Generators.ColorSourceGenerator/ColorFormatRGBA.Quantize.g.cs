#nullable enable

namespace HPPH;

public sealed partial class ColorFormatRGBA
{
    #region Methods

    IColor[] IColorFormat.CreateWuColorPalette(IImage image, int paletteSize)
    {
        ColorRGBA[] colors = PixelHelper.CreateWuColorPalette(image.AsRefImage<ColorRGBA>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    IColor[] IColorFormat.CreateMedianCutColorPalette(IImage image, int paletteSize)
    {
        ColorRGBA[] colors = PixelHelper.CreateMedianCutColorPalette(image.AsRefImage<ColorRGBA>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}