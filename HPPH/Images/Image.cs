using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

/// <inheritdoc cref="IImage{T}" />
[SkipLocalsInit]
public sealed class Image<T> : IImage<T>, IEquatable<Image<T>>
    where T : struct, IColor
{
    #region Properties & Fields

    private readonly byte[] _buffer;

    private readonly int _x;
    private readonly int _y;
    private readonly int _stride;

    /// <inheritdoc />
    public IColorFormat ColorFormat
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => T.ColorFormat;
    }

    /// <inheritdoc />
    public int Width { get; }

    /// <inheritdoc />
    public int Height { get; }

    /// <inheritdoc />
    public int SizeInBytes => Width * Height * ColorFormat.BytesPerPixel;

    #endregion

    #region Indexer

    /// <inheritdoc />
    IColor IImage.this[int x, int y] => this[x, y];

#pragma warning disable CA2208 // Not ideal, but splitting up all the checks introduces quite some overhead :(

    /// <inheritdoc />
    public ref readonly T this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((x < 0) || (y < 0) || (x >= Width) || (y >= Height)) throw new ArgumentOutOfRangeException();

            return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer.AsSpan()), (nint)(uint)((_y + y) * _stride))), (nint)(uint)(_x + x));
        }
    }

    /// <inheritdoc />
    IImage IImage.this[int x, int y, int width, int height]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((x < 0) || (y < 0) || (width <= 0) || (height <= 0) || ((x + width) > Width) || ((y + height) > Height)) throw new ArgumentOutOfRangeException();

            return new Image<T>(_buffer, _x + x, _y + y, width, height, _stride);
        }
    }

    /// <inheritdoc />
    public RefImage<T> this[int x, int y, int width, int height]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((x < 0) || (y < 0) || (width <= 0) || (height <= 0) || ((x + width) > Width) || ((y + height) > Height)) throw new ArgumentOutOfRangeException();

            return new RefImage<T>(_buffer, _x + x, _y + y, width, height, _stride);
        }
    }

