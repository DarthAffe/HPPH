﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace HPPH.Generators;

internal class Colors : IGeneratorFeature
{
    public IEnumerable<(string name, string source)> GenerateFor(ColorFormatData colorFormat)
    {
        string colorStructCode = GenerateColorStructCode(colorFormat);
        if (!string.IsNullOrWhiteSpace(colorStructCode))
        {
            yield return (colorFormat.TypeName, colorStructCode!);
            yield return ($"ColorFormat{colorFormat.Format}", GenerateColorFormatCode(colorFormat));
        }
    }

    public IEnumerable<(string name, string source)> GenerateFor(ImmutableArray<ColorFormatData> colorFormats)
    {
        yield return ("IColorFormat.Instances", GenerateColorFormats(colorFormats));
    }

    private static string GenerateColorStructCode(ColorFormatData colorFormat)
     => colorFormat.Bpp switch
     {
         3 => $$"""
                #nullable enable
                
                // ReSharper disable ConvertToAutoProperty
                // ReSharper disable ConvertToAutoPropertyWhenPossible
                // ReSharper disable ReplaceWithPrimaryConstructorParameter
                
                using System.Diagnostics;
                using System.Runtime.CompilerServices;
                using System.Runtime.InteropServices;
                
                namespace HPPH;
                
                /// <summary>
                /// Represents a color in 24 bit {{colorFormat.Format}}-format.
                /// </summary>
                /// <remarks>
                /// Initializes a new instance of the <see cref="{{colorFormat.TypeName}}"/> class.
                /// </remarks>
                /// <param name="{{colorFormat.FirstEntry}}">The {{colorFormat.FirstEntryName}}-component of the color.</param>
                /// <param name="{{colorFormat.SecondEntry}}">The {{colorFormat.SecondEntryName}}-component of the color.</param>
                /// <param name="{{colorFormat.ThirdEntry}}">The {{colorFormat.ThirdEntryName}}-component of the color.</param>
                [DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
                [SkipLocalsInit]
                [StructLayout(LayoutKind.Sequential)]
                public readonly partial struct {{colorFormat.TypeName}}(byte {{colorFormat.FirstEntry}}, byte {{colorFormat.SecondEntry}}, byte {{colorFormat.ThirdEntry}}): IColor, IEquatable<{{colorFormat.TypeName}}>
                {
                    #region Properties & Fields
                
                    /// <inheritdoc />
                    public static IColorFormat ColorFormat => IColorFormat.{{colorFormat.Format}};
                
                    private readonly byte _{{colorFormat.FirstEntry}} = {{colorFormat.FirstEntry}};
                    private readonly byte _{{colorFormat.SecondEntry}} = {{colorFormat.SecondEntry}};
                    private readonly byte _{{colorFormat.ThirdEntry}} = {{colorFormat.ThirdEntry}};
                
                    /// <inheritdoc />
                    public byte R => _r;
                    
                    /// <inheritdoc />
                    public byte G => _g;
                
                    /// <inheritdoc />
                    public byte B => _b;
                    
                    /// <inheritdoc />
                    public byte A => byte.MaxValue;
                
                    #endregion
                
                    #region Operators
                    
                    public static bool operator ==({{colorFormat.TypeName}} left, {{colorFormat.TypeName}} right) => left.Equals(right);
                    public static bool operator !=({{colorFormat.TypeName}} left, {{colorFormat.TypeName}} right) => !left.Equals(right);
                    
                    #endregion
                
                    #region Methods
                
                    /// <inheritdoc />
                    public bool Equals(IColor? other) => (other != null) && (R == other.R) && (G == other.G) && (B == other.B) && (A == other.A);
                
                    /// <inheritdoc />
                    public bool Equals({{colorFormat.TypeName}} other) => (_{{colorFormat.FirstEntry}} == other._{{colorFormat.FirstEntry}}) && (_{{colorFormat.SecondEntry}} == other._{{colorFormat.SecondEntry}}) && (_{{colorFormat.ThirdEntry}} == other._{{colorFormat.ThirdEntry}});
                    
                    /// <inheritdoc />
                    public override bool Equals(object? obj) => obj is {{colorFormat.TypeName}} other && Equals(other);
                    
                    /// <inheritdoc />
                    public override int GetHashCode() => HashCode.Combine(_{{colorFormat.FirstEntry}}, _{{colorFormat.SecondEntry}}, _{{colorFormat.ThirdEntry}});
                
                    /// <inheritdoc />
                    public override string ToString() => $"[A: {A}, R: {R}, G: {G}, B: {B}]";
                
                    /// <inheritdoc />
                    public static IColor Create(byte r, byte g, byte b, byte a) => new {{colorFormat.TypeName}}({{colorFormat.FirstEntry}}, {{colorFormat.SecondEntry}}, {{colorFormat.ThirdEntry}});
                
                    #endregion
                }
                
                """,

         4 => $$"""
                #nullable enable
                
                // ReSharper disable ConvertToAutoProperty
                // ReSharper disable ConvertToAutoPropertyWhenPossible
                // ReSharper disable ReplaceWithPrimaryConstructorParameter
                
                using System.Diagnostics;
                using System.Runtime.CompilerServices;
                using System.Runtime.InteropServices;
                
                namespace HPPH;
                
                /// <summary>
                /// Represents a color in 32 bit {{colorFormat.Format}}-format.
                /// </summary>
                /// <remarks>
                /// Initializes a new instance of the <see cref="{{colorFormat.TypeName}}"/> class.
                /// </remarks>
                /// <param name="{{colorFormat.FirstEntry}}">The {{colorFormat.FirstEntryName}}-component of the color.</param>
                /// <param name="{{colorFormat.SecondEntry}}">The {{colorFormat.SecondEntryName}}-component of the color.</param>
                /// <param name="{{colorFormat.ThirdEntry}}">The {{colorFormat.ThirdEntryName}}-component of the color.</param>
                /// <param name="{{colorFormat.FourthEntry}}">The {{colorFormat.FourthEntryName}}-component of the color.</param>
                [DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
                [SkipLocalsInit]
                [StructLayout(LayoutKind.Sequential)]
                public readonly partial struct {{colorFormat.TypeName}}(byte {{colorFormat.FirstEntry}}, byte {{colorFormat.SecondEntry}}, byte {{colorFormat.ThirdEntry}}, byte {{colorFormat.FourthEntry}}) : IColor, IEquatable<{{colorFormat.TypeName}}>
                {
                    #region Properties & Fields
                
                    /// <inheritdoc />
                    public static IColorFormat ColorFormat => IColorFormat.{{colorFormat.Format}};
                
                    private readonly byte _{{colorFormat.FirstEntry}} = {{colorFormat.FirstEntry}};
                    private readonly byte _{{colorFormat.SecondEntry}} = {{colorFormat.SecondEntry}};
                    private readonly byte _{{colorFormat.ThirdEntry}} = {{colorFormat.ThirdEntry}};
                    private readonly byte _{{colorFormat.FourthEntry}} = {{colorFormat.FourthEntry}};
                
                    /// <inheritdoc />
                    public byte R => _r;
                    
                    /// <inheritdoc />
                    public byte G => _g;
                
                    /// <inheritdoc />
                    public byte B => _b;
                    
                    /// <inheritdoc />
                    public byte A => _a;
                
                    #endregion
                
                    #region Operators
                    
                    public static bool operator ==({{colorFormat.TypeName}} left, {{colorFormat.TypeName}} right) => left.Equals(right);
                    public static bool operator !=({{colorFormat.TypeName}} left, {{colorFormat.TypeName}} right) => !left.Equals(right);
                    
                    #endregion
                
                    #region Methods
                
                    /// <inheritdoc />
                    public bool Equals(IColor? other) => (other != null) && (R == other.R) && (G == other.G) && (B == other.B) && (A == other.A);
                
                    /// <inheritdoc />
                    public bool Equals({{colorFormat.TypeName}} other) => (_{{colorFormat.FirstEntry}} == other._{{colorFormat.FirstEntry}}) && (_{{colorFormat.SecondEntry}} == other._{{colorFormat.SecondEntry}}) && (_{{colorFormat.ThirdEntry}} == other._{{colorFormat.ThirdEntry}})&& (_{{colorFormat.FourthEntry}}== other._{{colorFormat.FourthEntry}});
                    
                    /// <inheritdoc />
                    public override bool Equals(object? obj) => obj is {{colorFormat.TypeName}} other && Equals(other);
                    
                    /// <inheritdoc />
                    public override int GetHashCode() => HashCode.Combine(_{{colorFormat.FirstEntry}}, _{{colorFormat.SecondEntry}}, _{{colorFormat.ThirdEntry}}, _{{colorFormat.FourthEntry}});
                
                    /// <inheritdoc />
                    public override string ToString() => $"[A: {A}, R: {R}, G: {G}, B: {B}]";
                
                    /// <inheritdoc />
                    public static IColor Create(byte r, byte g, byte b, byte a) => new {{colorFormat.TypeName}}({{colorFormat.FirstEntry}}, {{colorFormat.SecondEntry}}, {{colorFormat.ThirdEntry}}, {{colorFormat.FourthEntry}});
                
                    #endregion
                }
                
                """,
         _ => null
     };

