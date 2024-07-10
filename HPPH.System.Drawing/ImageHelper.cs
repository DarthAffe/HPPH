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
    public static IImage LoadImage(Stream stream)
    {
        using Bitmap bmp = new(stream);
        return bmp.ToImage();
    }
}