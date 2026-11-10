#nullable enable

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ReplaceWithPrimaryConstructorParameter

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

/// <summary>
/// Represents a color in 32 bit RGBA-format.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ColorRGBA"/> class.
/// </remarks>
/// <param name="r">The Red-component of the color.</param>
/// <param name="g">The Green-component of the color.</param>
/// <param name="b">The Blue-component of the color.</param>
/// <param name="a">The Alpha-component of the color.</param>
[DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
[SkipLocalsInit]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct ColorRGBA(byte r, byte g, byte b, byte a) : IColor, IEquatable<ColorRGBA>
{
    #region Properties & Fields

    /// <inheritdoc />
    public static IColorFormat ColorFormat => IColorFormat.RGBA;

    private readonly byte _r = r;
    private readonly byte _g = g;
    private readonly byte _b = b;
    private readonly byte _a = a;

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
    
    public static bool operator ==(ColorRGBA left, ColorRGBA right) => left.Equals(right);
    public static bool operator !=(ColorRGBA left, ColorRGBA right) => !left.Equals(right);
    
    #endregion

    #region Methods

    /// <inheritdoc />
    public bool Equals(IColor? other) => (other != null) && (R == other.R) && (G == other.G) && (B == other.B) && (A == other.A);

    /// <inheritdoc />
    public bool Equals(ColorRGBA other) => Unsafe.BitCast<ColorRGBA, uint>(this) == Unsafe.BitCast<ColorRGBA, uint>(other);
    
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj switch
    {
        ColorRGBA other => Equals(other),
        IColor other => Equals(other),
        _ => false
    };
    
    /// <inheritdoc />
    public override int GetHashCode() => R | (G << 8) | (B << 16) | (A << 24);

    /// <inheritdoc />
    public override string ToString() => $"[A: {A}, R: {R}, G: {G}, B: {B}]";

    /// <inheritdoc />
    public static IColor Create(byte r, byte g, byte b, byte a) => new ColorRGBA(r, g, b, a);

    #endregion
}
