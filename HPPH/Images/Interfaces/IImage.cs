namespace HPPH;

/// <summary>
/// Represents an image.
/// </summary>
public interface IImage : IEnumerable<IColor>
{
    /// <summary>
    /// Gets the color format used in this image.
    /// </summary>
    IColorFormat ColorFormat { get; }

    /// <summary>
    /// Gets the width of this image.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of this image.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets the size in bytes of this image.
    /// </summary>
    int SizeInBytes { get; }

    /// <summary>
    /// Gets the color at the specified location.
    /// </summary>
    /// <param name="x">The X-location to read.</param>
    /// <param name="y">The Y-location to read.</param>
    /// <returns>The color at the specified location.</returns>
    IColor this[int x, int y] { get; }

    /// <summary>
    /// Gets an image representing the specified location.
    /// </summary>
    /// <param name="x">The X-location of the image.</param>
    /// <param name="y">The Y-location of the image.</param>
    /// <param name="width">The width of the sub-image.</param>
    /// <param name="height"></param>
    /// <returns></returns>
    IImage this[int x, int y, int width, int height] { get; }

    /// <summary>
    /// Gets a list of all rows of this image.
    /// </summary>
    IImageRows Rows { get; }

    /// <summary>
    /// Gets a list of all columns of this image.
    /// </summary>
    IImageColumns Columns { get; }

    /// <summary>
    /// Gets an <see cref="RefImage{TColor}"/> representing this <see cref="IImage"/>.
    /// </summary>
    /// <typeparam name="TColor">The color-type of the iamge.</typeparam>
    /// <returns>The <inheritdoc cref="RefImage{TColor}"/>.</returns>
    RefImage<TColor> AsRefImage<TColor>() where TColor : struct, IColor;

    IImage<TColor> ConvertTo<TColor>() where TColor : struct, IColor;
    
    /// <summary>
    /// Copies the contents of this <see cref="IImage"/> into a destination <see cref="Span{T}"/> instance.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> instance.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than the source <see cref="IImage"/> instance.
    /// </exception>
    void CopyTo(Span<byte> destination);

    IColor[] ToArray();

    /// <summary>
    /// Allocates a new array and copies this <see cref="IImage"/> into it.
    /// </summary>
    /// <returns>The new array containing the data of this <see cref="IImage"/>.</returns>
    byte[] ToRawArray();
}

/// <summary>
/// Represents an image.
/// </summary>
public interface IImage<T> : IImage
    where T : struct, IColor
{
    /// <summary>
    /// Gets the color at the specified location.
    /// </summary>
    /// <param name="x">The X-location to read.</param>
    /// <param name="y">The Y-location to read.</param>
    /// <returns>The color at the specified location.</returns>
    new ref readonly T this[int x, int y] { get; }

    /// <summary>
    /// Gets an image representing the specified location.
    /// </summary>
    /// <param name="x">The X-location of the image.</param>
    /// <param name="y">The Y-location of the image.</param>
    /// <param name="width">The width of the sub-image.</param>
    /// <param name="height"></param>
    /// <returns></returns>
    new RefImage<T> this[int x, int y, int width, int height] { get; }

    /// <summary>
    /// Gets a list of all rows of this image.
    /// </summary>
    new ImageRows<T> Rows { get; }

    /// <summary>
    /// Gets a list of all columns of this image.
    /// </summary>
    new ImageColumns<T> Columns { get; }

    /// <summary>
    /// Gets an <see cref="RefImage{TColor}"/> representing this <see cref="IImage"/>.
    /// </summary>
    /// <returns>The <inheritdoc cref="RefImage{TColor}"/>.</returns>
    RefImage<T> AsRefImage();

    /// <summary>
    /// Copies the contents of this <see cref="IImage"/> into a destination <see cref="Span{T}"/> instance.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> instance.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than the source <see cref="IImage"/> instance.
    /// </exception>
    void CopyTo(Span<T> destination);

    new T[] ToArray();

    new IEnumerator<T> GetEnumerator();
}