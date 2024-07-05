using System.Runtime.InteropServices;

namespace HPPH;

public sealed partial class ColorFormatARGB
{
    #region Methods

    unsafe IColor[] IColorFormat.CreateColorPalette(ReadOnlySpan<byte> data, int paletteSize)
    {
        ColorARGB[] colors = PixelHelper.CreateColorPalette<ColorARGB>(MemoryMarshal.Cast<byte, ColorARGB>(data), paletteSize);
        
        IColor[] result = new IColor[colors.Length];
        for(int i = 0; i < colors.Length; i++)
            result[i] = colors[i];
    
        return result;
    }

    #endregion
}