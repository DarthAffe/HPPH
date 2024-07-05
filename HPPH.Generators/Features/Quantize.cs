using System.Collections.Generic;
using System.Collections.Immutable;

namespace HPPH.Generators;

internal class Quantize : IGeneratorFeature
{
    public IEnumerable<(string name, string source)> GenerateFor(ColorFormatData colorFormat)
    {
        yield return ($"ColorFormat{colorFormat.Format}.Quantize", GenerateColorFormatQuantize(colorFormat));
    }

    public IEnumerable<(string name, string source)> GenerateFor(ImmutableArray<ColorFormatData> colorFormats)
    {
        yield return ("IColorFormat.Quantize", GenerateColorFormatInterfaceQuantize());
    }
    
    private static string GenerateColorFormatQuantize(ColorFormatData colorFormat)
    {
        return $$"""
                 using System.Runtime.InteropServices;
                 
                 namespace HPPH;

                 public sealed partial class ColorFormat{{colorFormat.Format}}
                 {
                     #region Methods
                 
                     unsafe IColor[] IColorFormat.CreateColorPalette(ReadOnlySpan<byte> data, int paletteSize)
                     {
                         Color{{colorFormat.Format}}[] colors = PixelHelper.CreateColorPalette<Color{{colorFormat.Format}}>(MemoryMarshal.Cast<byte, Color{{colorFormat.Format}}>(data), paletteSize);
                         
                         IColor[] result = new IColor[colors.Length];
                         for(int i = 0; i < colors.Length; i++)
                             result[i] = colors[i];
                     
                         return result;
                     }
                 
                     #endregion
                 }
                 """;
    }

    private static string GenerateColorFormatInterfaceQuantize()
    {
        return """
               namespace HPPH;
               
               public partial interface IColorFormat
               {
                   internal IColor[] CreateColorPalette(ReadOnlySpan<byte> data, int paletteSize);
               }
               """;
    }
}