#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatBGR
{
    #region Methods

    unsafe IColor[] IColorFormat.CreateColorPalette(ReadOnlySpan<byte> data, int paletteSize)
    {
        ColorBGR[] colors = PixelHelper.CreateColorPalette<ColorBGR>(MemoryMarshal.Cast<byte, ColorBGR>(data), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    unsafe IColor[] IColorFormat.CreateSimpleColorPalette(ReadOnlySpan<byte> data, int paletteSize)
    {
        ColorBGR[] colors = PixelHelper.CreateSimpleColorPalette<ColorBGR>(MemoryMarshal.Cast<byte, ColorBGR>(data), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}