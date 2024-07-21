using System.Collections.Generic;
using System.Collections.Immutable;

namespace HPPH.Generators;

internal class Average : IGeneratorFeature
{
    public IEnumerable<(string name, string source)> GenerateFor(ColorFormatData colorFormat)
    {
        yield return ($"ColorFormat{colorFormat.Format}.Average", GenerateColorFormatAverage(colorFormat));
    }

    public IEnumerable<(string name, string source)> GenerateFor(ImmutableArray<ColorFormatData> colorFormats)
    {
        yield return ("IColorFormat.Average", GenerateColorFormatInterfaceAverage());
    }

    private static string GenerateColorFormatAverage(ColorFormatData colorFormat)
    {
        return $$"""
                 #nullable enable
                 
                 using System.Runtime.InteropServices;
                 
                 namespace HPPH;

                 public sealed partial class ColorFormat{{colorFormat.Format}}
                 {
                     #region Methods
                 
                     unsafe IColor IColorFormat.Average(ReadOnlySpan<byte> data) => PixelHelper.Average(MemoryMarshal.Cast<byte, Color{{colorFormat.Format}}>(data));
                 
                     #endregion
                 }
                 """;
    }

    private static string GenerateColorFormatInterfaceAverage()
    {
        return """
               #nullable enable
               
               namespace HPPH;
               
               public partial interface IColorFormat
               {
                   internal IColor Average(ReadOnlySpan<byte> data);
               }
               """;
    }
}