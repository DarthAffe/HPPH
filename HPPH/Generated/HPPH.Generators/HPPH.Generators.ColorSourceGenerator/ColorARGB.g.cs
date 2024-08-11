#nullable enable

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ReplaceWithPrimaryConstructorParameter

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

/// <summary>
/// Represents a color in 32 bit ARGB-format.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ColorARGB"/> class.
/// </remarks>
/// <param name="a">The Alpha-component of the color.</param>
/// <param name="r">The Red-component of the color.</param>
/// <param name="g">The Green-component of the color.</param>
/// <param name="b">The Blue-component of the color.</param>
[DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
[SkipLocalsInit]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct ColorARGB(byte a, byte r, byte g, byte b) : IColor, IEquatable<ColorARGB>
{
    #region Properties & Fields

    /// <inheritdoc />
    public static IColorFormat ColorFormat => IColorFormat.ARGB;

    private readonly byte _a = a;
    private readonly byte _r = r;
    private readonly byte _g = g;
    private readonly byte _b = b;

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
    
    public static bool operator ==(ColorARGB left, ColorARGB right) => left.Equals(right);
    public static bool operator !=(ColorARGB left, ColorARGB right) => !left.Equals(right);
    
    #endregion

    #region Methods

    /// <inheritdoc />
    public bool Equals(IColor? other) => (other != null) && (R == other.R) && (G == other.G) && (B == other.B) && (A == other.A);

    /// <inheritdoc />
    public bool Equals(ColorARGB other) => (_a == other._a) && (_r == other._r) && (_g == other._g)&& (_b== other._b);
    
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ColorARGB other && Equals(other);
    
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_a, _r, _g, _b);

    /// <inheritdoc />
    public override string ToString() => $"[A: {A}, R: {R}, G: {G}, B: {B}]";

    /// <inheritdoc />
    public static IColor Create(byte r, byte g, byte b, byte a) => new ColorARGB(a, r, g, b);

    #endregion
}
