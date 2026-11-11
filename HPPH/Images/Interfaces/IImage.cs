namespace HPPH;

/// <summary>
/// Represents an image.
/// </summary>
public interface IImage : IEnumerable<IColor>, IEquatable<IImage>
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
    /// Gets the stride (bytes per row) of the underlying buffer.
    /// For sub-images this is the stride of the buffer they are a view of, not <c>Width * BytesPerPixel</c>.
    /// </summary>
    int Stride { get; }

    /// <summary>
    /// Gets the color at the specified location.
    /// </summary>
    /// <param name="x">The X-location to read.</param>
    /// <param name="y">The Y-location to read.</param>
    /// <returns>The color at the specified location.</returns>
    IColor this[int x, int y] { get; }

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
    RefImage<TColor> AsRefImage<TColor>() where TColor : unmanaged, IColor<TColor>;

    /// <summary>
    /// Gets an image representing the specified region of this image.
    /// </summary>
    /// <param name="x">The X-location of the region.</param>
    /// <param name="y">The Y-location of the region.</param>
    /// <param name="width">The width of the region.</param>
    /// <param name="height">The height of the region.</param>
    /// <returns>An image sharing the data of this image.</returns>
    IImage Slice(int x, int y, int width, int height);

    /// <summary>
    /// Executes <paramref name="operation"/> on this image.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <typeparam name="TOperation">The type of the operation.</typeparam>
    /// <typeparam name="TResult">The type of the result of the operation.</typeparam>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is <see langword="null"/>.</exception>
    TResult Execute<TOperation, TResult>(TOperation operation)
        where TOperation : IImageOperation<TResult>, allows ref struct;

    /// <summary>
    /// Executes <paramref name="operation"/> on this image.
    /// </summary>
    /// <remarks>
    /// Struct operations are boxed. Use <see cref="Execute{TOperation, TResult}(TOperation)"/> to avoid that.
    /// </remarks>
    /// <param name="operation">The operation to execute.</param>
    /// <typeparam name="TResult">The type of the result of the operation.</typeparam>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is <see langword="null"/>.</exception>
    TResult Execute<TResult>(IImageOperation<TResult> operation);

    /// <summary>
    /// Converts this image into a new image of the color format <typeparamref name="TColor"/>.
    /// </summary>
    IImage<TColor> ConvertTo<TColor>() where TColor : unmanaged, IColor<TColor>;

    /// <summary>
    /// Converts the contents of this <see cref="IImage"/> into a destination <see cref="Span{T}"/> instance with color format <typeparamref name="TColor"/>.
    /// </summary>
    /// <param name="destination">The buffer to write to. Needs at least <c>Width * Height</c> elements.</param>
    /// <typeparam name="TColor">The color format to convert to.</typeparam>
    void ConvertTo<TColor>(Span<TColor> destination) where TColor : unmanaged, IColor<TColor>;

    /// <summary>
    /// Converts the contents of this <see cref="IImage"/> into a destination <see cref="Span{T}"/> instance with color format <typeparamref name="TColor"/>.
    /// Padds each row to <paramref name="stride"/>-length
    /// </summary>
    /// <param name="destination">The buffer to write to. The bytes between rows are left as they are.</param>
    /// <param name="stride">The distance between the starts of two rows in <paramref name="destination"/>, in bytes. The last row is not padding.</param>
    /// <typeparam name="TColor">The color format to convert to.</typeparam>
    void ConvertTo<TColor>(Span<byte> destination, int stride) where TColor : unmanaged, IColor<TColor>;

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
public interface IImage<T> : IImage, IEquatable<IImage<T>>
    where T : unmanaged, IColor<T>
{
    /// <summary>
    /// Gets the color at the specified location.
    /// </summary>
    /// <param name="x">The X-location to read.</param>
    /// <param name="y">The Y-location to read.</param>
    /// <returns>The color at the specified location.</returns>
    new ref readonly T this[int x, int y] { get; }

    /// <summary>
    /// Gets a <see cref="RefImage{T}"/> representing the specified region of this image.
    /// </summary>
    /// <param name="x">The X-location of the region.</param>
    /// <param name="y">The Y-location of the region.</param>
    /// <param name="width">The width of the region.</param>
    /// <param name="height">The height of the region.</param>
    /// <returns>A view on the data of this image.</returns>
    RefImage<T> this[int x, int y, int width, int height] { get; }

    /// <inheritdoc cref="IImage.Slice"/>
    new IImage<T> Slice(int x, int y, int width, int height);

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