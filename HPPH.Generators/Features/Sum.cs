using System.Collections.Generic;
using System.Collections.Immutable;

namespace HPPH.Generators;

internal class Sum : IGeneratorFeature
{
    public IEnumerable<(string name, string source)> GenerateFor(ColorFormatData colorFormat)
    {
        yield return ($"{colorFormat.TypeName}.Sum", GenerateColorSum(colorFormat));
    }

    public IEnumerable<(string name, string source)> GenerateFor(ImmutableArray<ColorFormatData> colorFormats)
    {
        yield return ("IColor.Sum", GenerateColorInterfaceSum());
    }

    private static string GenerateColorSum(ColorFormatData colorFormat)
    {
        // The kernels return the sums in byte order. For 3-byte formats L4 holds the alpha sum (255 per pixel).
        int r = colorFormat.GetBytePosition("r");
        int g = colorFormat.GetBytePosition("g");
        int b = colorFormat.GetBytePosition("b");
        int a = colorFormat.Bpp == 4 ? colorFormat.GetBytePosition("a") : 4;

        return $$"""
                 #nullable enable

                 namespace HPPH;

                 public readonly partial struct {{colorFormat.TypeName}}
                 {
                     #region Methods

                     static ColorSum IColor<{{colorFormat.TypeName}}>.ToColorSum(Generic4LongData data) => new(data.L{{r}}, data.L{{g}}, data.L{{b}}, data.L{{a}});

                     #endregion
                 }
                 """;
    }

    private static string GenerateColorInterfaceSum()
    {
        return """
               #nullable enable

               namespace HPPH;

               public partial interface IColor<out TSelf>
               {
                   internal static abstract ColorSum ToColorSum(Generic4LongData data);
               }
               """;
    }
}
