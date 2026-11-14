using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HPPH;

/// <summary>
/// The per-channel minimum and maximum of a set of colors. The same type for every color format.
/// Colors without an alpha channel count as fully opaque (alpha 255-255).
/// </summary>
/// <param name="RedMin">The minimum of the red channel.</param>
/// <param name="RedMax">The maximum of the red channel.</param>
/// <param name="GreenMin">The minimum of the green channel.</param>
/// <param name="GreenMax">The maximum of the green channel.</param>
/// <param name="BlueMin">The minimum of the blue channel.</param>
/// <param name="BlueMax">The maximum of the blue channel.</param>
/// <param name="AlphaMin">The minimum of the alpha channel.</param>
/// <param name="AlphaMax">The maximum of the alpha channel.</param>
[DebuggerDisplay("[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]")]
public readonly record struct ColorMinMax(byte RedMin, byte RedMax, byte GreenMin, byte GreenMax, byte BlueMin, byte BlueMax, byte AlphaMin, byte AlphaMax)
{
    #region Properties & Fields

    /// <summary>
    /// Gets the difference between the maximum and minimum of the red channel.
    /// </summary>
    public byte RedRange => CalculateRange(RedMin, RedMax);

    /// <summary>
    /// Gets the difference between the maximum and minimum of the green channel.
    /// </summary>
    public byte GreenRange => CalculateRange(GreenMin, GreenMax);

    /// <summary>
    /// Gets the difference between the maximum and minimum of the blue channel.
    /// </summary>
    public byte BlueRange => CalculateRange(BlueMin, BlueMax);

    /// <summary>
    /// Gets the difference between the maximum and minimum of the alpha channel.
    /// </summary>
    public byte AlphaRange => CalculateRange(AlphaMin, AlphaMax);

    #endregion

    #region Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte CalculateRange(byte min, byte max) => max > min ? (byte)(max - min) : (byte)0;

    /// <inheritdoc />
    public override string ToString() => $"[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]";

    #endregion
}
