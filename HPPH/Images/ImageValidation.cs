using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
