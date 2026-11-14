using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

internal static class ImageValidation
{
    /// <summary>
    /// Validates the layout of an image inside a buffer. <paramref name="stride"/> is in bytes.
    /// The last row doesn't need padding, so a buffer of <c>(height - 1) * stride + width * bytesPerPixel</c> bytes is enough.
    /// </summary>
    public static void ValidateLayout(int bufferLength, int width, int height, int stride, int bytesPerPixel)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);

        long rowSize = (long)width * bytesPerPixel;
        if (stride < rowSize) throw new ArgumentException($"Stride ({stride} bytes) can't be smaller than width * bytes per pixel ({rowSize} bytes).", nameof(stride));

        long required = height == 0 ? 0 : ((long)(height - 1) * stride) + rowSize;
        if (bufferLength < required) throw new ArgumentException($"Not enough data in the buffer. {bufferLength} < {required} bytes.");
    }

    /// <summary>
    /// Validates a destination for a result of <paramref name="width"/> x <paramref name="height"/> pixels without row padding,
    /// and that it doesn't overlap <paramref name="source"/>.
    /// </summary>
    /// <returns>The bytes of <paramref name="destination"/> the result is written to.</returns>
    public static Span<byte> ValidateDestination<T>(RefImage<T> source, Span<T> destination, int width, int height)
        where T : unmanaged, IColor<T>
    {
        int pixels = width * height;
        if (destination.Length < pixels) throw new ArgumentException($"The destination is too small to fit the result. {destination.Length} < {pixels} pixels.", nameof(destination));

        Span<byte> written = MemoryMarshal.AsBytes(destination[..pixels]);
        ValidateNoOverlap(source, written);

        return written;
    }

    /// <summary>
    /// Validates a destination for a result of <paramref name="width"/> x <paramref name="height"/> pixels whose rows are <paramref name="stride"/> bytes apart,
    /// and that it doesn't overlap <paramref name="source"/>.
    /// </summary>
    public static void ValidateDestination<T>(RefImage<T> source, Span<byte> destination, int stride, int width, int height)
        where T : unmanaged, IColor<T>
    {
        int bpp = T.ColorFormat.BytesPerPixel;
        ValidateLayout(destination.Length, width, height, stride, bpp);

        int length = height == 0 ? 0 : ((height - 1) * stride) + (width * bpp);
        ValidateNoOverlap(source, destination[..length]);
    }

    /// <summary>
    /// Validates that <paramref name="destination"/> doesn't share memory with <paramref name="source"/>, including the padding between its rows.
    /// </summary>
    public static void ValidateNoOverlap<T>(RefImage<T> source, ReadOnlySpan<byte> destination)
        where T : unmanaged, IColor<T>
    {
        if (source.GetBytes().Overlaps(destination))
            throw new ArgumentException("The destination can't overlap the memory of the image.", nameof(destination));
    }

    /// <summary>
    /// Validates that the region lies within an image of the size <paramref name="imageWidth"/> x <paramref name="imageHeight"/> and isn't empty.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ValidateRegion(int x, int y, int width, int height, int imageWidth, int imageHeight)
    {
        if ((x < 0) || (y < 0) || (width <= 0) || (height <= 0) || (x > (imageWidth - width)) || (y > (imageHeight - height)))
            ThrowRegionOutOfRange(x, y, width, height, imageWidth, imageHeight);
    }

    /// <summary>
    /// Throws the exception for a location outside of an image. Kept out of line so the bounds checks of the indexers stay small.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowLocationOutOfRange(int x, int y, int imageWidth, int imageHeight)
    {
        if ((x < 0) || (x >= imageWidth))
            throw new ArgumentOutOfRangeException(nameof(x), x, $"The X-location has to be at least 0 and less than the width of the image ({imageWidth}).");

        throw new ArgumentOutOfRangeException(nameof(y), y, $"The Y-location has to be at least 0 and less than the height of the image ({imageHeight}).");
    }

    /// <summary>
    /// Throws the exception for a location outside of a row. Kept out of line so the bounds check of the indexer stays small.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowRowLocationOutOfRange(int x, int rowLength)
        => throw new ArgumentOutOfRangeException(nameof(x), x, $"The X-location has to be at least 0 and less than the length of the row ({rowLength}).");

    /// <summary>
    /// Throws the exception for a location outside of a column. Kept out of line so the bounds check of the indexer stays small.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowColumnLocationOutOfRange(int y, int columnLength)
        => throw new ArgumentOutOfRangeException(nameof(y), y, $"The Y-location has to be at least 0 and less than the length of the column ({columnLength}).");

    /// <summary>
    /// Throws the exception for a row index outside of an image. Kept out of line so the bounds check of the indexer stays small.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowRowOutOfRange(int row, int rowCount)
        => throw new ArgumentOutOfRangeException(nameof(row), row, $"The row has to be at least 0 and less than the number of rows ({rowCount}).");

    /// <summary>
    /// Throws the exception for a column index outside of an image. Kept out of line so the bounds check of the indexer stays small.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowColumnOutOfRange(int column, int columnCount)
        => throw new ArgumentOutOfRangeException(nameof(column), column, $"The column has to be at least 0 and less than the number of columns ({columnCount}).");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowRegionOutOfRange(int x, int y, int width, int height, int imageWidth, int imageHeight)
    {
        if (x < 0) throw new ArgumentOutOfRangeException(nameof(x), x, "The X-location of the region can't be negative.");
        if (y < 0) throw new ArgumentOutOfRangeException(nameof(y), y, "The Y-location of the region can't be negative.");
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), width, "The width of the region has to be greater than 0.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), height, "The height of the region has to be greater than 0.");

        if (x > (imageWidth - width))
            throw new ArgumentOutOfRangeException(nameof(width), width, $"The region (X: {x}, width: {width}) exceeds the width of the image ({imageWidth}).");

        throw new ArgumentOutOfRangeException(nameof(height), height, $"The region (Y: {y}, height: {height}) exceeds the height of the image ({imageHeight}).");
    }
}
