// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ReplaceWithPrimaryConstructorParameter

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HPPH;

[DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct SumABGR(long a, long b, long g, long r) : ISum
{
    #region Properties & Fields

    private readonly long _a = a;
    private readonly long _b = b;
    private readonly long _g = g;
    private readonly long _r = r;

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
