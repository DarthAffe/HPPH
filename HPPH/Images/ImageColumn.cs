using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

/// <summary>
/// Represents a single column of an image.
/// </summary>
/// <typeparam name="T">The color type of the pixels.</typeparam>
[SkipLocalsInit]
public readonly ref struct ImageColumn<T>
    where T : unmanaged, IColor<T>
{
    #region Properties & Fields

    private readonly ReadOnlySpan<byte> _buffer;
    private readonly int _start;
    private readonly int _length;
    private readonly int _step;

    /// <summary>
    /// Gets the number of pixels in this column.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets the size in bytes of the pixels of this column.
    /// </summary>
    public int SizeInBytes => Length * T.ColorFormat.BytesPerPixel;

    #endregion

    #region Indexer

    /// <summary>
    /// Gets the color at the specified location.
    /// </summary>
    /// <param name="y">The Y-location to read.</param>
    /// <returns>The color at the specified location.</returns>
    public ref readonly T this[int y]
    {
        get
        {
            if ((y < 0) || (y >= _length)) throw new ArgumentOutOfRangeException(nameof(y));

            return ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), (nint)(uint)(_start + (y * _step))));
        }
    }

    #endregion

    #region Constructors

    internal ImageColumn(ReadOnlySpan<byte> buffer, int start, int length, int step)
    {
        this._buffer = buffer;
        this._start = start;
        this._length = length;
        this._step = step;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Copies the pixels of this column into a destination buffer.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than <see cref="Length"/>.
    /// </exception>
    public void CopyTo(Span<T> destination) => CopyTo(MemoryMarshal.AsBytes(destination));

    /// <summary>
    /// Copies the pixels of this column into a destination buffer.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than <see cref="SizeInBytes"/>.
    /// </exception>
    public void CopyTo(Span<byte> destination)
    {
        if (destination.Length < SizeInBytes) throw new ArgumentException("The destination is too small to fit this image.", nameof(destination));

        ref byte dataRef = ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), _start);
        Span<T> target = MemoryMarshal.Cast<byte, T>(destination);
        for (int i = 0; i < Length; i++)
            target[i] = Unsafe.As<byte, T>(ref Unsafe.Add(ref dataRef, i * _step));
    }

    /// <summary>
    /// Allocates a new array and copies the pixels of this column into it.
    /// </summary>
    /// <returns>The new array containing the pixels of this column.</returns>
    public T[] ToArray()
    {
        T[] array = GC.AllocateUninitializedArray<T>(Length);
        CopyTo(array);
        return array;
    }

    /// <inheritdoc cref="System.Collections.IEnumerable.GetEnumerator"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImageColumnEnumerator GetEnumerator() => new(this);

    #endregion

    /// <summary>
    /// Enumerates the pixels of an <see cref="ImageColumn{T}"/>.
    /// </summary>
    public ref struct ImageColumnEnumerator
    {
        #region Properties & Fields

        private readonly ImageColumn<T> _column;
        private int _position;

        /// <inheritdoc cref="System.Collections.Generic.IEnumerator{T}.Current"/>
        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _column[_position];
        }

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ImageColumnEnumerator(ImageColumn<T> column)
        {
            this._column = column;
            this._position = -1;
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="System.Collections.IEnumerator.MoveNext"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_position < _column.Length;

        #endregion
    }
}
