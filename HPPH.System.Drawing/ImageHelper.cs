using System.Drawing;
using System.Runtime.Versioning;

namespace HPPH.System.Drawing;

/// <summary>
/// Contains methods to load images using System.Drawing.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Loads the image file at <paramref name="path"/>.
    /// </summary>
    /// <remarks>
    /// The image is <see cref="ColorBGR"/> for 24 bit images and <see cref="ColorBGRA"/> otherwise.
    /// </remarks>
    /// <param name="path">The path of the image file.</param>
    /// <returns>The loaded image.</returns>
    [SupportedOSPlatform("windows")]
    public static IImage LoadImage(string path)
    {
        using Bitmap bmp = new(path);
        return bmp.ToImage();
    }

    /// <summary>
    /// Loads the image file at <paramref name="path"/> as an <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="path">The path of the image file.</param>
    /// <typeparam name="T">The color format of the loaded image.</typeparam>
    /// <returns>The loaded image.</returns>
    [SupportedOSPlatform("windows")]
    public static Image<T> LoadImage<T>(string path)
        where T : unmanaged, IColor<T>
    {
        using Bitmap bmp = new(path);
        return bmp.ToImage<T>();
    }

    /// <summary>
    /// Loads the image contained in <paramref name="stream"/>.
    /// </summary>
    /// <remarks>
    /// The image is <see cref="ColorBGR"/> for 24 bit images and <see cref="ColorBGRA"/> otherwise.
    /// </remarks>
    /// <param name="stream">The stream containing the image.</param>
    /// <returns>The loaded image.</returns>
    [SupportedOSPlatform("windows")]
    public static IImage LoadImage(Stream stream)
    {
        using Bitmap bmp = new(stream);
        return bmp.ToImage();
    }

    /// <summary>
    /// Loads the image contained in <paramref name="stream"/> as an <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="stream">The stream containing the image.</param>
    /// <typeparam name="T">The color format of the loaded image.</typeparam>
    /// <returns>The loaded image.</returns>
    [SupportedOSPlatform("windows")]
    public static Image<T> LoadImage<T>(Stream stream)
        where T : unmanaged, IColor<T>
    {
        using Bitmap bmp = new(stream);
        return bmp.ToImage<T>();
    }
}