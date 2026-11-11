using System.Drawing;
using System.Runtime.Versioning;

namespace HPPH.System.Drawing;

public static class ImageHelper
{
    [SupportedOSPlatform("windows")]
    public static IImage LoadImage(string path)
    {
        using Bitmap bmp = new(path);
        return bmp.ToImage();
    }

    [SupportedOSPlatform("windows")]
    public static Image<T> LoadImage<T>(string path)
        where T : unmanaged, IColor<T>
    {
        using Bitmap bmp = new(path);
        return bmp.ToImage<T>();
    }

    [SupportedOSPlatform("windows")]
    public static IImage LoadImage(Stream stream)
    {
        using Bitmap bmp = new(stream);
        return bmp.ToImage();
    }

    [SupportedOSPlatform("windows")]
    public static Image<T> LoadImage<T>(Stream stream)
        where T : unmanaged, IColor<T>
    {
        using Bitmap bmp = new(stream);
        return bmp.ToImage<T>();
    }
}