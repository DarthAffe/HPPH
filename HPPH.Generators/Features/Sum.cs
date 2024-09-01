using System.Collections.Generic;
using System.Collections.Immutable;

namespace HPPH.Generators;

internal class Sum : IGeneratorFeature
{
    public IEnumerable<(string name, string source)> GenerateFor(ColorFormatData colorFormat)
    {
        yield return ($"Sum{colorFormat.Format}", GenerateSumStruct(colorFormat));
        yield return ($"ColorFormat{colorFormat.Format}.Sum", GenerateColorFormatSum(colorFormat));
    }

    public IEnumerable<(string name, string source)> GenerateFor(ImmutableArray<ColorFormatData> colorFormats)
    {
        yield return ("IColorFormat.Sum", GenerateColorFormatInterfaceSum());
    }

    private static string GenerateSumStruct(ColorFormatData colorFormat)
     => colorFormat.Bpp switch
     {
         3 => $$"""
                #nullable enable
                
                // ReSharper disable ConvertToAutoProperty
                // ReSharper disable ConvertToAutoPropertyWhenPossible
                // ReSharper disable ReplaceWithPrimaryConstructorParameter
                
                using System.Diagnostics;
                using System.Runtime.InteropServices;
                
                namespace HPPH;
                
                [DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
                [StructLayout(LayoutKind.Sequential)]
                public readonly partial struct Sum{{colorFormat.Format}}(long {{colorFormat.FirstEntry}}, long {{colorFormat.SecondEntry}}, long {{colorFormat.ThirdEntry}}, long a) : ISum
                {
                    #region Properties & Fields
                
                    private readonly long _{{colorFormat.FirstEntry}} = {{colorFormat.FirstEntry}};
                    private readonly long _{{colorFormat.SecondEntry}} = {{colorFormat.SecondEntry}};
                    private readonly long _{{colorFormat.ThirdEntry}} = {{colorFormat.ThirdEntry}};
                    private readonly long _a = a;
                
                    public long A => _a;
                    public long R => _r;
                    public long G => _g;
                    public long B => _b;
                
                    #endregion
                
                    #region Methods
                
                    /// <inheritdoc />
                    public override string ToString() => $"[A: {A}, R: {R}, G: {G}, B: {B}]";
                
                    #endregion
                }
                
                """,

         4 => $$"""
                #nullable enable
                
                // ReSharper disable ConvertToAutoProperty
                // ReSharper disable ConvertToAutoPropertyWhenPossible
                // ReSharper disable ReplaceWithPrimaryConstructorParameter
                
                using System.Diagnostics;
                using System.Runtime.InteropServices;
                
                namespace HPPH;
                
                [DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
                [StructLayout(LayoutKind.Sequential)]
                public readonly partial struct Sum{{colorFormat.Format}}(long {{colorFormat.FirstEntry}}, long {{colorFormat.SecondEntry}}, long {{colorFormat.ThirdEntry}}, long {{colorFormat.FourthEntry}}) : ISum
                {
                    #region Properties & Fields
                
                    private readonly long _{{colorFormat.FirstEntry}} = {{colorFormat.FirstEntry}};
                    private readonly long _{{colorFormat.SecondEntry}} = {{colorFormat.SecondEntry}};
                    private readonly long _{{colorFormat.ThirdEntry}} = {{colorFormat.ThirdEntry}};
                    private readonly long _{{colorFormat.FourthEntry}} = {{colorFormat.FourthEntry}};
                
                    public long R => _r;
                    public long G => _g;
                    public long B => _b;
                    public long A => _a;
                
                    #endregion
                
                    #region Methods
                
                    /// <inheritdoc />
                    public override string ToString() => $"[A: {A}, R: {R}, G: {G}, B: {B}]";
                
                    #endregion
                }
                
                """,
         _ => null
     };

    private static string GenerateColorFormatSum(ColorFormatData colorFormat)
    {
        return $$"""
                 #nullable enable
                 
                 using System.Runtime.CompilerServices;
                 using System.Runtime.InteropServices;
                 
                 namespace HPPH;

                 public sealed partial class ColorFormat{{colorFormat.Format}}
                 {
                     #region Methods
                 
                     unsafe Generic4LongData IColorFormat.Sum(ReadOnlySpan<byte> data) => PixelHelper.Sum(MemoryMarshal.Cast<byte, Generic{{colorFormat.Bpp}}ByteData>(data));
                     unsafe ISum IColorFormat.ToSum(Generic4LongData data) => Unsafe.BitCast<Generic4LongData, Sum{{colorFormat.Format}}>(data);
                 
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
                   internal Generic4LongData Sum(ReadOnlySpan<byte> data);
                   internal ISum ToSum(Generic4LongData data);
               }
               """;
    }
}