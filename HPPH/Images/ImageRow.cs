using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

[SkipLocalsInit]
public readonly ref struct ImageRow<T>
    where T : struct, IColor
{
    #region Properties & Fields

    private readonly ReadOnlySpan<byte> _buffer;
    private readonly int _start;
    private readonly int _length;

    public int Length => _length;

    public int SizeInBytes => Length * T.ColorFormat.BytesPerPixel;

    #endregion

    #region Indexer

    public ref readonly T this[int x]
    {
        get
        {
            if ((x < 0) || (x >= _length)) throw new IndexOutOfRangeException();

            return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), (nint)(uint)_start)), (nint)(uint)x);
        }
    }

    #endregion

    #region Constructors

    internal ImageRow(ReadOnlySpan<byte> buffer, int start, int length)
    {
        this._buffer = buffer;
        this._start = start;
        this._length = length;
    }

    #endregion

    #region Methods

    public void CopyTo(Span<T> destination) => CopyTo(MemoryMarshal.AsBytes(destination));

    public void CopyTo(Span<byte> destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        if (destination.Length < SizeInBytes) throw new ArgumentException("The destination is too small to fit this image.", nameof(destination));

        _buffer.Slice(_start, SizeInBytes).CopyTo(destination);
    }

    public T[] ToArray()
    {
        T[] array = new T[Length];
        CopyTo(array);
        return array;
    }
    /// <inheritdoc cref="System.Collections.IEnumerable.GetEnumerator"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImageRowEnumerator GetEnumerator() => new(this);

    #endregion

    public ref struct ImageRowEnumerator
    {
        #region Properties & Fields

        private readonly ImageRow<T> _column;
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
        internal ImageRowEnumerator(ImageRow<T> column)
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

//HACK DarthAffe 14.07.2024: Not nice, should be removed once ref structs are able to implement interfaces (https://github.com/dotnet/csharplang/blob/main/proposals/ref-struct-interfaces.md)
[SkipLocalsInit]
internal class IColorImageRow<T> : IImageRow
    where T : struct, IColor
{
    #region Properties & Fields

    private readonly byte[] _buffer;
    private readonly int _start;
    private readonly int _length;

    /// <inheritdoc />
    public int Length => _length;

    /// <inheritdoc />
    public int SizeInBytes => Length * T.ColorFormat.BytesPerPixel;

    #endregion

    #region Indexer

    /// <inheritdoc />
    public IColor this[int x]
    {
        get
        {
            if ((x < 0) || (x >= _length)) throw new IndexOutOfRangeException();

            return Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer.AsSpan()), (nint)(uint)_start)), (nint)(uint)x);
        }
    }

    #endregion

    #region Constructors

    internal IColorImageRow(byte[] buffer, int start, int length)
    {
        this._buffer = buffer;
        this._start = start;
        this._length = length;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void CopyTo(Span<IColor> destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        if (destination.Length < _length) throw new ArgumentException("The destination is too small to fit this image.", nameof(destination));

        for (int i = 0; i < _length; i++)
            destination[i] = this[i];
    }

    /// <inheritdoc />
    public void CopyTo(Span<byte> destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        if (destination.Length < SizeInBytes) throw new ArgumentException("The destination is too small to fit this image.", nameof(destination));

        _buffer.AsSpan().Slice(_start, SizeInBytes).CopyTo(destination);
    }

    /// <inheritdoc />
    public IColor[] ToArray()
    {
        IColor[] array = new IColor[Length];
        CopyTo(array);
        return array;
    }

    /// <inheritdoc />
    public IEnumerator<IColor> GetEnumerator()
    {
        for (int i = 0; i < _length; i++)
            yield return this[i];
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}