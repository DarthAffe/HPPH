#nullable enable

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ReplaceWithPrimaryConstructorParameter

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

/// <summary>
/// Represents a color in 32 bit ABGR-format.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ColorABGR"/> class.
/// </remarks>
/// <param name="a">The Alpha-component of the color.</param>
/// <param name="b">The Blue-component of the color.</param>
/// <param name="g">The Green-component of the color.</param>
/// <param name="r">The Red-component of the color.</param>
[DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
[SkipLocalsInit]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct ColorABGR(byte a, byte b, byte g, byte r) : IColor, IEquatable<ColorABGR>
{
    #region Properties & Fields

    /// <inheritdoc />
    public static IColorFormat ColorFormat => IColorFormat.ABGR;

    private readonly byte _a = a;
    private readonly byte _b = b;
    private readonly byte _g = g;
    private readonly byte _r = r;

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
    
    public static bool operator ==(ColorABGR left, ColorABGR right) => left.Equals(right);
    public static bool operator !=(ColorABGR left, ColorABGR right) => !left.Equals(right);
    
    #endregion

    #region Methods

    /// <inheritdoc />
    public bool Equals(IColor? other) => (other != null) && (R == other.R) && (G == other.G) && (B == other.B) && (A == other.A);

    /// <inheritdoc />
    public bool Equals(ColorABGR other) => (_a == other._a) && (_b == other._b) && (_g == other._g)&& (_r== other._r);
    
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ColorABGR other && Equals(other);
    
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_a, _b, _g, _r);

    /// <inheritdoc />
    public override string ToString() => $"[A: {A}, R: {R}, G: {G}, B: {B}]";

    /// <inheritdoc />
    public static IColor Create(byte r, byte g, byte b, byte a) => new ColorABGR(a, b, g, r);

    #endregion
}
