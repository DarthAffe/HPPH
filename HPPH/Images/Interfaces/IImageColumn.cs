namespace HPPH;

/// <summary>
/// Represents a single column of an image.
/// </summary>
public interface IImageColumn : IEnumerable<IColor>
{
    /// <summary>
    /// Gets the length of the column.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Gets the size in bytes of this column.
    /// </summary>
    int SizeInBytes { get; }

    /// <summary>
    /// Gets the <see cref="IColor"/> at the specified location.
    /// </summary>
    /// <param name="y">The location to get the color from.</param>
    /// <returns>The <see cref="IColor"/> at the specified location.</returns>
    IColor this[int y] { get; }

    void CopyTo(Span<IColor> destination);

    /// <summary>
    /// Copies the contents of this <see cref="IImageColumn"/> into a destination <see cref="Span{T}"/> instance.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> instance.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than the source <see cref="IImageColumn"/> instance.
    /// </exception>
    void CopyTo(Span<byte> destination);

    /// <summary>
    /// Allocates a new array and copies this <see cref="IImageColumn"/> into it.
    /// </summary>
    /// <returns>The new array containing the data of this <see cref="IImageColumn"/>.</returns>
    IColor[] ToArray();
}