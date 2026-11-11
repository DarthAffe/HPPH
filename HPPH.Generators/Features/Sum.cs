using System.Collections.Generic;
using System.Collections.Immutable;

namespace HPPH.Generators;

internal class Sum : IGeneratorFeature
{
    public IEnumerable<(string name, string source)> GenerateFor(ColorFormatData colorFormat)
    {
        yield return ($"ColorFormat{colorFormat.Format}.Sum", GenerateColorFormatSum(colorFormat));
    }

    public IEnumerable<(string name, string source)> GenerateFor(ImmutableArray<ColorFormatData> colorFormats)
    {
        yield return ("IColorFormat.Sum", GenerateColorFormatInterfaceSum());
    }

    private static string GenerateColorFormatSum(ColorFormatData colorFormat)
    {
        // The kernels return the sums in byte order. For 3-byte formats L4 holds the alpha sum (255 per pixel).
        int r = colorFormat.GetBytePosition("r");
        int g = colorFormat.GetBytePosition("g");
        int b = colorFormat.GetBytePosition("b");
        int a = colorFormat.Bpp == 4 ? colorFormat.GetBytePosition("a") : 4;

        return $$"""
                 #nullable enable

                 namespace HPPH;

                 public sealed partial class ColorFormat{{colorFormat.Format}}
                 {
                     #region Methods

                     ColorSum IColorFormat.ToColorSum(Generic4LongData data) => new(data.L{{r}}, data.L{{g}}, data.L{{b}}, data.L{{a}});

                     #endregion
                 }
                 """;
    }

    private static string GenerateColorFormatInterfaceSum()
    {
        return """
               #nullable enable

               namespace HPPH;

               public partial interface IColorFormat
               {
                   internal ColorSum ToColorSum(Generic4LongData data);
               }
               """;
    }
}
