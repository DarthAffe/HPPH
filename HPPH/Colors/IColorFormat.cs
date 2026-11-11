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

    /// <summary>
    /// Gets the name of this color-format.
    /// </summary>
    string Name { get; }

    internal ReadOnlySpan<byte> ByteMapping { get; }

    /// <summary>
    /// Creates an image of this color format from a copy of <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">The pixel data.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="stride">The distance between the starts of two rows in <paramref name="buffer"/>, in bytes. The last row needs no padding.</param>
    /// <returns>The created image.</returns>
    IImage CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride);

    /// <summary>
    /// Creates an image of this color format on top of <paramref name="buffer"/> without copying it. Changes to the buffer show up in the image.
    /// </summary>
    /// <param name="buffer">The pixel data.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="stride">The distance between the starts of two rows, in bytes. The last row needs no padding.</param>
    /// <returns>The created image.</returns>
    IImage WrapImage(byte[] buffer, int width, int height, int stride);
}