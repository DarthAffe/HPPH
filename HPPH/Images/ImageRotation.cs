namespace HPPH;

/// <summary>
/// Specifies by how many degrees an image is rotated clockwise.
/// </summary>
public enum ImageRotation
{
    /// <summary>
    /// No rotatetion.
    /// </summary>
    None = 0,

    /// <summary>
    /// Rotated by 90 degrees clockwise: the top row becomes the right column. Width and height are swapped.
    /// </summary>
    Rotate90 = 90,

    /// <summary>
    /// Rotated by 180 degrees. This gives the same result as flipping it horizontally and vertically.
    /// </summary>
    Rotate180 = 180,

    /// <summary>
    /// Rotated by 270 degrees clockwise: the top row becomes the left column. Width and height are swapped.
    /// </summary>
    Rotate270 = 270
}
