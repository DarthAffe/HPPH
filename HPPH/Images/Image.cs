using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

/// <inheritdoc cref="IImage{T}" />
[SkipLocalsInit]
public sealed class Image<T> : IImage<T>, IEquatable<Image<T>>
    where T : unmanaged, IColor<T>
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

    /// <inheritdoc />
    public int Stride => _stride;

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

    /// <summary>
    /// Creates an image from a copy of <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">The pixel data.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    public static Image<T> Create(ReadOnlySpan<T> buffer, int width, int height)
        => Create(MemoryMarshal.AsBytes(buffer), width, height, width * T.ColorFormat.BytesPerPixel);

    /// <summary>
    /// Creates an image from a copy of <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">The pixel data.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="stride">The distance between the starts of two rows, in bytes. The last row needs no padding.</param>
    public static Image<T> Create(ReadOnlySpan<byte> buffer, int width, int height, int stride)
    {
        ImageValidation.ValidateLayout(buffer.Length, width, height, stride, T.ColorFormat.BytesPerPixel);

        byte[] data = GC.AllocateUninitializedArray<byte>(buffer.Length);
        buffer.CopyTo(data);
        return new Image<T>(data, 0, 0, width, height, stride);
    }

    /// <summary>
    /// Creates an image on top of <paramref name="buffer"/> without copying it. Changes to the buffer show up in the image.
    /// </summary>
    /// <param name="buffer">The pixel data.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="stride">The distance between the starts of two rows, in bytes. The last row needs no padding.</param>
    public static Image<T> Wrap(byte[] buffer, int width, int height, int stride)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ImageValidation.ValidateLayout(buffer.Length, width, height, stride, T.ColorFormat.BytesPerPixel);

        return new Image<T>(buffer, 0, 0, width, height, stride);
    }

    /// <inheritdoc cref="IImage{T}.Slice" />
    public Image<T> Slice(int x, int y, int width, int height)
    {
#pragma warning disable CA2208 // Not ideal, but splitting up all the checks introduces quite some overhead :(

        if ((x < 0) || (y < 0) || (width <= 0) || (height <= 0) || ((x + width) > Width) || ((y + height) > Height)) throw new ArgumentOutOfRangeException();

#pragma warning restore CA2208

        return new Image<T>(_buffer, _x + x, _y + y, width, height, _stride);
    }

    /// <inheritdoc />
    IImage<T> IImage<T>.Slice(int x, int y, int width, int height) => Slice(x, y, width, height);

    /// <inheritdoc />
    IImage IImage.Slice(int x, int y, int width, int height) => Slice(x, y, width, height);

    /// <inheritdoc />
    public IImage<TColor> ConvertTo<TColor>()
        where TColor : unmanaged, IColor<TColor>
        => AsRefImage().ConvertTo<TColor>();

    /// <inheritdoc />
    public void ConvertTo<TColor>(Span<TColor> destination)
        where TColor : unmanaged, IColor<TColor>
        => AsRefImage().ConvertTo(destination);

    /// <inheritdoc />
    public void ConvertTo<TColor>(Span<byte> destination, int stride)
        where TColor : unmanaged, IColor<TColor>
        => AsRefImage().ConvertTo<TColor>(destination, stride);

    /// <inheritdoc />
    public void CopyTo(Span<T> destination) => AsRefImage().CopyTo(destination);

    /// <inheritdoc />
    public void CopyTo(Span<byte> destination) => AsRefImage().CopyTo(destination);

    /// <inheritdoc />
    public byte[] ToRawArray()
    {
        byte[] array = GC.AllocateUninitializedArray<byte>(SizeInBytes);
        CopyTo(array);
        return array;
    }

    /// <inheritdoc />
    public T[] ToArray()
    {
        T[] colors = GC.AllocateUninitializedArray<T>(Width * Height);
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
        where TColor : unmanaged, IColor<TColor>
    {
        if (typeof(TColor) != typeof(T)) throw new ArgumentException("The requested color format does not fit this image.", nameof(TColor));

        return new RefImage<TColor>(_buffer, _x, _y, Width, Height, _stride);
    }

    /// <inheritdoc />
    public TResult Execute<TOperation, TResult>(TOperation operation)
        where TOperation : IImageOperation<TResult>, allows ref struct
    {
        if (operation is null) throw new ArgumentNullException(nameof(operation));

        return operation.Execute(AsRefImage());
    }

    /// <inheritdoc />
    public TResult Execute<TResult>(IImageOperation<TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return operation.Execute(AsRefImage());
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
        int bpp = T.ColorFormat.BytesPerPixel;
        for (int y = 0; y < Height; y++)
        {
            int rowOffset = ((_y + y) * _stride) + (_x * bpp);
            for (int x = 0; x < Width; x++)
                yield return Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), (nint)(uint)rowOffset)), (nint)(uint)x);
        }
    }

    /// <inheritdoc />
    IEnumerator<IColor> IEnumerable<IColor>.GetEnumerator()
    {
        int bpp = T.ColorFormat.BytesPerPixel;
        for (int y = 0; y < Height; y++)
        {
            int rowOffset = ((_y + y) * _stride) + (_x * bpp);
            for (int x = 0; x < Width; x++)
                yield return Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), (nint)(uint)rowOffset)), (nint)(uint)x);
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public bool Equals(IImage? other)
    {
        if (other == null) return false;
        if (other.ColorFormat != ColorFormat) return false;

        return Equals(other.AsRefImage<T>());
    }

    /// <inheritdoc />
    public bool Equals(IImage<T>? other)
    {
        if (other == null) return false;

        return Equals(other.AsRefImage());
    }

    /// <inheritdoc />
    public bool Equals(Image<T>? other)
    {
        if (other == null) return false;

        return Equals(other.AsRefImage());
    }

    public bool Equals(RefImage<T> other)
    {
        if (other.Width != Width) return false;
        if (other.Height != Height) return false;

        RefImage<T> thisRef = AsRefImage();

        for (int y = 0; y < Height; y++)
            if (!thisRef.Rows[y].AsByteSpan().SequenceEqual(other.Rows[y].AsByteSpan()))
                return false;

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Image<T>);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(ColorFormat, Width, Height);

    #endregion
}