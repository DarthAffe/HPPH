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
}
