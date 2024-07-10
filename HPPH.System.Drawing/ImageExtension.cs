using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace HPPH.System.Drawing;

public static class ImageExtension
{
    public static Bitmap ToBitmap(this IImage image)
    {
        throw new NotImplementedException();
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
            image = Image<ColorARGB>.Create(buffer, data.Width, data.Height, data.Stride);
        else throw new NotSupportedException($"Unsupported pixel format '{bitmap.PixelFormat}'.");

        bitmap.UnlockBits(data);

        return image;
    }
}