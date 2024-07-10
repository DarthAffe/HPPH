using System.Runtime.CompilerServices;

namespace HPPH;

[SkipLocalsInit]
internal struct ColorCube<T>
    where T : unmanaged, IColor
{
    #region Properties & Fields

    private readonly int _offset;
    private readonly int _length;
    private SortTarget _sortOrder;

    #endregion

    #region Constructors

    internal ColorCube(int offset, int length, SortTarget sortOrder)
    {
        this._offset = offset;
        this._length = length;
        this._sortOrder = sortOrder;
    }

    #endregion

    #region Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly Span<T> Slice(Span<T> fullColorList) => fullColorList.Slice(_offset, _length);

    internal void Split(Span<T> fullColorList, out ColorCube<T> a, out ColorCube<T> b)
    {
        OrderColors(Slice(fullColorList), _sortOrder);

        int median = _length / 2;

        a = new ColorCube<T>(_offset, median, _sortOrder);
        b = new ColorCube<T>(_offset + median, _length - median, _sortOrder);
    }

    private void OrderColors(Span<T> colors, SortTarget preOrdered)
    {
        if (colors.Length < 2) return;
        IMinMax colorRanges = colors.MinMax();

        if ((colorRanges.RedRange > colorRanges.GreenRange) && (colorRanges.RedRange > colorRanges.BlueRange))
        {
            if (preOrdered != SortTarget.Red)
                colors.SortByRed();

            _sortOrder = SortTarget.Red;
        }
        else if (colorRanges.GreenRange > colorRanges.BlueRange)
        {
            if (preOrdered != SortTarget.Green)
                colors.SortByGreen();

            _sortOrder = SortTarget.Green;
        }
        else
        {
            if (preOrdered != SortTarget.Blue)
                colors.SortByBlue();

            _sortOrder = SortTarget.Blue;
        }
    }

    #endregion
}