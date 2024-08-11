using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

[SkipLocalsInit]
public readonly ref struct RefImage<T>
    where T : struct, IColor
{
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
    /// Gets the stride (entries per row) of the underlying buffer.
    /// Only useful if you want to work with a pinned buffer.
    /// </summary>
    public int RawStride { get; }

    #endregion

    #region Indexer

#pragma warning disable CA2208 // Not ideal, but splitting up all the checks introduces quite some overhead :(

    public ref readonly T this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((x < 0) || (y < 0) || (x >= Width) || (y >= Height)) throw new ArgumentOutOfRangeException();

            return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_data), (nint)(uint)((_y + y) * RawStride))), (nint)(uint)(_x + x));
        }
    }

    public RefImage<T> this[int x, int y, int width, int height]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((x < 0) || (y < 0) || (width <= 0) || (height <= 0) || ((x + width) > Width) || ((y + height) > Height)) throw new ArgumentOutOfRangeException();

            return new RefImage<T>(_data, _x + x, _y + y, width, height, RawStride);
        }
    }

#pragma warning restore CA2208

    public ImageRows<T> Rows
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_data, _x, _y, Width, Height, RawStride);
    }

    public ImageColumns<T> Columns
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_data, _x, _y, Width, Height, RawStride);
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
        this.RawStride = stride;
    }

    #endregion

    #region Methods

    public static RefImage<T> Wrap(ReadOnlySpan<T> buffer, int width, int height)
        => Wrap(MemoryMarshal.AsBytes(buffer), width, height, width * T.ColorFormat.BytesPerPixel);

    public static RefImage<T> Wrap(ReadOnlySpan<byte> buffer, int width, int height, int stride)
    {
        if (stride < width) throw new ArgumentException("Stride can't be smaller than width.");
        if (buffer.Length < (height * stride)) throw new ArgumentException("Not enough data in the buffer.");

        return new RefImage<T>(buffer, 0, 0, width, height, stride);
    }

    /// <summary>
    /// Copies the contents of this <see cref="RefImage{T}"/> into a destination <see cref="Span{T}"/> instance.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> instance.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than the source <see cref="RefImage{T}"/> instance.
    /// </exception>
    public void CopyTo(Span<T> destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        if (destination.Length < (Width * Height)) throw new ArgumentException("The destination is too small to fit this image.", nameof(destination));

        ImageRows<T> rows = Rows;
        Span<T> target = destination;
        foreach (ImageRow<T> row in rows)
        {
            row.CopyTo(target);
            target = target[Width..];
        }
    }

    /// <summary>
    /// Allocates a new array and copies this <see cref="RefImage{T}"/> into it.
    /// </summary>
    /// <returns>The new array containing the data of this <see cref="RefImage{T}"/>.</returns>
    public T[] ToArray()
    {
        T[] array = new T[Width * Height];
        CopyTo(array);
        return array;
    }

    /// <summary>
    /// Returns a reference to the first element of this image inside the full image buffer.
    /// </summary>
    public ref readonly byte GetPinnableReference()
    {
        if (_data.Length == 0)
            return ref Unsafe.NullRef<byte>();

        return ref Unsafe.Add(ref MemoryMarshal.GetReference(_data), (_y * RawStride) + (_x * T.ColorFormat.BytesPerPixel));
    }

    /// <inheritdoc cref="System.Collections.IEnumerable.GetEnumerator"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImageEnumerator GetEnumerator() => new(_data, _x, _y, Width, Height, RawStride);

    #endregion

    public ref struct ImageEnumerator
    {
        #region Properties & Fields

        private readonly ReadOnlySpan<byte> _data;
        private readonly int _x;
        private readonly int _y;
        private readonly int _width;
        private readonly int _stride;
        private readonly int _count;

        private int _position;

        /// <inheritdoc cref="System.Collections.Generic.IEnumerator{T}.Current"/>
        public ref readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int y = (_position / _width);
                int x = _position - (y * _width);

                return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_data), (nint)(uint)((_y + y) * _stride))), (nint)(uint)(_x + x));
            }
        }

        #endregion

        #region Constructors

        // ReSharper disable once ConvertToPrimaryConstructor - Not possible with ref types
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ImageEnumerator(ReadOnlySpan<byte> data, int x, int y, int width, int height, int stride)
        {
            this._data = data;
            this._x = x;
            this._y = y;
            this._width = width;
            this._stride = stride;

            _count = _width * height;

            _position = -1;
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="System.Collections.IEnumerator.MoveNext"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_position < _count;

        #endregion
    }
}