using System.Collections.Generic;
using System.Collections.Immutable;

namespace HPPH.Generators;

internal class MinMax : IGeneratorFeature
{
    public IEnumerable<(string name, string source)> GenerateFor(ColorFormatData colorFormat)
    {
        yield return ($"MinMax{colorFormat.Format}", GenerateMinMaxStruct(colorFormat));
        yield return ($"ColorFormat{colorFormat.Format}.MinMax", GenerateColorFormatMinMax(colorFormat));
    }

    public IEnumerable<(string name, string source)> GenerateFor(ImmutableArray<ColorFormatData> colorFormats)
    {
        yield return ("IColorFormat.MinMax", GenerateColorFormatInterfaceMinMax());
    }

    private static string GenerateMinMaxStruct(ColorFormatData colorFormat)
     => colorFormat.Bpp switch
     {
         3 => $$"""
                 #nullable enable
                 
                 // ReSharper disable ConvertToAutoProperty
                 // ReSharper disable ReplaceWithPrimaryConstructorParameter
                 
                 using System.Diagnostics;
                 using System.Runtime.InteropServices;
                 
                 namespace HPPH;
                 
                 [DebuggerDisplay("[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]")]
                 [StructLayout(LayoutKind.Sequential)]
                 public readonly partial struct MinMax{{colorFormat.Format}}(byte {{colorFormat.FirstEntry}}Min, byte {{colorFormat.FirstEntry}}Max, byte {{colorFormat.SecondEntry}}Min, byte {{colorFormat.SecondEntry}}Max, byte {{colorFormat.ThirdEntry}}Min, byte {{colorFormat.ThirdEntry}}Max) : IMinMax
                 {
                     #region Properties & Fields
                 
                     private readonly byte _{{colorFormat.FirstEntry}}Min = {{colorFormat.FirstEntry}}Min;
                     private readonly byte _{{colorFormat.FirstEntry}}Max = {{colorFormat.FirstEntry}}Max;
                     
                     private readonly byte _{{colorFormat.SecondEntry}}Min = {{colorFormat.SecondEntry}}Min;
                     private readonly byte _{{colorFormat.SecondEntry}}Max = {{colorFormat.SecondEntry}}Max;
                     
                     private readonly byte _{{colorFormat.ThirdEntry}}Min = {{colorFormat.ThirdEntry}}Min;
                     private readonly byte _{{colorFormat.ThirdEntry}}Max = {{colorFormat.ThirdEntry}}Max;
                 
                     public byte RedMin => _rMin;
                     public byte RedMax => _rMax;
                     
                     public byte GreenMin => _gMin;
                     public byte GreenMax => _gMax;
                     
                     public byte BlueMin => _bMin;
                     public byte BlueMax => _bMax;
                     
                     public byte AlphaMin => byte.MaxValue;
                     public byte AlphaMax => byte.MaxValue;
                     
                     public byte RedRange => (byte)(_rMax - _rMin);
                     public byte GreenRange => (byte)(_gMax - _gMin);
                     public byte BlueRange => (byte)(_bMax - _bMin);
                     public byte AlphaRange => 0;
                 
                     #endregion
                 
                     #region Methods
                 
                     /// <inheritdoc />
                     public override string ToString() => $"[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]";
                 
                     #endregion
                 }
                 
                 """,

         4 => $$"""
                #nullable enable
                
                // ReSharper disable ConvertToAutoProperty
                // ReSharper disable ReplaceWithPrimaryConstructorParameter
                
                using System.Diagnostics;
                using System.Runtime.InteropServices;
                
                namespace HPPH;
                
                [DebuggerDisplay("[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]")]
                [StructLayout(LayoutKind.Sequential)]
                public readonly partial struct MinMax{{colorFormat.Format}}(byte {{colorFormat.FirstEntry}}Min, byte {{colorFormat.FirstEntry}}Max, byte {{colorFormat.SecondEntry}}Min, byte {{colorFormat.SecondEntry}}Max, byte {{colorFormat.ThirdEntry}}Min, byte {{colorFormat.ThirdEntry}}Max, byte {{colorFormat.FourthEntry}}Min, byte {{colorFormat.FourthEntry}}Max) : IMinMax
                {
                    #region Properties & Fields
                
                    private readonly byte _{{colorFormat.FirstEntry}}Min = {{colorFormat.FirstEntry}}Min;
                    private readonly byte _{{colorFormat.FirstEntry}}Max = {{colorFormat.FirstEntry}}Max;
                    
                    private readonly byte _{{colorFormat.SecondEntry}}Min = {{colorFormat.SecondEntry}}Min;
                    private readonly byte _{{colorFormat.SecondEntry}}Max = {{colorFormat.SecondEntry}}Max;
                    
                    private readonly byte _{{colorFormat.ThirdEntry}}Min = {{colorFormat.ThirdEntry}}Min;
                    private readonly byte _{{colorFormat.ThirdEntry}}Max = {{colorFormat.ThirdEntry}}Max;
                
                    private readonly byte _{{colorFormat.FourthEntry}}Min = {{colorFormat.FourthEntry}}Min;
                    private readonly byte _{{colorFormat.FourthEntry}}Max = {{colorFormat.FourthEntry}}Max;
                
                    public byte RedMin => _rMin;
                    public byte RedMax => _rMax;
                    
                    public byte GreenMin => _gMin;
                    public byte GreenMax => _gMax;
                    
                    public byte BlueMin => _bMin;
                    public byte BlueMax => _bMax;
                    
                    public byte AlphaMin => _aMin;
                    public byte AlphaMax => _aMax;
                    
                    public byte RedRange => (byte)(_rMax - _rMin);
                    public byte GreenRange => (byte)(_gMax - _gMin);
                    public byte BlueRange => (byte)(_bMax - _bMin);
                    public byte AlphaRange => (byte)(_aMax - _aMin);
                    
                    #endregion
                
                    #region Methods
                
                    /// <inheritdoc />
                    public override string ToString() => $"[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]";
                
                    #endregion
                }
                
                """,
         _ => null
     };

    private static string GenerateColorFormatMinMax(ColorFormatData colorFormat)
    {
        return $$"""
                 #nullable enable
                 
                 using System.Runtime.InteropServices;
                 
                 namespace HPPH;

                 public sealed partial class ColorFormat{{colorFormat.Format}}
                 {
                     #region Methods
                 
                     unsafe IMinMax IColorFormat.MinMax(ReadOnlySpan<byte> data) => PixelHelper.MinMax<Color{{colorFormat.Format}}, MinMax{{colorFormat.Format}}>(MemoryMarshal.Cast<byte, Color{{colorFormat.Format}}>(data));
                 
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
                   internal IMinMax MinMax(ReadOnlySpan<byte> data);
               }
               """;
    }
}