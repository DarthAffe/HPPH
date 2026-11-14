using System.Collections.Generic;
using System.Collections.Immutable;

namespace HPPH.Generators;

internal class MinMax : IGeneratorFeature
{
    public IEnumerable<(string name, string source)> GenerateFor(ColorFormatData colorFormat)
    {
        yield return ($"{colorFormat.TypeName}.MinMax", GenerateColorMinMax(colorFormat));
    }

    public IEnumerable<(string name, string source)> GenerateFor(ImmutableArray<ColorFormatData> colorFormats)
    {
        yield return ("IColor.MinMax", GenerateColorInterfaceMinMax());
    }

    private static string GenerateColorMinMax(ColorFormatData colorFormat)
    {
        string Channel(string entry)
        {
            int position = colorFormat.GetBytePosition(entry);
            return position == 0 ? "byte.MaxValue, byte.MaxValue" : $"data.B{position}Min, data.B{position}Max";
        }

        string create = $"new({Channel("r")}, {Channel("g")}, {Channel("b")}, {Channel("a")})";
        string create3Byte = colorFormat.Bpp == 3 ? create : "throw new NotSupportedException()";
        string create4Byte = colorFormat.Bpp == 4 ? create : "throw new NotSupportedException()";

        return $$"""
                 #nullable enable

                 namespace HPPH;

                 public readonly partial struct {{colorFormat.TypeName}}
                 {
                     #region Methods

                     static ColorMinMax IColor<{{colorFormat.TypeName}}>.ToColorMinMax(Generic3ByteMinMax data) => {{create3Byte}};
                     static ColorMinMax IColor<{{colorFormat.TypeName}}>.ToColorMinMax(Generic4ByteMinMax data) => {{create4Byte}};

                     #endregion
                 }
                 """;
    }

    private static string GenerateColorInterfaceMinMax()
    {
        return """
               #nullable enable

               namespace HPPH;

               public partial interface IColor<out TSelf>
               {
                   internal static abstract ColorMinMax ToColorMinMax(Generic3ByteMinMax data);
                   internal static abstract ColorMinMax ToColorMinMax(Generic4ByteMinMax data);
               }
               """;
    }
}
