using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

[SkipLocalsInit]
public readonly ref struct ImageRow<T>
    where T : unmanaged, IColor<T>
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
            if ((x < 0) || (x >= _length)) throw new ArgumentOutOfRangeException(nameof(x));

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

    public ReadOnlySpan<T> AsSpan() => MemoryMarshal.Cast<byte, T>(AsByteSpan());
    public ReadOnlySpan<byte> AsByteSpan() => _buffer.Slice(_start, SizeInBytes);

    public void CopyTo(Span<T> destination) => CopyTo(MemoryMarshal.AsBytes(destination));

    public void CopyTo(Span<byte> destination)
    {
        if (destination.Length < SizeInBytes) throw new ArgumentException("The destination is too small to fit this image.", nameof(destination));

        AsByteSpan().CopyTo(destination);
    }

    public T[] ToArray()
    {
        T[] array = GC.AllocateUninitializedArray<T>(Length);
        CopyTo(array);
        return array;
    }

    /// <inheritdoc cref="System.Collections.IEnumerable.GetEnumerator"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

    #endregion
}
