using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatBGRA
{
    #region Methods

    unsafe IColor[] IColorFormat.CreateColorPalette(ReadOnlySpan<byte> data, int paletteSize)
    {
        ColorBGRA[] colors = PixelHelper.CreateColorPalette<ColorBGRA>(MemoryMarshal.Cast<byte, ColorBGRA>(data), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    unsafe IColor[] IColorFormat.CreateSimpleColorPalette(ReadOnlySpan<byte> data, int paletteSize)
    {
        ColorBGRA[] colors = PixelHelper.CreateSimpleColorPalette<ColorBGRA>(MemoryMarshal.Cast<byte, ColorBGRA>(data), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}