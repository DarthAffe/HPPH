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
                 #nullable enable
                 
                 namespace HPPH;

                 public sealed partial class ColorFormat{{colorFormat.Format}}
                 {
                     #region Methods
                 
                     IColor[] IColorFormat.CreateWuColorPalette(IImage image, int paletteSize)
                     {
                         Color{{colorFormat.Format}}[] colors = PixelHelper.CreateWuColorPalette(image.AsRefImage<Color{{colorFormat.Format}}>(), paletteSize);
                         
                         IColor[] result = new IColor[colors.Length];
                         for(int i = 0; i < colors.Length; i++)
                             result[i] = colors[i];
                     
                         return result;
                     }
                 
                     IColor[] IColorFormat.CreateMedianCutColorPalette(IImage image, int paletteSize)
                     {
                         Color{{colorFormat.Format}}[] colors = PixelHelper.CreateMedianCutColorPalette(image.AsRefImage<Color{{colorFormat.Format}}>(), paletteSize);
                         
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
               #nullable enable
               
               namespace HPPH;
               
               public partial interface IColorFormat
               {
                   internal IColor[] CreateWuColorPalette(IImage image, int paletteSize);
                   internal IColor[] CreateMedianCutColorPalette(IImage image, int paletteSize);
               }
               """;
    }
}