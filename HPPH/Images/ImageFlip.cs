namespace HPPH;

/// <summary>
/// Specifies the directions in which an image is flipped.
/// </summary>
[Flags]
public enum ImageFlip
{
    /// <summary>
    /// The image is not flipped.
    /// </summary>
    None = 0,

    /// <summary>
    /// Left and right are swapped: the pixel at <c>(x, y)</c> moves to <c>(Width - 1 - x, y)</c>.
    /// </summary>
    Horizontal = 1,

    /// <summary>
    /// Top and bottom are swapped: the pixel at <c>(x, y)</c> moves to <c>(x, Height - 1 - y)</c>.
    /// </summary>
    Vertical = 2
}
