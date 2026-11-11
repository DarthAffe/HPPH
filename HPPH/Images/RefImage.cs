using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

[SkipLocalsInit]
public readonly ref struct RefImage<T>
    where T : unmanaged, IColor<T>
{
    #region Constants

    private const int COPY_CHUNK_SIZE = 64 * 1024;

    #endregion

    #region Properties & Fields

    private readonly ReadOnlySpan<byte> _data;

    private readonly int _x;
    private readonly int _y;

    /// <summary>
    /// Gets the width of the image.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the image.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the stride (bytes per row) of the underlying buffer.
    /// Only useful if you want to work with a pinned buffer.
    /// </summary>
    public int Stride { get; }

    /// <summary>
    /// Gets the size in bytes of the pixels of this image without row padding.
    /// </summary>
    public int SizeInBytes => Width * Height * T.ColorFormat.BytesPerPixel;

    /// <summary>
    /// Gets a value indicating whether the rows of this image follow each other without padding.
    /// </summary>
    internal bool IsContiguous => Stride == (Width * T.ColorFormat.BytesPerPixel);

    #endregion

    #region Indexer

#pragma warning disable CA2208 // Not ideal, but splitting up all the checks introduces quite some overhead :(

    public ref readonly T this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((x < 0) || (y < 0) || (x >= Width) || (y >= Height)) throw new ArgumentOutOfRangeException();

            return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_data), (nint)(uint)((_y + y) * Stride))), (nint)(uint)(_x + x));
        }
    }

    /// <summary>
    /// Gets a <see cref="RefImage{T}"/> representing the specified region of this image. Same as <see cref="Slice"/>.
    /// </summary>
    /// <param name="x">The X-location of the region.</param>
    /// <param name="y">The Y-location of the region.</param>
    /// <param name="width">The width of the region.</param>
    /// <param name="height">The height of the region.</param>
    /// <returns>A view on the data of this image.</returns>
    public RefImage<T> this[int x, int y, int width, int height]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Slice(x, y, width, height);
    }

