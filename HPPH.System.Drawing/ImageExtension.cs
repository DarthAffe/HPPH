using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace HPPH.System.Drawing;

public static class ImageExtension
{
    [SupportedOSPlatform("windows")]
    public static Bitmap ToBitmap(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        PixelFormat format = image.ColorFormat.BytesPerPixel switch
        {
            3 => PixelFormat.Format24bppRgb,
            4 => PixelFormat.Format32bppArgb,
            _ => throw new NotSupportedException($"Unsupported color format '{image.ColorFormat}'.")
        };

        Bitmap bitmap = new(image.Width, image.Height, format);
        try
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, format);
            try
            {
                Span<byte> pixels = GetPixels(data);
                if (format == PixelFormat.Format24bppRgb)
                    image.ConvertTo<ColorBGR>(pixels, data.Stride);
                else
                    image.ConvertTo<ColorBGRA>(pixels, data.Stride);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }

        return bitmap;
    }

    [SupportedOSPlatform("windows")]
    public static byte[] ToPng(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        using Bitmap bitmap = image.ToBitmap();
        using MemoryStream ms = new();
        bitmap.Save(ms, ImageFormat.Png);

        return ms.ToArray();
    }

    [SupportedOSPlatform("windows")]
    public static IImage ToImage(this Bitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        return bitmap.PixelFormat == PixelFormat.Format24bppRgb ? bitmap.ToImage<ColorBGR>() : bitmap.ToImage<ColorBGRA>();
    }

    [SupportedOSPlatform("windows")]
    public static Image<T> ToImage<T>(this Bitmap bitmap)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        bool is24Bit = bitmap.PixelFormat == PixelFormat.Format24bppRgb;
        PixelFormat format = is24Bit ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb;

        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, format);
        try
        {
            ReadOnlySpan<byte> pixels = GetPixels(data);
            return is24Bit
                       ? RefImage<ColorBGR>.Wrap(pixels, data.Width, data.Height, data.Stride).ConvertTo<T>()
                       : RefImage<ColorBGRA>.Wrap(pixels, data.Width, data.Height, data.Stride).ConvertTo<T>();
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    [SupportedOSPlatform("windows")]
    private static unsafe Span<byte> GetPixels(BitmapData data)
    {
        if (data.Stride < 0) throw new NotSupportedException("Bottom-up bitmaps (negative stride) are not supported.");

        return new Span<byte>(data.Scan0.ToPointer(), data.Stride * data.Height);
    }
}
