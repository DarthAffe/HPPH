using System.Runtime.CompilerServices;

namespace HPPH;

/// <summary>
/// Represents the list of all rows of an image.
/// </summary>
/// <typeparam name="T">The color type of the pixels.</typeparam>
[SkipLocalsInit]
public readonly ref struct ImageRows<T>
    where T : unmanaged, IColor<T>
{
    #region Properties & Fields

    private readonly ReadOnlySpan<byte> _data;
    private readonly int _x;
    private readonly int _y;
    private readonly int _width;
    private readonly int _height;
    private readonly int _stride;
    private readonly int _bpp;

    /// <summary>
    /// Gets the number of rows.
    /// </summary>
    public int Count => _height;

    #endregion

    #region Indexer

    /// <summary>
    /// Gets the row at the specified index.
    /// </summary>
    /// <param name="row">The index of the row.</param>
    /// <returns>The row at the specified index.</returns>
    public ImageRow<T> this[int row]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((row < 0) || (row >= _height)) throw new ArgumentOutOfRangeException(nameof(row));

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

    /// <summary>
    /// Enumerates the rows of an <see cref="ImageRows{T}"/>.
    /// </summary>
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
