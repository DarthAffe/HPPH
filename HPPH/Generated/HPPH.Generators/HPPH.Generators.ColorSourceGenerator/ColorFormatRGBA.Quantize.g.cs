#nullable enable

using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatRGBA
{
    #region Methods

    unsafe IColor[] IColorFormat.CreateColorPalette(ReadOnlySpan<byte> data, int paletteSize)
    {
        ColorRGBA[] colors = PixelHelper.CreateColorPalette<ColorRGBA>(MemoryMarshal.Cast<byte, ColorRGBA>(data), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    unsafe IColor[] IColorFormat.CreateSimpleColorPalette(ReadOnlySpan<byte> data, int paletteSize)
    {
        ColorRGBA[] colors = PixelHelper.CreateSimpleColorPalette<ColorRGBA>(MemoryMarshal.Cast<byte, ColorRGBA>(data), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}