#pragma warning restore CA2208

    /// <inheritdoc />
    IImageRows IImage.Rows => new IColorImageRows<T>(_buffer, _x, _y, Width, Height, _stride);

    /// <inheritdoc />
    public ImageRows<T> Rows
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_buffer, _x, _y, Width, Height, _stride);
    }

    IImageColumns IImage.Columns => new IColorImageColumns<T>(_buffer, _x, _y, Width, Height, _stride);
    /// <inheritdoc />
    public ImageColumns<T> Columns
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_buffer, _x, _y, Width, Height, _stride);
    }

    #endregion

    #region Constructors

    private Image(byte[] buffer, int x, int y, int width, int height, int stride)
    {
        this._buffer = buffer;
        this._x = x;
        this._y = y;
        this.Width = width;
        this.Height = height;
        this._stride = stride;
    }

    #endregion

    #region Methods

    public static Image<T> Create(ReadOnlySpan<T> buffer, int width, int height)
        => Create(MemoryMarshal.AsBytes(buffer), width, height, width * T.ColorFormat.BytesPerPixel);

    public static Image<T> Create(ReadOnlySpan<byte> buffer, int width, int height, int stride)
    {
        if (stride < width) throw new ArgumentException("Stride can't be smaller than width.");
        if (buffer.Length < (height * stride)) throw new ArgumentException("Not enough data in the buffer.");

        byte[] data = new byte[buffer.Length];
        buffer.CopyTo(data);
        return new Image<T>(data, 0, 0, width, height, stride);
    }

    public static Image<T> Wrap(byte[] buffer, int width, int height, int stride)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
        if (stride < width) throw new ArgumentException("Stride can't be smaller than width.");
        if (buffer.Length < (height * stride)) throw new ArgumentException("Not enough data in the buffer.");

        return new Image<T>(buffer, 0, 0, width, height, stride);
    }

    /// <inheritdoc />
    public IImage<TColor> ConvertTo<TColor>()
        where TColor : struct, IColor
    {
        int targetBpp = TColor.ColorFormat.BytesPerPixel;
        if (targetBpp == ColorFormat.BytesPerPixel)
        {
            byte[] data = ToRawArray();
            MemoryMarshal.Cast<byte, T>(data.AsSpan()).ConvertInPlace<T, TColor>();
            return new Image<TColor>(data, 0, 0, Width, Height, Width * targetBpp);
        }
        else
        {
            byte[] data = ToRawArray();
            byte[] target = new byte[Width * Height * targetBpp];
            MemoryMarshal.Cast<byte, T>(data.AsSpan()).Convert(MemoryMarshal.Cast<byte, TColor>(target));
            return new Image<TColor>(target, 0, 0, Width, Height, Width * targetBpp);
        }
    }

    /// <inheritdoc />
    public void CopyTo(Span<T> destination) => CopyTo(MemoryMarshal.AsBytes(destination));

    /// <inheritdoc />
    public void CopyTo(Span<byte> destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        if (destination.Length < SizeInBytes) throw new ArgumentException("The destination is too small to fit this image.", nameof(destination));

        int targetStride = Width * ColorFormat.BytesPerPixel;
        ImageRows<T> rows = Rows;
        Span<byte> target = destination;
        foreach (ImageRow<T> row in rows)
        {
            row.CopyTo(target);
            target = target[targetStride..];
        }
    }

    /// <inheritdoc />
    public byte[] ToRawArray()
    {
        byte[] array = new byte[SizeInBytes];
        CopyTo(array);
        return array;
    }

    /// <inheritdoc />
    public T[] ToArray()
    {
        T[] colors = new T[Width * Height];
        CopyTo(colors);
        return colors;
    }

    //TODO DarthAffe 11.07.2024: This has some potential for optimization
    /// <inheritdoc />
    IColor[] IImage.ToArray()
    {
        IColor[] colors = new IColor[Width * Height];

        int counter = 0;
        foreach (ImageRow<T> row in Rows)
            foreach (T color in row)
                colors[counter++] = color;

        return colors;
    }

    /// <inheritdoc />
    public RefImage<T> AsRefImage() => new(_buffer, _x, _y, Width, Height, _stride);

    /// <inheritdoc />
    public RefImage<TColor> AsRefImage<TColor>()
        where TColor : struct, IColor
    {
        if (typeof(TColor) != typeof(T)) throw new ArgumentException("The requested color format does not fit this image.", nameof(TColor));

        return new RefImage<TColor>(_buffer, _x, _y, Width, Height, _stride);
    }

    /// <summary>
    /// Returns a reference to the first element of this image inside the full image buffer.
    /// </summary>
    public ref readonly byte GetPinnableReference()
    {
        if (_buffer.Length == 0)
            return ref Unsafe.NullRef<byte>();

        return ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer.AsSpan()), (_y * _stride) + (_x * ColorFormat.BytesPerPixel));
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                yield return this[x, y];
    }

    /// <inheritdoc />
    IEnumerator<IColor> IEnumerable<IColor>.GetEnumerator()
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                yield return this[x, y];
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    //TODO DarthAffe 20.07.2024: All of those equals can be optimized
    /// <inheritdoc />
    public bool Equals(IImage? other)
    {
        if (other == null) return false;
        if (other.ColorFormat != ColorFormat) return false;
        if (other.Width != Width) return false;
        if (other.Height != Height) return false;

        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (!this[x, y].Equals(other[x, y]))
                    return false;

        return true;
    }

    /// <inheritdoc />
    public bool Equals(IImage<T>? other)
    {
        if (other == null) return false;
        if (other.Width != Width) return false;
        if (other.Height != Height) return false;

        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (!this[x, y].Equals(other[x, y]))
                    return false;

        return true;
    }

    /// <inheritdoc />
    public bool Equals(Image<T>? other)
    {
        if (other == null) return false;
        if (other.Width != Width) return false;
        if (other.Height != Height) return false;

        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (!this[x, y].Equals(other[x, y]))
                    return false;

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Image<T>);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(ColorFormat, Width, Height, _buffer.GetHashCode());

    #endregion
}