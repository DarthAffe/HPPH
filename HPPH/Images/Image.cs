using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

/// <summary>
/// Represents an image with the color type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The color type of the pixels.</typeparam>
[SkipLocalsInit]
public sealed class Image<T> : IImage, IEnumerable<T>
    where T : unmanaged, IColor<T>
{
    #region Properties & Fields

    private readonly byte[] _buffer;

    private readonly int _x;
    private readonly int _y;
    private readonly int _stride;

    /// <inheritdoc />
    public IColorFormat ColorFormat
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => T.ColorFormat;
    }

    /// <inheritdoc />
    public int Width { get; }

    /// <inheritdoc />
    public int Height { get; }

    /// <inheritdoc />
    public int SizeInBytes => Width * Height * ColorFormat.BytesPerPixel;

    /// <inheritdoc />
    public int Stride => _stride;

    #endregion

    #region Indexer

    /// <inheritdoc />
    IColor IImage.this[int x, int y] => this[x, y];

    /// <summary>
    /// Gets the color at the specified location.
    /// </summary>
    /// <param name="x">The X-location to read.</param>
    /// <param name="y">The Y-location to read.</param>
    /// <returns>The color at the specified location.</returns>
    public ref readonly T this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((x < 0) || (y < 0) || (x >= Width) || (y >= Height)) ImageValidation.ThrowLocationOutOfRange(x, y, Width, Height);

            return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer.AsSpan()), (nint)(uint)((_y + y) * _stride))), (nint)(uint)(_x + x));
        }
    }

    /// <summary>
    /// Gets a <see cref="RefImage{T}"/> representing the specified region of this image.
    /// </summary>
    /// <remarks>
    /// Use <see cref="RefImage{T}.ToImage"/> on the result to get an independent copy of the region.
    /// </remarks>
    /// <param name="x">The X-location of the region.</param>
    /// <param name="y">The Y-location of the region.</param>
    /// <param name="width">The width of the region.</param>
    /// <param name="height">The height of the region.</param>
    /// <returns>A view on the data of this image.</returns>
    public RefImage<T> this[int x, int y, int width, int height]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ImageValidation.ValidateRegion(x, y, width, height, Width, Height);

            return new RefImage<T>(_buffer, _x + x, _y + y, width, height, _stride);
        }
    }

    /// <summary>
    /// Gets a list of all rows of this image.
    /// </summary>
    public ImageRows<T> Rows
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_buffer, _x, _y, Width, Height, _stride);
    }

    /// <summary>
    /// Gets a list of all columns of this image.
    /// </summary>
    public ImageColumns<T> Columns
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_buffer, _x, _y, Width, Height, _stride);
    }

    #endregion

    #region Constructors

    private Image(byte[] buffer, int x, int y, int width, int height, int stride)
    {
        this._buffer = buffer;
        this._x = x;
        this._y = y;
        this.Width = width;
        this.Height = height;
        this._stride = stride;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates an image from a copy of <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">The pixel data.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    public static Image<T> Create(ReadOnlySpan<T> buffer, int width, int height)
        => Create(MemoryMarshal.AsBytes(buffer), width, height, width * T.ColorFormat.BytesPerPixel);

    /// <summary>
    /// Creates an image from a copy of <paramref name="buffer"/>.
    /// </summary>
    /// <remarks>
    /// Only the pixels are copied. Row padding and data after the last row are dropped.
    /// After creation its <see cref="Stride"/> is <c>width * BytesPerPixel</c>.
    /// </remarks>
    /// <param name="buffer">The pixel data.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="stride">The distance between the starts of two rows in <paramref name="buffer"/>, in bytes. The last row needs no padding.</param>
    public static Image<T> Create(ReadOnlySpan<byte> buffer, int width, int height, int stride)
        => RefImage<T>.Wrap(buffer, width, height, stride).ToImage();

    /// <summary>
    /// Creates an image on top of <paramref name="buffer"/> without copying it. Changes to the buffer show up in the image.
    /// </summary>
    /// <param name="buffer">The pixel data.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="stride">The distance between the starts of two rows, in bytes. The last row needs no padding.</param>
    public static Image<T> Wrap(byte[] buffer, int width, int height, int stride)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ImageValidation.ValidateLayout(buffer.Length, width, height, stride, T.ColorFormat.BytesPerPixel);

        return new Image<T>(buffer, 0, 0, width, height, stride);
    }

    /// <inheritdoc cref="IImage.Slice" />
    public Image<T> Slice(int x, int y, int width, int height)
    {
        ImageValidation.ValidateRegion(x, y, width, height, Width, Height);

        return new Image<T>(_buffer, _x + x, _y + y, width, height, _stride);
    }

    /// <inheritdoc />
    IImage IImage.Slice(int x, int y, int width, int height) => Slice(x, y, width, height);

    /// <inheritdoc />
    public Image<TColor> ConvertTo<TColor>()
        where TColor : unmanaged, IColor<TColor>
        => AsRefImage().ConvertTo<TColor>();

    /// <inheritdoc />
    public void ConvertTo<TColor>(Span<TColor> destination)
        where TColor : unmanaged, IColor<TColor>
        => AsRefImage().ConvertTo(destination);

    /// <inheritdoc />
    public void ConvertTo<TColor>(Span<byte> destination, int stride)
        where TColor : unmanaged, IColor<TColor>
        => AsRefImage().ConvertTo<TColor>(destination, stride);

    /// <inheritdoc cref="RefImage{T}.CopyTo(Span{T})" />
    public void CopyTo(Span<T> destination) => AsRefImage().CopyTo(destination);

    /// <inheritdoc />
    public void CopyTo(Span<byte> destination) => AsRefImage().CopyTo(destination);

    /// <inheritdoc />
    public void CopyTo(Span<byte> destination, int stride) => AsRefImage().CopyTo(destination, stride);

    /// <inheritdoc />
    public byte[] ToRawArray()
    {
        byte[] array = GC.AllocateUninitializedArray<byte>(SizeInBytes);
        CopyTo(array);
        return array;
    }

    /// <summary>
    /// Allocates a new array and copies the pixels of this image into it, row after row.
    /// </summary>
    /// <returns>The new array containing the pixels of this image.</returns>
    public T[] ToArray()
    {
        T[] colors = GC.AllocateUninitializedArray<T>(Width * Height);
        CopyTo(colors);
        return colors;
    }

    /// <inheritdoc cref="IImage.ToImage" />
    public Image<T> ToImage() => AsRefImage().ToImage();

    /// <inheritdoc />
    IImage IImage.ToImage() => ToImage();

    /// <summary>
    /// Gets a <see cref="RefImage{T}"/> representing this image.
    /// </summary>
    /// <returns>A view on the data of this image.</returns>
    public RefImage<T> AsRefImage() => new(_buffer, _x, _y, Width, Height, _stride);

    /// <inheritdoc />
    public RefImage<TColor> AsRefImage<TColor>()
        where TColor : unmanaged, IColor<TColor>
    {
        if (typeof(TColor) != typeof(T)) throw new ArgumentException("The requested color format does not fit this image.", nameof(TColor));

        return new RefImage<TColor>(_buffer, _x, _y, Width, Height, _stride);
    }

    /// <inheritdoc />
    public TResult Execute<TOperation, TResult>(TOperation operation)
        where TOperation : IImageOperation<TResult>, allows ref struct
    {
        if (operation is null) throw new ArgumentNullException(nameof(operation));

        return operation.Execute(AsRefImage());
    }

    /// <inheritdoc />
    public TResult Execute<TResult>(IImageOperation<TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return operation.Execute(AsRefImage());
    }

    /// <summary>
    /// Returns a reference to the first element of this image inside the full image buffer.
    /// </summary>
    public ref readonly byte GetPinnableReference()
    {
        if (_buffer.Length == 0)
            return ref Unsafe.NullRef<byte>();

        return ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer.AsSpan()), (_y * _stride) + (_x * ColorFormat.BytesPerPixel));
    }

    /// <inheritdoc cref="System.Collections.IEnumerable.GetEnumerator"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefImage<T>.ImageEnumerator GetEnumerator() => AsRefImage().GetEnumerator();

    /// <inheritdoc />
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        int bpp = T.ColorFormat.BytesPerPixel;
        for (int y = 0; y < Height; y++)
        {
            int rowOffset = ((_y + y) * _stride) + (_x * bpp);
            for (int x = 0; x < Width; x++)
                yield return Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), (nint)(uint)rowOffset)), (nint)(uint)x);
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
    
    /// <inheritdoc />
    public bool ContentEquals(IImage? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.ColorFormat != ColorFormat) return false;

        return ContentEquals(other.AsRefImage<T>());
    }

    /// <inheritdoc cref="ContentEquals(IImage)" />
    public bool ContentEquals(Image<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return ContentEquals(other.AsRefImage());
    }

    /// <inheritdoc cref="RefImage{T}.ContentEquals(RefImage{T})" />
    public bool ContentEquals(RefImage<T> other) => AsRefImage().ContentEquals(other);

    #endregion
}