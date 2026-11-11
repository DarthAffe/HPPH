// ReSharper disable InconsistentNaming

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
    public static unsafe SKBitmap ToSKBitmap(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        bool rgba = image is Image<ColorRGBA>;

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

    /// <summary>
    /// Copies the pixels of <paramref name="skImage"/> into a new <see cref="Image{T}"/> of the color format <see cref="ColorBGRA"/>.
    /// </summary>
    /// <param name="skImage">The image to convert.</param>
    /// <returns>The new image.</returns>
    public static Image<ColorBGRA> ToImage(this SKImage skImage) => skImage.ToImage<ColorBGRA>();

    /// <summary>
    /// Copies the pixels of <paramref name="skImage"/> into a new <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="skImage">The image to convert.</param>
    /// <typeparam name="T">The color format of the new image.</typeparam>
    /// <returns>The new image.</returns>
    public static Image<T> ToImage<T>(this SKImage skImage)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(skImage);

        using SKBitmap bitmap = SKBitmap.FromImage(skImage);
        return bitmap.ToImage<T>();
    }

    /// <summary>
    /// Copies the pixels of <paramref name="bitmap"/> into a new <see cref="Image{T}"/> of the color format <see cref="ColorBGRA"/>.
    /// </summary>
    /// <param name="bitmap">The bitmap to convert.</param>
    /// <returns>The new image.</returns>
    public static Image<ColorBGRA> ToImage(this SKBitmap bitmap) => bitmap.ToImage<ColorBGRA>();

    /// <summary>
    /// Copies the pixels of <paramref name="bitmap"/> into a new <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="bitmap">The bitmap to convert.</param>
    /// <typeparam name="T">The color format of the new image.</typeparam>
    /// <returns>The new image.</returns>
    public static Image<T> ToImage<T>(this SKBitmap bitmap)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        bool straightAlpha = bitmap.AlphaType is SKAlphaType.Unpremul or SKAlphaType.Opaque;

        if (straightAlpha && (bitmap.ColorType == SKColorType.Bgra8888))
            return RefImage<ColorBGRA>.Wrap(bitmap.GetPixelSpan(), bitmap.Width, bitmap.Height, bitmap.RowBytes).ConvertTo<T>();

        if (straightAlpha && (bitmap.ColorType == SKColorType.Rgba8888))
            return RefImage<ColorRGBA>.Wrap(bitmap.GetPixelSpan(), bitmap.Width, bitmap.Height, bitmap.RowBytes).ConvertTo<T>();

        return RefImage<ColorBGRA>.Wrap(MemoryMarshal.Cast<SKColor, ColorBGRA>(bitmap.Pixels), bitmap.Width, bitmap.Height).ConvertTo<T>();
    }
}
