using System.Collections;
using System.Runtime.CompilerServices;

namespace HPPH;

[SkipLocalsInit]
public readonly ref struct ImageColumns<T>
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

    public int Count => _width;

    #endregion

    #region Indexer

    public ImageColumn<T> this[int column]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((column < 0) || (column > _width)) throw new IndexOutOfRangeException();

            return new ImageColumn<T>(_data, (_y * _stride) + ((column + _x) * _bpp), _height, _stride);
        }
    }

    #endregion

    #region Constructors

    // ReSharper disable once ConvertToPrimaryConstructor - Not possible with ref types
    internal ImageColumns(ReadOnlySpan<byte> data, int x, int y, int width, int height, int stride)
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
    public ImageColumnsEnumerator GetEnumerator() => new(this);

    #endregion

    public ref struct ImageColumnsEnumerator
    {
        #region Properties & Fields

        private readonly ImageColumns<T> _columns;
        private int _position;

        /// <inheritdoc cref="System.Collections.Generic.IEnumerator{T}.Current"/>
        public readonly ImageColumn<T> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _columns[_position];
        }

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ImageColumnsEnumerator(ImageColumns<T> columns)
        {
            this._columns = columns;
            this._position = -1;
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="System.Collections.IEnumerator.MoveNext"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_position < _columns._width;

        #endregion
    }
}

//HACK DarthAffe 14.07.2024: Not nice, should be removed once ref structs are able to implement interfaces (https://github.com/dotnet/csharplang/blob/main/proposals/ref-struct-interfaces.md)
[SkipLocalsInit]
internal class IColorImageColumns<T> : IImageColumns
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

    public int Count => _width;

    #endregion

    #region Indexer

    public IImageColumn this[int column]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((column < 0) || (column > _width)) throw new IndexOutOfRangeException();

            return new IColorImageColumn<T>(_data, (_y * _stride) + ((column + _x) * _bpp), _height, _stride);
        }
    }

    #endregion

    #region Constructors

    // ReSharper disable once ConvertToPrimaryConstructor - Not possible with ref types
    internal IColorImageColumns(byte[] data, int x, int y, int width, int height, int stride)
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

    public IEnumerator<IImageColumn> GetEnumerator()
    {
        for (int i = 0; i < _width; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}