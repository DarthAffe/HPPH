namespace HPPH;

/// <summary>
/// Represents a color format.
/// </summary>
// ReSharper disable once InconsistentNaming
public partial interface IColorFormat
{
    /// <summary>
    /// Gets the Bytes per pixel for this color-format.
    /// </summary>
    int BytesPerPixel { get; }

    string Name { get; }
}