using HPPH;

namespace HPPHExample;

public enum SortChannel
{
    Red,
    Green,
    Blue,
    Alpha
}

internal static class Operations
{
    #region Constants

    private const byte SENTINEL = 0xA5;

    #endregion

    #region Properties & Fields

    public static IReadOnlyList<IColorFormat> Formats { get; } = [IColorFormat.RGB, IColorFormat.BGR, IColorFormat.ARGB, IColorFormat.ABGR, IColorFormat.RGBA, IColorFormat.BGRA];

    #endregion

    #region Methods

    public static IImage ConvertTo(IImage image, IColorFormat format)
        => format switch
        {
            ColorFormatRGB => image.ConvertTo<ColorRGB>(),
            ColorFormatBGR => image.ConvertTo<ColorBGR>(),
            ColorFormatARGB => image.ConvertTo<ColorARGB>(),
            ColorFormatABGR => image.ConvertTo<ColorABGR>(),
            ColorFormatRGBA => image.ConvertTo<ColorRGBA>(),
            ColorFormatBGRA => image.ConvertTo<ColorBGRA>(),
            _ => throw new NotSupportedException($"Unknown color format '{format.Name}'.")
        };

    public static IImage AddPadding(IImage image, int padding)
        => WriteInto(image.ColorFormat, image.Width, image.Height, padding, (destination, stride) => image.CopyTo(destination, stride));

    // The buffer ends right after the last row and the padding is filled with SENTINEL, so writes outside of the pixels become visible as wrong colors.
    public static IImage WriteInto(IColorFormat format, int width, int height, int padding, Action<byte[], int> write)
    {
        int rowSize = width * format.BytesPerPixel;
        int stride = rowSize + padding;

        byte[] buffer = new byte[height == 0 ? 0 : ((height - 1) * stride) + rowSize];
        buffer.AsSpan().Fill(SENTINEL);

        write(buffer, stride);

        return format.WrapImage(buffer, width, height, stride);
    }

    #endregion
}

internal readonly struct SortOperation(SortChannel channel) : IImageOperation<IImage>
{
    public IImage Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T>
    {
        T[] pixels = image.ToArray();
        Span<T> span = pixels;

        switch (channel)
        {
            case SortChannel.Red:
                span.SortByRed();
                break;

            case SortChannel.Green:
                span.SortByGreen();
                break;

            case SortChannel.Blue:
                span.SortByBlue();
                break;

            case SortChannel.Alpha:
                span.SortByAlpha();
                break;
        }

        return Image<T>.Create(pixels, image.Width, image.Height);
    }
}

internal readonly struct RemapOperation(IColor[] palette) : IImageOperation<IImage>
{
    public IImage Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T>
    {
        T[] pixels = image.ToArray();
        Dictionary<T, T> cache = [];

        for (int i = 0; i < pixels.Length; i++)
        {
            T pixel = pixels[i];
            if (!cache.TryGetValue(pixel, out T mapped))
            {
                mapped = FindNearest(pixel);
                cache[pixel] = mapped;
            }

            pixels[i] = mapped;
        }

        return Image<T>.Create(pixels, image.Width, image.Height);
    }

    private T FindNearest<T>(T color) where T : unmanaged, IColor<T>
    {
        IColor? nearest = null;
        int nearestDistance = int.MaxValue;

        foreach (IColor candidate in palette)
        {
            int r = candidate.R - color.R;
            int g = candidate.G - color.G;
            int b = candidate.B - color.B;
            int a = candidate.A - color.A;

            int distance = (r * r) + (g * g) + (b * b) + (a * a);
            if (distance < nearestDistance)
            {
                nearest = candidate;
                nearestDistance = distance;
            }
        }

        return nearest == null ? color : T.Create(nearest.R, nearest.G, nearest.B, nearest.A);
    }
}
