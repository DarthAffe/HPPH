using System.Diagnostics;

namespace HPPH;

/// <summary>
/// The per-channel minimum and maximum of a set of colors. The same type for every color format.
/// Colors without an alpha channel count as fully opaque (alpha 255-255).
/// </summary>
[DebuggerDisplay("[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]")]
public readonly record struct ColorMinMax(byte RedMin, byte RedMax, byte GreenMin, byte GreenMax, byte BlueMin, byte BlueMax, byte AlphaMin, byte AlphaMax)
{
    #region Properties & Fields

    public byte RedRange => (byte)(RedMax - RedMin);
    public byte GreenRange => (byte)(GreenMax - GreenMin);
    public byte BlueRange => (byte)(BlueMax - BlueMin);
    public byte AlphaRange => (byte)(AlphaMax - AlphaMin);

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToString() => $"[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]";

    #endregion
}
