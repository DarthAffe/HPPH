﻿#nullable enable

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ReplaceWithPrimaryConstructorParameter

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HPPH;

[DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct SumBGR(long b, long g, long r, long a) : ISum
{
    #region Properties & Fields

    private readonly long _b = b;
    private readonly long _g = g;
    private readonly long _r = r;
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
