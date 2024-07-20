namespace HPPH;

/// <summary>
/// Represents a single row of an image.
/// </summary>
public interface IImageRow : IEnumerable<IColor>
{
    /// <summary>
    /// Gets the length of the row.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Gets the size in bytes of this row.
    /// </summary>
    int SizeInBytes { get; }

    /// <summary>
    /// Gets the <see cref="IColor"/> at the specified location.
    /// </summary>
    /// <param name="x">The location to get the color from.</param>
    /// <returns>The <see cref="IColor"/> at the specified location.</returns>
    IColor this[int x] { get; }

    void CopyTo(Span<IColor> destination);

    /// <summary>
    /// Copies the contents of this <see cref="IImageRow"/> into a destination <see cref="Span{T}"/> instance.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> instance.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than the source <see cref="IImageRow"/> instance.
    /// </exception>
    void CopyTo(Span<byte> destination);

    /// <summary>
    /// Allocates a new array and copies this <see cref="IImageRow"/> into it.
    /// </summary>
    /// <returns>The new array containing the data of this <see cref="IImageRow"/>.</returns>
    IColor[] ToArray();
}