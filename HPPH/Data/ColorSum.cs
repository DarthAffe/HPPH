using System.Diagnostics;

namespace HPPH;

/// <summary>
/// The per-channel sums of a set of colors. The same type for every color format.
/// Colors without an alpha channel count as fully opaque (255 each).
/// </summary>
/// <param name="R">The sum of the red channel.</param>
/// <param name="G">The sum of the green channel.</param>
/// <param name="B">The sum of the blue channel.</param>
/// <param name="A">The sum of the alpha channel.</param>
[DebuggerDisplay("[A: {A}, R: {R}, G: {G}, B: {B}]")]
public readonly record struct ColorSum(long R, long G, long B, long A)
{
    #region Methods

    /// <inheritdoc />
    public override string ToString() => $"[A: {A}, R: {R}, G: {G}, B: {B}]";

    #endregion
}
