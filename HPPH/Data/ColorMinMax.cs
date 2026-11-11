using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HPPH;

/// <summary>
/// The per-channel minimum and maximum of a set of colors. The same type for every color format.
/// Colors without an alpha channel count as fully opaque (alpha 255-255).
/// </summary>
[DebuggerDisplay("[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]")]
public readonly record struct ColorMinMax(byte RedMin, byte RedMax, byte GreenMin, byte GreenMax, byte BlueMin, byte BlueMax, byte AlphaMin, byte AlphaMax)
{
    #region Properties & Fields

    public byte RedRange => CalculateRange(RedMin, RedMax);
    public byte GreenRange => CalculateRange(GreenMin, GreenMax);
    public byte BlueRange => CalculateRange(BlueMin, BlueMax);
    public byte AlphaRange => CalculateRange(AlphaMin, AlphaMax);

    #endregion

    #region Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte CalculateRange(byte min, byte max) => max > min ? (byte)(max - min) : (byte)0;

    /// <inheritdoc />
    public override string ToString() => $"[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]";

    #endregion
}
