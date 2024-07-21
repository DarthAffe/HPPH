using System.Collections;
using System.Runtime.CompilerServices;

namespace HPPH;

[SkipLocalsInit]
public readonly ref struct ImageRows<T>
    where T : struct, IColor
{
    #region Properties & Fields

    private readonly ReadOnlySpan<byte> _data;
    private readonly int _x;
    private readonly int _y;
    private readonly int _width;
    private readonly int _height;
    private readonly int _stride;
    private readonly int _bpp;

    public int Count => _height;

    #endregion

    #region Indexer

    public ImageRow<T> this[int row]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((row < 0) || (row >= _height)) throw new IndexOutOfRangeException();

            return new ImageRow<T>(_data, ((row + _y) * _stride) + (_x * _bpp), _width);
        }
    }

    #endregion

    #region Constructors

    // ReSharper disable once ConvertToPrimaryConstructor - Not possible with ref types
    internal ImageRows(ReadOnlySpan<byte> data, int x, int y, int width, int height, int stride)
    {
        this._data = data;
        this._x = x;
        this._y = y;
        this._width = width;
        this._height = height;
        this._stride = stride;

        _bpp = T.ColorFormat.BytesPerPixel;
    }

    #endregion

    #region Methods

    /// <inheritdoc cref="System.Collections.IEnumerable.GetEnumerator"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImageRowsEnumerator GetEnumerator() => new(this);

    #endregion

    public ref struct ImageRowsEnumerator
    {
        #region Properties & Fields

        private readonly ImageRows<T> _rows;
        private int _position;

        /// <inheritdoc cref="System.Collections.Generic.IEnumerator{T}.Current"/>
        public readonly ImageRow<T> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rows[_position];
        }

        #endregion

        #region Constructors


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ImageRowsEnumerator(ImageRows<T> rows)
        {
            this._rows = rows;

            _position = -1;
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="System.Collections.IEnumerator.MoveNext"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_position < _rows._height;

        #endregion
    }
}

//HACK DarthAffe 14.07.2024: Not nice, should be removed once ref structs are able to implement interfaces (https://github.com/dotnet/csharplang/blob/main/proposals/ref-struct-interfaces.md)
[SkipLocalsInit]
internal class IColorImageRows<T> : IImageRows
    where T : struct, IColor
{
    #region Properties & Fields

    private readonly byte[] _data;
    private readonly int _x;
    private readonly int _y;
    private readonly int _width;
    private readonly int _height;
    private readonly int _stride;
    private readonly int _bpp;

    /// <inheritdoc />
    public int Count => _height;

    #endregion

    #region Indexer

    /// <inheritdoc />
    public IImageRow this[int row]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((row < 0) || (row >= _height)) throw new IndexOutOfRangeException();

            return new IColorImageRow<T>(_data, ((row + _y) * _stride) + (_x * _bpp), _width);
        }
    }

    #endregion

    #region Constructors

    // ReSharper disable once ConvertToPrimaryConstructor - Not possible with ref types
    internal IColorImageRows(byte[] data, int x, int y, int width, int height, int stride)
    {
        this._data = data;
        this._x = x;
        this._y = y;
        this._width = width;
        this._height = height;
        this._stride = stride;

        _bpp = T.ColorFormat.BytesPerPixel;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerator<IImageRow> GetEnumerator()
    {
        for (int i = 0; i < _height; i++)
            yield return this[i];
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}