#pragma warning restore CA2208

    public ImageRows<T> Rows
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_data, _x, _y, Width, Height, Stride);
    }

    public ImageColumns<T> Columns
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_data, _x, _y, Width, Height, Stride);
    }

    #endregion

    #region Constructors

    internal RefImage(ReadOnlySpan<byte> data, int x, int y, int width, int height, int stride)
    {
        this._data = data;
        this._x = x;
        this._y = y;
        this.Width = width;
        this.Height = height;
        this.Stride = stride;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Wraps a buffer without copying it.
    /// </summary>
    /// <param name="buffer">The pixel data.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    public static RefImage<T> Wrap(ReadOnlySpan<T> buffer, int width, int height)
        => Wrap(MemoryMarshal.AsBytes(buffer), width, height, width * T.ColorFormat.BytesPerPixel);

    /// <summary>
    /// Wraps a buffer without copying it.
    /// </summary>
    /// <param name="buffer">The pixel data.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="stride">The distance between the starts of two rows, in bytes. The last row needs no padding.</param>
    public static RefImage<T> Wrap(ReadOnlySpan<byte> buffer, int width, int height, int stride)
    {
        ImageValidation.ValidateLayout(buffer.Length, width, height, stride, T.ColorFormat.BytesPerPixel);

        return new RefImage<T>(buffer, 0, 0, width, height, stride);
    }

    /// <summary>
    /// Gets a <see cref="RefImage{T}"/> representing the specified region of this image.
    /// </summary>
    /// <param name="x">The X-location of the region.</param>
    /// <param name="y">The Y-location of the region.</param>
    /// <param name="width">The width of the region.</param>
    /// <param name="height">The height of the region.</param>
    /// <returns>A view on the data of this image.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefImage<T> Slice(int x, int y, int width, int height)
    {
#pragma warning disable CA2208 // Not ideal, but splitting up all the checks introduces quite some overhead :(

        if ((x < 0) || (y < 0) || (width <= 0) || (height <= 0) || ((x + width) > Width) || ((y + height) > Height)) throw new ArgumentOutOfRangeException();

#pragma warning restore CA2208

        return new RefImage<T>(_data, _x + x, _y + y, width, height, Stride);
    }

    /// <summary>
    /// Copies the contents of this <see cref="RefImage{T}"/> into a destination <see cref="Span{T}"/> instance.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> instance.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than the source <see cref="RefImage{T}"/> instance.
    /// </exception>
    public void CopyTo(Span<T> destination) => CopyTo(MemoryMarshal.AsBytes(destination));

    /// <summary>
    /// Copies the pixels of this <see cref="RefImage{T}"/> into a destination buffer, row after row without padding.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than <see cref="SizeInBytes"/>.
    /// </exception>
    public void CopyTo(Span<byte> destination)
    {
        if (destination.Length < SizeInBytes)
            throw new ArgumentException("The destination is too small to fit this image.", nameof(destination));

        if (IsContiguous)
        {
            ReadOnlySpan<byte> source = GetBytes();
            for (int offset = 0; offset < source.Length; offset += COPY_CHUNK_SIZE)
            {
                int length = Math.Min(COPY_CHUNK_SIZE, source.Length - offset);
                source.Slice(offset, length).CopyTo(destination.Slice(offset, length));
            }
        }
        else
        {
            int rowSize = Width * T.ColorFormat.BytesPerPixel;
            for (int y = 0; y < Height; y++)
                GetRowBytes(y).CopyTo(destination.Slice(y * rowSize, rowSize));
        }
    }

    /// <summary>
    /// Allocates a new array and copies this <see cref="RefImage{T}"/> into it.
    /// </summary>
    /// <returns>The new array containing the data of this <see cref="RefImage{T}"/>.</returns>
    public T[] ToArray()
    {
        T[] array = GC.AllocateUninitializedArray<T>(Width * Height);
        CopyTo(array);
        return array;
    }

    /// <summary>
    /// Allocates a new array and copies the pixels of this <see cref="RefImage{T}"/> into it, row after row without padding.
    /// </summary>
    /// <returns>The new array containing the data of this <see cref="RefImage{T}"/>.</returns>
    public byte[] ToRawArray()
    {
        byte[] array = GC.AllocateUninitializedArray<byte>(SizeInBytes);
        CopyTo(array);
        return array;
    }

    /// <summary>
    /// Converts this image into a new image of the color format <typeparamref name="TColor"/>.
    /// </summary>
    /// <typeparam name="TColor">The color format to convert to.</typeparam>
    public Image<TColor> ConvertTo<TColor>()
        where TColor : unmanaged, IColor<TColor>
    {
        int targetBpp = TColor.ColorFormat.BytesPerPixel;
        byte[] target = GC.AllocateUninitializedArray<byte>(Width * Height * targetBpp);

        ConvertTo(MemoryMarshal.Cast<byte, TColor>(target.AsSpan()));

        return Image<TColor>.Wrap(target, Width, Height, Width * targetBpp);
    }

    /// <summary>
    /// Converts the contents of this <see cref="IImage"/> into a destination <see cref="Span{T}"/> instance with color format <typeparamref name="TColor"/>.
    /// </summary>
    /// <param name="destination">The buffer to write to. Needs at least <c>Width * Height</c> elements.</param>
    /// <typeparam name="TColor">The color format to convert to.</typeparam>
    public void ConvertTo<TColor>(Span<TColor> destination)
        where TColor : unmanaged, IColor<TColor>
    {
        if (destination.Length < (Width * Height))
            throw new ArgumentException("The destination is too small to fit this image.", nameof(destination));

        if (IsContiguous)
            MemoryMarshal.Cast<byte, T>(GetBytes()).Convert(destination);
        else
            for (int y = 0; y < Height; y++)
                MemoryMarshal.Cast<byte, T>(GetRowBytes(y)).Convert(destination.Slice(y * Width, Width));
    }

    /// <summary>
    /// Converts the contents of this <see cref="IImage"/> into a destination <see cref="Span{T}"/> instance with color format <typeparamref name="TColor"/>.
    /// Padds each row to <paramref name="stride"/>-length
    /// </summary>
    /// <param name="destination">The buffer to write to. The bytes between rows are left as they are.</param>
    /// <param name="stride">The distance between the starts of two rows in <paramref name="destination"/>, in bytes. The last row is not padding.</param>
    /// <typeparam name="TColor">The color format to convert to.</typeparam>
    public void ConvertTo<TColor>(Span<byte> destination, int stride)
        where TColor : unmanaged, IColor<TColor>
    {
        int targetBpp = TColor.ColorFormat.BytesPerPixel;
        ImageValidation.ValidateLayout(destination.Length, Width, Height, stride, targetBpp);

        int targetRowSize = Width * targetBpp;
        if (IsContiguous && (stride == targetRowSize))
            MemoryMarshal.Cast<byte, T>(GetBytes()).Convert(MemoryMarshal.Cast<byte, TColor>(destination[..(Height * targetRowSize)]));
        else
            for (int y = 0; y < Height; y++)
                MemoryMarshal.Cast<byte, T>(GetRowBytes(y)).Convert(MemoryMarshal.Cast<byte, TColor>(destination.Slice(y * stride, targetRowSize)));
    }

    /// <inheritdoc cref="IImage.Execute{TOperation, TResult}(TOperation)"/>
    public TResult Execute<TOperation, TResult>(TOperation operation)
        where TOperation : IImageOperation<TResult>, allows ref struct
    {
        if (operation is null) throw new ArgumentNullException(nameof(operation));

        return operation.Execute(this);
    }

    /// <inheritdoc cref="IImage.Execute{TResult}(IImageOperation{TResult})"/>
    public TResult Execute<TResult>(IImageOperation<TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return operation.Execute(this);
    }

    /// <summary>
    /// Returns a reference to the first element of this image inside the full image buffer.
    /// </summary>
    public ref readonly byte GetPinnableReference()
    {
        if (_data.Length == 0)
            return ref Unsafe.NullRef<byte>();

        return ref Unsafe.Add(ref MemoryMarshal.GetReference(_data), (_y * Stride) + (_x * T.ColorFormat.BytesPerPixel));
    }

    /// <inheritdoc cref="System.Collections.IEnumerable.GetEnumerator"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImageEnumerator GetEnumerator() => new(_data, _x, _y, Width, Height, Stride);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<byte> GetBytes()
    {
        if (Height == 0) return [];

        return _data.Slice((_y * Stride) + (_x * T.ColorFormat.BytesPerPixel), SizeInBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<byte> GetRowBytes(int y)
    {
        int bpp = T.ColorFormat.BytesPerPixel;
        return _data.Slice(((_y + y) * Stride) + (_x * bpp), Width * bpp);
    }
    
    #endregion

    public ref struct ImageEnumerator
    {
        #region Properties & Fields

        private ref byte _row;
        private readonly int _width;
        private readonly int _stride;
        private int _x;
        private int _rowsLeft;

        /// <inheritdoc cref="System.Collections.Generic.IEnumerator{T}.Current"/>
        public readonly ref readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.As<byte, T>(ref _row), (nint)(uint)_x);
        }

        #endregion

        #region Constructors

        // ReSharper disable once ConvertToPrimaryConstructor - Not possible with ref types
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ImageEnumerator(ReadOnlySpan<byte> data, int x, int y, int width, int height, int stride)
        {
            this._width = width;
            this._stride = stride;
            this._x = -1;

            if ((width > 0) && (height > 0))
            {
                _row = ref Unsafe.Add(ref MemoryMarshal.GetReference(data), (nint)(uint)((y * stride) + (x * T.ColorFormat.BytesPerPixel)));
                _rowsLeft = height;
            }
            else
            {
                _row = ref Unsafe.NullRef<byte>();
                _rowsLeft = 0;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="System.Collections.IEnumerator.MoveNext"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (++_x < _width) return _rowsLeft > 0;

            _x = 0;
            if (--_rowsLeft <= 0)
            {
                _rowsLeft = 0;
                return false;
            }

            _row = ref Unsafe.Add(ref _row, (nint)(uint)_stride);

            return true;
        }

        #endregion
    }
}