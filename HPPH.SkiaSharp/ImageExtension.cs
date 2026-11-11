// ReSharper disable InconsistentNaming

using System.Runtime.InteropServices;
using SkiaSharp;

namespace HPPH.SkiaSharp;

public static class ImageExtension
{
    public static SKImage ToSKImage(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        using SKBitmap bitmap = image.ToSKBitmap();
        return SKImage.FromBitmap(bitmap);
    }

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

    public static byte[] ToPng(this IImage image)
    {
        using SKImage skImage = image.ToSKImage();
        using SKData data = skImage.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public static Image<ColorBGRA> ToImage(this SKImage skImage) => skImage.ToImage<ColorBGRA>();

    public static Image<T> ToImage<T>(this SKImage skImage)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(skImage);

        using SKBitmap bitmap = SKBitmap.FromImage(skImage);
        return bitmap.ToImage<T>();
    }

    public static Image<ColorBGRA> ToImage(this SKBitmap bitmap) => bitmap.ToImage<ColorBGRA>();

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
