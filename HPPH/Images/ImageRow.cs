using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

/// <summary>
/// Represents a single row of an image.
/// </summary>
/// <typeparam name="T">The color type of the pixels.</typeparam>
[SkipLocalsInit]
public readonly ref struct ImageRow<T>
    where T : unmanaged, IColor<T>
{
    #region Properties & Fields

    private readonly ReadOnlySpan<byte> _buffer;
    private readonly int _start;
    private readonly int _length;

    /// <summary>
    /// Gets the number of pixels in this row.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets the size in bytes of this row.
    /// </summary>
    public int SizeInBytes => Length * T.ColorFormat.BytesPerPixel;

    #endregion

    #region Indexer

    /// <summary>
    /// Gets the color at the specified location.
    /// </summary>
    /// <param name="x">The X-location to read.</param>
    /// <returns>The color at the specified location.</returns>
    public ref readonly T this[int x]
    {
        get
        {
            if ((x < 0) || (x >= _length)) ImageValidation.ThrowRowLocationOutOfRange(x, _length);

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

    /// <summary>
    /// Gets the pixels of this row as a <see cref="ReadOnlySpan{T}"/> without copying them.
    /// </summary>
    /// <returns>A span over the pixels of this row.</returns>
    public ReadOnlySpan<T> AsSpan() => MemoryMarshal.Cast<byte, T>(AsByteSpan());

    /// <summary>
    /// Gets the raw bytes of this row without copying them.
    /// </summary>
    /// <returns>A span over the bytes of this row.</returns>
    public ReadOnlySpan<byte> AsByteSpan() => _buffer.Slice(_start, SizeInBytes);

    /// <summary>
    /// Copies the pixels of this row into a destination buffer.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than <see cref="Length"/>.
    /// </exception>
    public void CopyTo(Span<T> destination) => CopyTo(MemoryMarshal.AsBytes(destination));

    /// <summary>
    /// Copies the pixels of this row into a destination buffer.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is shorter than <see cref="SizeInBytes"/>.
    /// </exception>
    public void CopyTo(Span<byte> destination)
    {
        if (destination.Length < SizeInBytes) throw new ArgumentException("The destination is too small to fit this image.", nameof(destination));

        AsByteSpan().CopyTo(destination);
    }

    /// <summary>
    /// Allocates a new array and copies the pixels of this row into it.
    /// </summary>
    /// <returns>The new array containing the pixels of this row.</returns>
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
