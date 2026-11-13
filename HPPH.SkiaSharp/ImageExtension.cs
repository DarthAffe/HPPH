// ReSharper disable InconsistentNaming

using System.Buffers;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace HPPH.SkiaSharp;

/// <summary>
/// Contains extension methods to convert between HPPH images and SkiaSharp images.
/// </summary>
public static class ImageExtension
{
    /// <summary>
    /// Copies the pixels of <paramref name="image"/> into a new <see cref="SKImage"/>.
    /// </summary>
    /// <param name="image">The image to convert.</param>
    /// <returns>The new <see cref="SKImage"/>.</returns>
    public static SKImage ToSKImage(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        using SKBitmap bitmap = image.ToSKBitmap();
        bitmap.SetImmutable();

        return SKImage.FromBitmap(bitmap);
    }

    /// <inheritdoc cref="ToSKImage(IImage)"/>
    public static SKImage ToSKImage<T>(this Image<T> image)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().ToSKImage();
    }

    /// <inheritdoc cref="ToSKImage(IImage)"/>
    public static SKImage ToSKImage<T>(this RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        using SKBitmap bitmap = image.ToSKBitmap();
        bitmap.SetImmutable();

        return SKImage.FromBitmap(bitmap);
    }

    /// <summary>
    /// Copies the pixels of <paramref name="image"/> into a new <see cref="SKBitmap"/>.
    /// </summary>
    /// <remarks>
    /// The bitmap is <see cref="SKColorType.Rgba8888"/> for <see cref="ColorRGBA"/>-images and <see cref="SKColorType.Bgra8888"/> otherwise.
    /// </remarks>
    /// <param name="image">The image to convert.</param>
    /// <returns>The new <see cref="SKBitmap"/>.</returns>
    public static SKBitmap ToSKBitmap(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<ToSKBitmapOperation, SKBitmap>(default);
    }

    /// <inheritdoc cref="ToSKBitmap(IImage)"/>
    public static SKBitmap ToSKBitmap<T>(this Image<T> image)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().ToSKBitmap();
    }

    /// <inheritdoc cref="ToSKBitmap(IImage)"/>
    public static unsafe SKBitmap ToSKBitmap<T>(this RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        bool rgba = typeof(T) == typeof(ColorRGBA);

        SKBitmap bitmap = new(image.Width, image.Height, rgba ? SKColorType.Rgba8888 : SKColorType.Bgra8888, SKAlphaType.Unpremul);

        try
        {
            Span<byte> pixels = new((void*)bitmap.GetPixels(out nint length), (int)length);
            if (rgba)
                image.ConvertTo<ColorRGBA>(pixels, bitmap.RowBytes);
            else
                image.ConvertTo<ColorBGRA>(pixels, bitmap.RowBytes);
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }

        return bitmap;
    }

    /// <summary>
    /// Encodes <paramref name="image"/> as PNG.
    /// </summary>
    /// <param name="image">The image to encode.</param>
    /// <returns>The PNG-encoded data.</returns>
    public static byte[] ToPng(this IImage image)
    {
        using SKImage skImage = image.ToSKImage();
        using SKData data = skImage.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <inheritdoc cref="ToPng(IImage)"/>
    public static byte[] ToPng<T>(this Image<T> image)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().ToPng();
    }

    /// <inheritdoc cref="ToPng(IImage)"/>
    public static byte[] ToPng<T>(this RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        using SKImage skImage = image.ToSKImage();
        using SKData data = skImage.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>
    /// Copies the pixels of <paramref name="skImage"/> into a new <see cref="Image{T}"/> of the color format <see cref="ColorBGRA"/>.
    /// </summary>
    /// <param name="skImage">The image to convert.</param>
    /// <returns>The new image.</returns>
    /// <exception cref="InvalidOperationException">The pixels of <paramref name="skImage"/> can't be read.</exception>
    public static Image<ColorBGRA> ToImage(this SKImage skImage) => skImage.ToImage<ColorBGRA>();

    /// <summary>
    /// Copies the pixels of <paramref name="skImage"/> into a new <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="skImage">The image to convert.</param>
    /// <typeparam name="T">The color format of the new image.</typeparam>
    /// <returns>The new image.</returns>
    /// <exception cref="InvalidOperationException">The pixels of <paramref name="skImage"/> can't be read.</exception>
    public static Image<T> ToImage<T>(this SKImage skImage)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(skImage);

        // Raster images expose their pixels. Everything else, like lazily decoded images, has to be read.
        using SKPixmap? pixmap = skImage.PeekPixels();
        if (pixmap != null) return ToImage<T>(pixmap);

        return ReadPixels<T>(skImage.Width, skImage.Height, skImage.AlphaType, (info, pixels) => skImage.ReadPixels(info, pixels, info.RowBytes))
            ?? throw new InvalidOperationException("The pixels of the image could not be read.");
    }

    /// <summary>
    /// Copies the pixels of <paramref name="bitmap"/> into a new <see cref="Image{T}"/> of the color format <see cref="ColorBGRA"/>.
    /// </summary>
    /// <param name="bitmap">The bitmap to convert.</param>
    /// <returns>The new image.</returns>
    /// <exception cref="ArgumentException"><paramref name="bitmap"/> has a size but no pixels.</exception>
    public static Image<ColorBGRA> ToImage(this SKBitmap bitmap) => bitmap.ToImage<ColorBGRA>();

    /// <summary>
    /// Copies the pixels of <paramref name="bitmap"/> into a new <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="bitmap">The bitmap to convert.</param>
    /// <typeparam name="T">The color format of the new image.</typeparam>
    /// <returns>The new image.</returns>
    /// <exception cref="ArgumentException"><paramref name="bitmap"/> has a size but no pixels.</exception>
    public static Image<T> ToImage<T>(this SKBitmap bitmap)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        using SKPixmap? pixmap = bitmap.PeekPixels();
        if (pixmap == null)
        {
            if ((bitmap.Width == 0) || (bitmap.Height == 0)) return Image<T>.Create([], bitmap.Width, bitmap.Height);

            throw new ArgumentException("The bitmap has no pixels.", nameof(bitmap));
        }

        return ToImage<T>(pixmap);
    }

    private static Image<T> ToImage<T>(SKPixmap pixmap)
        where T : unmanaged, IColor<T>
    {
        bool straightAlpha = pixmap.AlphaType is SKAlphaType.Unpremul or SKAlphaType.Opaque;

        if (straightAlpha && (pixmap.ColorType == SKColorType.Bgra8888))
            return RefImage<ColorBGRA>.Wrap(pixmap.GetPixelSpan(), pixmap.Width, pixmap.Height, pixmap.RowBytes).ConvertTo<T>();

        if (straightAlpha && (pixmap.ColorType == SKColorType.Rgba8888))
            return RefImage<ColorRGBA>.Wrap(pixmap.GetPixelSpan(), pixmap.Width, pixmap.Height, pixmap.RowBytes).ConvertTo<T>();

        return ReadPixels<T>(pixmap.Width, pixmap.Height, pixmap.AlphaType, (info, pixels) => pixmap.ReadPixels(info, pixels, info.RowBytes))
            ?? throw new InvalidOperationException("The pixels could not be read.");
    }

    /// <summary>
    /// Lets SkiaSharp write the pixels with straight alpha directly into the buffer of a new image.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="sourceAlphaType">The alpha type of the source. Opaque sources stay opaque, all others are read as straight alpha.</param>
    /// <param name="read">Writes the pixels in the given format to the given address, rows <c>info.RowBytes</c> apart. Returns whether that worked.</param>
    /// <typeparam name="T">The color format of the new image.</typeparam>
    /// <returns>The new image, or <see langword="null"/> if <paramref name="read"/> failed.</returns>
    internal static unsafe Image<T>? ReadPixels<T>(int width, int height, SKAlphaType sourceAlphaType, Func<SKImageInfo, nint, bool> read)
        where T : unmanaged, IColor<T>
    {
        bool rgba = typeof(T) == typeof(ColorRGBA);
        SKImageInfo info = new(width, height, rgba ? SKColorType.Rgba8888 : SKColorType.Bgra8888, sourceAlphaType == SKAlphaType.Opaque ? SKAlphaType.Opaque : SKAlphaType.Unpremul);
        int size = checked((int)info.BytesSize64);

        if (T.ColorFormat.BytesPerPixel == 4)
        {
            byte[] buffer = GC.AllocateUninitializedArray<byte>(size);
            fixed (byte* ptr = buffer)
                if (!read(info, (nint)ptr)) return null;

            // SkiaSharp has no ARGB or ABGR color type, so these are reordered in place.
            if (!rgba && (typeof(T) != typeof(ColorBGRA)))
                MemoryMarshal.Cast<byte, ColorBGRA>(buffer.AsSpan()).ConvertInPlace<ColorBGRA, T>();

            return Image<T>.Wrap(buffer, width, height, info.RowBytes);
        }

        // SkiaSharp has no 24 bit color type, so these take a detour through a temporary 32 bit buffer.
        byte[] temp = ArrayPool<byte>.Shared.Rent(size);
        try
        {
            fixed (byte* ptr = temp)
                if (!read(info, (nint)ptr)) return null;

            return RefImage<ColorBGRA>.Wrap(temp.AsSpan(0, size), width, height, info.RowBytes).ConvertTo<T>();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(temp);
        }
    }

    private readonly struct ToSKBitmapOperation : IImageOperation<SKBitmap>
    {
        public SKBitmap Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image.ToSKBitmap();
    }
}
