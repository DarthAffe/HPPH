using System.IO;
using System.Windows.Media.Imaging;

namespace HPPH.WPF;

/// <summary>
/// Contains methods to load images using WPF.
/// </summary>
public static class ImageHelper
{
    #region Methods

    /// <summary>
    /// Loads the image file at <paramref name="path"/>.
    /// </summary>
    /// <remarks>
    /// The image is <see cref="ColorBGR"/> or <see cref="ColorRGB"/> if WPF decodes the file to 24 bit and <see cref="ColorBGRA"/> otherwise.
    /// </remarks>
    /// <param name="path">The path of the image file.</param>
    /// <returns>The loaded image.</returns>
    public static IImage LoadImage(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return LoadImage(stream);
    }

    /// <summary>
    /// Loads the image file at <paramref name="path"/> as an <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="path">The path of the image file.</param>
    /// <typeparam name="T">The color format of the loaded image.</typeparam>
    /// <returns>The loaded image.</returns>
    public static Image<T> LoadImage<T>(string path)
        where T : unmanaged, IColor<T>
    {
        using FileStream stream = File.OpenRead(path);
        return LoadImage<T>(stream);
    }

    /// <summary>
    /// Loads the image contained in <paramref name="stream"/>.
    /// </summary>
    /// <remarks>
    /// The image is <see cref="ColorBGR"/> or <see cref="ColorRGB"/> if WPF decodes the data to 24 bit and <see cref="ColorBGRA"/> otherwise.
    /// </remarks>
    /// <param name="stream">The stream containing the image.</param>
    /// <returns>The loaded image.</returns>
    public static IImage LoadImage(Stream stream) => Decode(stream).ToImage();

    /// <summary>
    /// Loads the image contained in <paramref name="stream"/> as an <see cref="Image{T}"/> of the color format <typeparamref name="T"/>.
    /// </summary>
    /// <param name="stream">The stream containing the image.</param>
    /// <typeparam name="T">The color format of the loaded image.</typeparam>
    /// <returns>The loaded image.</returns>
    public static Image<T> LoadImage<T>(Stream stream)
        where T : unmanaged, IColor<T>
        => Decode(stream).ToImage<T>();

    private static BitmapFrame Decode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad);
        return decoder.Frames[0];
    }

    #endregion
}
