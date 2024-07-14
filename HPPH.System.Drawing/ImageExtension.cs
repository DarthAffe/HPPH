using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace HPPH.System.Drawing;

public static class ImageExtension
{
    [SupportedOSPlatform("windows")]
    public static unsafe Bitmap ToBitmap(this IImage image)
    {
        switch (image.ColorFormat.BytesPerPixel)
        {
            case 3:
                {
                    Bitmap bitmap = new(image.Width, image.Height, PixelFormat.Format24bppRgb);
                    BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

                    IImage<ColorRGB> convertedImage = image.ConvertTo<ColorRGB>();

                    nint ptr = bmpData.Scan0;
                    foreach (ImageRow<ColorRGB> row in convertedImage.Rows)
                    {
                        row.CopyTo(new Span<byte>((void*)ptr, bmpData.Stride));
                        ptr += bmpData.Stride;
                    }

                    bitmap.UnlockBits(bmpData);

                    return bitmap;
                }

            case 4:
                {
                    Bitmap bitmap = new(image.Width, image.Height, PixelFormat.Format32bppArgb);
                    BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

                    IImage<ColorRGBA> convertedImage = image.ConvertTo<ColorRGBA>();

                    nint ptr = bmpData.Scan0;
                    foreach (ImageRow<ColorRGBA> row in convertedImage.Rows)
                    {
                        row.CopyTo(new Span<byte>((void*)ptr, bmpData.Stride));
                        ptr += bmpData.Stride;
                    }

                    bitmap.UnlockBits(bmpData);

                    return bitmap;
                }

            default:
                throw new NotSupportedException($"Unsupported color format '{image.ColorFormat}'.");
        }
    }

    [SupportedOSPlatform("windows")]
    public static byte[] ToPng(this IImage image)
    {
        using Bitmap bitmap = ToBitmap(image);
        using MemoryStream ms = new();
        bitmap.Save(ms, ImageFormat.Png);

        return ms.ToArray();
    }

    [SupportedOSPlatform("windows")]
    public static unsafe IImage ToImage(this Bitmap bitmap)
    {
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        ReadOnlySpan<byte> buffer = new(data.Scan0.ToPointer(), data.Stride * data.Height);

        IImage image;

        if (data.PixelFormat.HasFlag(PixelFormat.Format24bppRgb))
            image = Image<ColorRGB>.Create(buffer, data.Width, data.Height, data.Stride);
        else if (data.PixelFormat.HasFlag(PixelFormat.Format32bppArgb))
            image = Image<ColorRGBA>.Create(buffer, data.Width, data.Height, data.Stride);
        else throw new NotSupportedException($"Unsupported pixel format '{bitmap.PixelFormat}'.");

        bitmap.UnlockBits(data);

        return image;
    }
}