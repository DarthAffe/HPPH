using SkiaSharp;

namespace HPPH.SkiaSharp;

#pragma warning disable CA1508

public static class ImageHelper
{
    public static IImage LoadImage(string path) => LoadImage<ColorBGRA>(path);

    public static IImage<T> LoadImage<T>(string path)
        where T : unmanaged, IColor<T>
    {
        using SKCodec codec = SKCodec.Create(path) ?? throw new InvalidDataException($"'{path}' is not an image SkiaSharp can decode.");
        return Decode<T>(codec);
    }

    public static IImage LoadImage(Stream stream) => LoadImage<ColorBGRA>(stream);

    public static IImage<T> LoadImage<T>(Stream stream)
        where T : unmanaged, IColor<T>
    {
        using SKData data = SKData.Create(stream);
        using SKCodec codec = SKCodec.Create(data) ?? throw new InvalidDataException("The stream does not contain an image SkiaSharp can decode.");
        return Decode<T>(codec);
    }

    private static IImage<T> Decode<T>(SKCodec codec)
        where T : unmanaged, IColor<T>
    {
        SKImageInfo info = codec.Info;
        SKColorType colorType = typeof(T) == typeof(ColorRGBA) ? SKColorType.Rgba8888 : SKColorType.Bgra8888;
        SKImageInfo target = new(info.Width, info.Height, colorType, info.AlphaType == SKAlphaType.Opaque ? SKAlphaType.Opaque : SKAlphaType.Unpremul);

        using SKBitmap bitmap = SKBitmap.Decode(codec, target) ?? throw new InvalidDataException("The image could not be decoded.");
        return bitmap.ToImage<T>();
    }
}
