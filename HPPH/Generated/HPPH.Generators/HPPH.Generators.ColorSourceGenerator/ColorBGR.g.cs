﻿#nullable enable

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ReplaceWithPrimaryConstructorParameter

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

/// <summary>
/// Represents a color in 24 bit BGR-format.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ColorBGR"/> class.
/// </remarks>
/// <param name="b">The Blue-component of the color.</param>
/// <param name="g">The Green-component of the color.</param>
/// <param name="r">The Red-component of the color.</param>
[DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
[SkipLocalsInit]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct ColorBGR(byte b, byte g, byte r): IColor, IEquatable<ColorBGR>
{
    #region Properties & Fields

    /// <inheritdoc />
    public static IColorFormat ColorFormat => IColorFormat.BGR;

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
    public byte A => byte.MaxValue;

    #endregion

    #region Operators
    
    public static bool operator ==(ColorBGR left, ColorBGR right) => left.Equals(right);
    public static bool operator !=(ColorBGR left, ColorBGR right) => !left.Equals(right);
    
    #endregion

    #region Methods

    /// <inheritdoc />
    public bool Equals(IColor? other) => (other != null) && (R == other.R) && (G == other.G) && (B == other.B) && (A == other.A);

    /// <inheritdoc />
    public bool Equals(ColorBGR other) => (_b == other._b) && (_g == other._g) && (_r == other._r);
    
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ColorBGR other && Equals(other);
    
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_b, _g, _r);

    /// <inheritdoc />
    public override string ToString() => $"[A: {A}, R: {R}, G: {G}, B: {B}]";

    /// <inheritdoc />
    public static IColor Create(byte r, byte g, byte b, byte a) => new ColorBGR(b, g, r);

    #endregion
}