    private static string GenerateColorFormatCode(ColorFormatData colorFormat)
    {
        return $$"""
                 #nullable enable
                 
                 namespace HPPH;

                 public sealed partial class ColorFormat{{colorFormat.Format}} : IColorFormat
                 {
                     #region Properties & Fields
                 
                     public static ColorFormat{{colorFormat.Format}} Instance { get; } = new();
                 
                     public int BytesPerPixel => {{colorFormat.Bpp}};
                     
                     public string Name => "{{colorFormat.Format}}";
                 
                     ReadOnlySpan<byte> IColorFormat.ByteMapping => [{{colorFormat.ByteMapping}}];
                 
                     #endregion
                 
                     #region Constructors
                 
                     private ColorFormat{{colorFormat.Format}}() {}
                 
                     #endregion
                     
                     #region Methods
                     
                     public IColor CreateColor(byte r, byte g, byte b, byte a) => Color{{colorFormat.Format}}.Create(r, g, b, a);
                     
                     #endregion
                 }
                 """;
    }

    private static string GenerateColorFormats(ImmutableArray<ColorFormatData> colorFormats)
    {
        StringBuilder sb = new();
        sb.AppendLine("""
                      #nullable enable
                      
                      namespace HPPH;

                      public partial interface IColorFormat
                      {
                          #region Instances
                      """);

        sb.AppendLine();

        foreach (ColorFormatData colorFormat in colorFormats)
            sb.AppendLine($"    public static ColorFormat{colorFormat.Format} {colorFormat.Format} => ColorFormat{colorFormat.Format}.Instance;");

        sb.AppendLine();

        sb.AppendLine("""
                          IColor CreateColor(byte r, byte g, byte b, byte a);
                      
                          #endregion
                      }
                      """);

        return sb.ToString();
    }
}