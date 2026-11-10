using System.Collections.Generic;
using System.Collections.Immutable;

namespace HPPH.Generators;

internal class MinMax : IGeneratorFeature
{
    public IEnumerable<(string name, string source)> GenerateFor(ColorFormatData colorFormat)
    {
        yield return ($"ColorFormat{colorFormat.Format}.MinMax", GenerateColorFormatMinMax(colorFormat));
    }

    public IEnumerable<(string name, string source)> GenerateFor(ImmutableArray<ColorFormatData> colorFormats)
    {
        yield return ("IColorFormat.MinMax", GenerateColorFormatInterfaceMinMax());
    }

    private static string GenerateColorFormatMinMax(ColorFormatData colorFormat)
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

                 public sealed partial class ColorFormat{{colorFormat.Format}}
                 {
                     #region Methods

                     ColorMinMax IColorFormat.MinMax(IImage image) => PixelHelper.MinMax(image.AsRefImage<Color{{colorFormat.Format}}>());

                     ColorMinMax IColorFormat.ToColorMinMax(Generic3ByteMinMax data) => {{create3Byte}};
                     ColorMinMax IColorFormat.ToColorMinMax(Generic4ByteMinMax data) => {{create4Byte}};

                     #endregion
                 }
                 """;
    }

    private static string GenerateColorFormatInterfaceMinMax()
    {
        return """
               #nullable enable

               namespace HPPH;

               public partial interface IColorFormat
               {
                   internal ColorMinMax MinMax(IImage image);

                   internal ColorMinMax ToColorMinMax(Generic3ByteMinMax data);
                   internal ColorMinMax ToColorMinMax(Generic4ByteMinMax data);
               }
               """;
    }
}
