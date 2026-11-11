using SkiaSharp;

namespace HPPH.SkiaSharp;

#pragma warning disable CA1508

/// <summary>
/// Contains methods to load images using SkiaSharp.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Loads the image file at <paramref name="path"/> as an <see cref="Image{T}"/> of the color format <see cref="ColorBGRA"/>.
    /// </summary>
    /// <param name="path">The path of the image file.</param>
    /// <returns>The loaded image.</returns>
    /// <exception cref="InvalidDataException">The file can't be decoded.</exception>
    public static Image<ColorBGRA> LoadImage(string path) => LoadImage<ColorBGRA>(path);

    /// <summary>
    /// Loads the image file at <paramref name="path"/> as an <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="path">The path of the image file.</param>
    /// <typeparam name="T">The color format of the loaded image.</typeparam>
    /// <returns>The loaded image.</returns>
    /// <exception cref="InvalidDataException">The file can't be decoded.</exception>
    public static Image<T> LoadImage<T>(string path)
        where T : unmanaged, IColor<T>
    {
        using SKCodec codec = SKCodec.Create(path) ?? throw new InvalidDataException($"'{path}' is not an image SkiaSharp can decode.");
        return Decode<T>(codec);
    }

    /// <summary>
    /// Loads the image contained in <paramref name="stream"/> as an <see cref="Image{T}"/> of the color format <see cref="ColorBGRA"/>.
    /// </summary>
    /// <param name="stream">The stream containing the image.</param>
    /// <returns>The loaded image.</returns>
    /// <exception cref="InvalidDataException">The stream can't be decoded.</exception>
    public static Image<ColorBGRA> LoadImage(Stream stream) => LoadImage<ColorBGRA>(stream);

    /// <summary>
    /// Loads the image contained in <paramref name="stream"/> as an <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="stream">The stream containing the image.</param>
    /// <typeparam name="T">The color format of the loaded image.</typeparam>
    /// <returns>The loaded image.</returns>
    /// <exception cref="InvalidDataException">The stream can't be decoded.</exception>
    public static Image<T> LoadImage<T>(Stream stream)
        where T : unmanaged, IColor<T>
    {
        using SKData data = SKData.Create(stream);
        using SKCodec codec = SKCodec.Create(data) ?? throw new InvalidDataException("The stream does not contain an image SkiaSharp can decode.");
        return Decode<T>(codec);
    }

    private static Image<T> Decode<T>(SKCodec codec)
        where T : unmanaged, IColor<T>
    {
        SKImageInfo info = codec.Info;
        SKColorType colorType = typeof(T) == typeof(ColorRGBA) ? SKColorType.Rgba8888 : SKColorType.Bgra8888;
        SKImageInfo target = new(info.Width, info.Height, colorType, info.AlphaType == SKAlphaType.Opaque ? SKAlphaType.Opaque : SKAlphaType.Unpremul);

        using SKBitmap bitmap = SKBitmap.Decode(codec, target) ?? throw new InvalidDataException("The image could not be decoded.");
        return bitmap.ToImage<T>();
    }
}
