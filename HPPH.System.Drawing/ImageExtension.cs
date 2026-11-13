using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace HPPH.System.Drawing;

/// <summary>
/// Contains extension methods to convert between HPPH images and System.Drawing bitmaps.
/// </summary>
public static class ImageExtension
{
    /// <summary>
    /// Copies the pixels of <paramref name="image"/> into a new <see cref="Bitmap"/>.
    /// </summary>
    /// <remarks>
    /// The bitmap is <see cref="PixelFormat.Format24bppRgb"/> for 3-byte color formats and <see cref="PixelFormat.Format32bppArgb"/> for 4-byte color formats.
    /// </remarks>
    /// <param name="image">The image to convert.</param>
    /// <returns>The new <see cref="Bitmap"/>.</returns>
    [SupportedOSPlatform("windows")]
    public static Bitmap ToBitmap(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<ToBitmapOperation, Bitmap>(default);
    }

    /// <inheritdoc cref="ToBitmap(IImage)"/>
    [SupportedOSPlatform("windows")]
    public static Bitmap ToBitmap<T>(this Image<T> image)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().ToBitmap();
    }

    /// <inheritdoc cref="ToBitmap(IImage)"/>
    [SupportedOSPlatform("windows")]
    public static Bitmap ToBitmap<T>(this RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        PixelFormat format = T.ColorFormat.BytesPerPixel switch
        {
            3 => PixelFormat.Format24bppRgb,
            4 => PixelFormat.Format32bppArgb,
            _ => throw new NotSupportedException($"Unsupported color format '{T.ColorFormat}'.")
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

    /// <summary>
    /// Encodes <paramref name="image"/> as PNG.
    /// </summary>
    /// <param name="image">The image to encode.</param>
    /// <returns>The PNG-encoded data.</returns>
    [SupportedOSPlatform("windows")]
    public static byte[] ToPng(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        using Bitmap bitmap = image.ToBitmap();
        using MemoryStream ms = new();
        bitmap.Save(ms, ImageFormat.Png);

        return ms.ToArray();
    }

    /// <inheritdoc cref="ToPng(IImage)"/>
    [SupportedOSPlatform("windows")]
    public static byte[] ToPng<T>(this Image<T> image)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().ToPng();
    }

    /// <inheritdoc cref="ToPng(IImage)"/>
    [SupportedOSPlatform("windows")]
    public static byte[] ToPng<T>(this RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        using Bitmap bitmap = image.ToBitmap();
        using MemoryStream ms = new();
        bitmap.Save(ms, ImageFormat.Png);

        return ms.ToArray();
    }

    /// <summary>
    /// Copies the pixels of <paramref name="bitmap"/> into a new image.
    /// </summary>
    /// <remarks>
    /// The image is <see cref="ColorBGR"/> for <see cref="PixelFormat.Format24bppRgb"/>-bitmaps and <see cref="ColorBGRA"/> otherwise.
    /// </remarks>
    /// <param name="bitmap">The bitmap to convert.</param>
    /// <returns>The new image.</returns>
    [SupportedOSPlatform("windows")]
    public static IImage ToImage(this Bitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        return bitmap.PixelFormat == PixelFormat.Format24bppRgb ? bitmap.ToImage<ColorBGR>() : bitmap.ToImage<ColorBGRA>();
    }

    /// <summary>
    /// Copies the pixels of <paramref name="bitmap"/> into a new <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="bitmap">The bitmap to convert.</param>
    /// <typeparam name="T">The color format of the new image.</typeparam>
    /// <returns>The new image.</returns>
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

    [SupportedOSPlatform("windows")]
    private readonly struct ToBitmapOperation : IImageOperation<Bitmap>
    {
        public Bitmap Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image.ToBitmap();
    }
}
