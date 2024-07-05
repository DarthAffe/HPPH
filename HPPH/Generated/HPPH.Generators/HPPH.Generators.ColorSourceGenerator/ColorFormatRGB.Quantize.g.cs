using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatRGB
{
    #region Methods

    unsafe IColor[] IColorFormat.CreateColorPalette(ReadOnlySpan<byte> data, int paletteSize)
    {
        ColorRGB[] colors = PixelHelper.CreateColorPalette<ColorRGB>(MemoryMarshal.Cast<byte, ColorRGB>(data), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}