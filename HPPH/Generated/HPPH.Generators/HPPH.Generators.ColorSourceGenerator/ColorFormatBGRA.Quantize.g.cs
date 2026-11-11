#nullable enable

namespace HPPH;

public sealed partial class ColorFormatBGRA
{
    #region Methods

    IColor[] IColorFormat.CreateWuColorPalette(IImage image, int paletteSize)
    {
        ColorBGRA[] colors = PixelHelper.CreateWuColorPalette(image.AsRefImage<ColorBGRA>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    IColor[] IColorFormat.CreateMedianCutColorPalette(IImage image, int paletteSize)
    {
        ColorBGRA[] colors = PixelHelper.CreateMedianCutColorPalette(image.AsRefImage<ColorBGRA>(), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}