using System.Buffers;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HPPH.WPF;

/// <summary>
/// Contains extension methods to convert between HPPH images and WPF bitmaps.
/// </summary>
public static class ImageExtension
{
    #region Methods

    /// <summary>
    /// Copies the pixels of <paramref name="image"/> into a new frozen <see cref="BitmapSource"/>.
    /// </summary>
    /// <remarks>
    /// The bitmap is <see cref="PixelFormats.Rgb24"/> for <see cref="ColorRGB"/>, <see cref="PixelFormats.Bgr24"/> for <see cref="ColorBGR"/> and <see cref="PixelFormats.Bgra32"/> for 4-byte color formats.
    /// It is frozen, so it can be used on every thread.
    /// </remarks>
    /// <param name="image">The image to convert.</param>
    /// <returns>The new <see cref="BitmapSource"/>.</returns>
    public static BitmapSource ToBitmapSource(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<ToBitmapSourceOperation, BitmapSource>(default);
    }

    /// <inheritdoc cref="ToBitmapSource(IImage)"/>
    public static BitmapSource ToBitmapSource<T>(this Image<T> image)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().ToBitmapSource();
    }

    /// <inheritdoc cref="ToBitmapSource(IImage)"/>
    public static BitmapSource ToBitmapSource<T>(this RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        WriteableBitmap bitmap = image.ToWriteableBitmap();
        bitmap.Freeze();

        return bitmap;
    }

    /// <summary>
    /// Copies the pixels of <paramref name="image"/> into a new <see cref="WriteableBitmap"/>.
    /// </summary>
    /// <remarks>
    /// The bitmap is <see cref="PixelFormats.Rgb24"/> for <see cref="ColorRGB"/>, <see cref="PixelFormats.Bgr24"/> for <see cref="ColorBGR"/> and <see cref="PixelFormats.Bgra32"/> for 4-byte color formats.
    /// Use <see cref="CopyTo(IImage, WriteableBitmap)"/> to update it later on.
    /// </remarks>
    /// <param name="image">The image to convert.</param>
    /// <returns>The new <see cref="WriteableBitmap"/>.</returns>
    public static WriteableBitmap ToWriteableBitmap(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<ToWriteableBitmapOperation, WriteableBitmap>(default);
    }

    /// <inheritdoc cref="ToWriteableBitmap(IImage)"/>
    public static WriteableBitmap ToWriteableBitmap<T>(this Image<T> image)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().ToWriteableBitmap();
    }

    /// <inheritdoc cref="ToWriteableBitmap(IImage)"/>
    public static WriteableBitmap ToWriteableBitmap<T>(this RefImage<T> image)
        where T : unmanaged, IColor<T>
    {
        PixelFormat format = typeof(T) == typeof(ColorRGB) ? PixelFormats.Rgb24
                           : typeof(T) == typeof(ColorBGR) ? PixelFormats.Bgr24
                           : PixelFormats.Bgra32;

        WriteableBitmap bitmap = new(image.Width, image.Height, 96, 96, format, null);
        image.CopyTo(bitmap);

        return bitmap;
    }

    /// <summary>
    /// Copies the pixels of <paramref name="image"/> into the back buffer of <paramref name="bitmap"/>, converting them to the format of the bitmap.
    /// </summary>
    /// <remarks>
    /// Supported are <see cref="PixelFormats.Bgra32"/>, <see cref="PixelFormats.Bgr32"/>, <see cref="PixelFormats.Bgr24"/> and <see cref="PixelFormats.Rgb24"/>.
    /// Has to be called on the thread <paramref name="bitmap"/> belongs to.
    /// </remarks>
    /// <param name="image">The image to copy.</param>
    /// <param name="bitmap">The bitmap to write to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="bitmap"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="bitmap"/> doesn't have the size of <paramref name="image"/>.</exception>
    /// <exception cref="NotSupportedException">The pixel format of <paramref name="bitmap"/> is not supported.</exception>
    public static void CopyTo(this IImage image, WriteableBitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(image);

        image.Execute<CopyToOperation, bool>(new CopyToOperation(bitmap));
    }

    /// <inheritdoc cref="CopyTo(IImage, WriteableBitmap)"/>
    public static void CopyTo<T>(this Image<T> image, WriteableBitmap bitmap)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        image.AsRefImage().CopyTo(bitmap);
    }

    /// <inheritdoc cref="CopyTo(IImage, WriteableBitmap)"/>
    public static unsafe void CopyTo<T>(this RefImage<T> image, WriteableBitmap bitmap)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        if ((bitmap.PixelWidth != image.Width) || (bitmap.PixelHeight != image.Height))
            throw new ArgumentException($"The bitmap needs the size of the image. {bitmap.PixelWidth}x{bitmap.PixelHeight} != {image.Width}x{image.Height}", nameof(bitmap));

        PixelFormat format = bitmap.Format;
        if ((format != PixelFormats.Bgra32) && (format != PixelFormats.Bgr32) && (format != PixelFormats.Bgr24) && (format != PixelFormats.Rgb24))
            throw new NotSupportedException($"Unsupported pixel format '{format}'.");

        bitmap.Lock();
        try
        {
            int stride = bitmap.BackBufferStride;
            Span<byte> pixels = new((void*)bitmap.BackBuffer, stride * bitmap.PixelHeight);

            if (format == PixelFormats.Bgr24)
                image.ConvertTo<ColorBGR>(pixels, stride);
            else if (format == PixelFormats.Rgb24)
                image.ConvertTo<ColorRGB>(pixels, stride);
            else
                image.ConvertTo<ColorBGRA>(pixels, stride);

            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
        }
        finally
        {
            bitmap.Unlock();
        }
    }

    /// <summary>
    /// Encodes <paramref name="image"/> as PNG.
    /// </summary>
    /// <param name="image">The image to encode.</param>
    /// <returns>The PNG-encoded data.</returns>
    public static byte[] ToPng(this IImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return EncodePng(image.ToBitmapSource());
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
        => EncodePng(image.ToBitmapSource());

    /// <summary>
    /// Copies the pixels of <paramref name="bitmap"/> into a new image.
    /// </summary>
    /// <remarks>
    /// The image is <see cref="ColorBGR"/> for <see cref="PixelFormats.Bgr24"/>-bitmaps, <see cref="ColorRGB"/> for <see cref="PixelFormats.Rgb24"/>-bitmaps and <see cref="ColorBGRA"/> otherwise.
    /// </remarks>
    /// <param name="bitmap">The bitmap to convert.</param>
    /// <returns>The new image.</returns>
    public static IImage ToImage(this BitmapSource bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        PixelFormat format = bitmap.Format;
        if (format == PixelFormats.Bgr24) return bitmap.ToImage<ColorBGR>();
        if (format == PixelFormats.Rgb24) return bitmap.ToImage<ColorRGB>();
        return bitmap.ToImage<ColorBGRA>();
    }

    /// <summary>
    /// Copies the pixels of <paramref name="bitmap"/> into a new <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Pixel formats other than <see cref="PixelFormats.Bgra32"/>, <see cref="PixelFormats.Bgr24"/> and <see cref="PixelFormats.Rgb24"/> are converted to <see cref="PixelFormats.Bgra32"/> by WPF first.
    /// Premultiplied sources are converted to straight alpha.
    /// </remarks>
    /// <param name="bitmap">The bitmap to convert.</param>
    /// <typeparam name="T">The color format of the new image.</typeparam>
    /// <returns>The new image.</returns>
    public static Image<T> ToImage<T>(this BitmapSource bitmap)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        PixelFormat format = bitmap.Format;
        if (format == PixelFormats.Bgr24) return CopyPixels<ColorBGR, T>(bitmap);
        if (format == PixelFormats.Rgb24) return CopyPixels<ColorRGB, T>(bitmap);
        if (format == PixelFormats.Bgra32) return CopyPixels<ColorBGRA, T>(bitmap);
        return CopyPixels<ColorBGRA, T>(new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0));
    }

    private static Image<TTarget> CopyPixels<TSource, TTarget>(BitmapSource bitmap)
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * TSource.ColorFormat.BytesPerPixel;
        int size = stride * height;

        if (typeof(TSource) == typeof(TTarget))
        {
            byte[] buffer = new byte[size];
            bitmap.CopyPixels(buffer, stride, 0);

            return Image<TTarget>.Wrap(buffer, width, height, stride);
        }

        byte[] rented = ArrayPool<byte>.Shared.Rent(size);
        try
        {
            bitmap.CopyPixels(rented, stride, 0);

            return RefImage<TSource>.Wrap(rented.AsSpan(0, size), width, height, stride).ConvertTo<TTarget>();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static byte[] EncodePng(BitmapSource bitmap)
    {
        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using MemoryStream stream = new();
        encoder.Save(stream);

        return stream.ToArray();
    }

    #endregion

    private readonly struct ToBitmapSourceOperation : IImageOperation<BitmapSource>
    {
        public BitmapSource Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image.ToBitmapSource();
    }

    private readonly struct ToWriteableBitmapOperation : IImageOperation<WriteableBitmap>
    {
        public WriteableBitmap Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image.ToWriteableBitmap();
    }

    private readonly struct CopyToOperation(WriteableBitmap bitmap) : IImageOperation<bool>
    {
        public bool Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T>
        {
            image.CopyTo(bitmap);
            return true;
        }
    }
}
