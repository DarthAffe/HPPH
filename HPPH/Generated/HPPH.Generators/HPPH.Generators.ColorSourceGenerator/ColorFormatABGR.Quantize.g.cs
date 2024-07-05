using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatABGR
{
    #region Methods

    unsafe IColor[] IColorFormat.CreateColorPalette(ReadOnlySpan<byte> data, int paletteSize)
    {
        ColorABGR[] colors = PixelHelper.CreateColorPalette<ColorABGR>(MemoryMarshal.Cast<byte, ColorABGR>(data), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}