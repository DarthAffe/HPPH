using SkiaSharp;

namespace HPPH.SkiaSharp;

public static class ImageHelper
{
    public static IImage LoadImage(string path)
    {
        using SKImage image = SKImage.FromEncodedData(path);
        return image.ToImage();
    }

    public static IImage LoadImage(Stream stream)
    {
        using SKImage image = SKImage.FromEncodedData(stream);
        return image.ToImage();
    }
}