﻿#nullable enable

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ReplaceWithPrimaryConstructorParameter

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

/// <summary>
/// Represents a color in 24 bit RGB-format.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ColorRGB"/> class.
/// </remarks>
/// <param name="r">The Red-component of the color.</param>
/// <param name="g">The Green-component of the color.</param>
/// <param name="b">The Blue-component of the color.</param>
[DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
[SkipLocalsInit]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct ColorRGB(byte r, byte g, byte b): IColor, IEquatable<ColorRGB>
{
    #region Properties & Fields

    /// <inheritdoc />
    public static IColorFormat ColorFormat => IColorFormat.RGB;

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
    public byte A => byte.MaxValue;

    #endregion

    #region Operators
    
    public static bool operator ==(ColorRGB left, ColorRGB right) => left.Equals(right);
    public static bool operator !=(ColorRGB left, ColorRGB right) => !left.Equals(right);
    
    #endregion

    #region Methods

    /// <inheritdoc />
    public bool Equals(IColor? other) => (other != null) && (R == other.R) && (G == other.G) && (B == other.B) && (A == other.A);

    /// <inheritdoc />
    public bool Equals(ColorRGB other) => (_r == other._r) && (_g == other._g) && (_b == other._b);
    
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ColorRGB other && Equals(other);
    
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_r, _g, _b);

    /// <inheritdoc />
    public override string ToString() => $"[A: {A}, R: {R}, G: {G}, B: {B}]";

    /// <inheritdoc />
    public static IColor Create(byte r, byte g, byte b, byte a) => new ColorRGB(r, g, b);

    #endregion
}
