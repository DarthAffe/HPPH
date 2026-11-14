using System.IO;
using HPPH;
using DrawingImageExtension = HPPH.System.Drawing.ImageExtension;
using DrawingImageHelper = HPPH.System.Drawing.ImageHelper;
using SkiaImageExtension = HPPH.SkiaSharp.ImageExtension;
using SkiaImageHelper = HPPH.SkiaSharp.ImageHelper;
using WpfImageExtension = HPPH.WPF.ImageExtension;
using WpfImageHelper = HPPH.WPF.ImageHelper;

namespace HPPHExample;

public enum ImageLoader
{
    WPF,
    SystemDrawing,
    SkiaSharp
}

internal static class ImageIO
{
    #region Methods

    public static IImage Load(string path, ImageLoader loader)
        => loader switch
        {
            ImageLoader.WPF => WpfImageHelper.LoadImage(path),
            ImageLoader.SystemDrawing => DrawingImageHelper.LoadImage(path),
            ImageLoader.SkiaSharp => SkiaImageHelper.LoadImage(path),
            _ => throw new ArgumentOutOfRangeException(nameof(loader))
        };

    public static void SavePng(IImage image, string path, ImageLoader loader)
    {
        byte[] data = loader switch
        {
            ImageLoader.WPF => WpfImageExtension.ToPng(image),
            ImageLoader.SystemDrawing => DrawingImageExtension.ToPng(image),
            ImageLoader.SkiaSharp => SkiaImageExtension.ToPng(image),
            _ => throw new ArgumentOutOfRangeException(nameof(loader))
        };

        File.WriteAllBytes(path, data);
    }

    #endregion
}
