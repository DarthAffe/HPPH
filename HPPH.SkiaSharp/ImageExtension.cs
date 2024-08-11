// ReSharper disable InconsistentNaming

using System.Runtime.InteropServices;
using SkiaSharp;

namespace HPPH.SkiaSharp;

public static class ImageExtension
{
    public static unsafe SKImage ToSKImage(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));

        using SKBitmap bitmap = image.ToSKBitmap();
        return SKImage.FromBitmap(bitmap);
    }

    public static unsafe SKBitmap ToSKBitmap(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));

        SKBitmap bitmap = new(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        nint pixelPtr = bitmap.GetPixels(out nint length);

        (image as IImage<ColorBGRA> ?? image.ConvertTo<ColorBGRA>()).CopyTo(new Span<byte>((void*)pixelPtr, (int)length));

        return bitmap;
    }

    public static byte[] ToPng(this IImage image)
    {
        using SKImage skImage = image.ToSKImage();
        return skImage.Encode(SKEncodedImageFormat.Png, 100).ToArray();
    }

    public static IImage<ColorBGRA> ToImage(this SKImage skImage) => SKBitmap.FromImage(skImage).ToImage();

    public static IImage<ColorBGRA> ToImage(this SKBitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap, nameof(bitmap));

        return Image<ColorBGRA>.Create(MemoryMarshal.Cast<SKColor, ColorBGRA>(bitmap.Pixels), bitmap.Width, bitmap.Height);
    }
}