using System.Drawing;

namespace HPPH.Test;

internal static class ImageHelper
{
    #region Methods

    public static T[] GetColorsFromImage<T>(string file)
        where T : struct, IColor
    {
        using FileStream stream = File.OpenRead(file);
        using Bitmap bmp = new(stream);

        return GetColors<T>(bmp);
    }

    public static Image<T> GetImage<T>(string file)
        where T : struct, IColor
    {
        using FileStream stream = File.OpenRead(file);
        using Bitmap bmp = new(stream);

        T[] colors = new T[bmp.Width * bmp.Height];
        int i = 0;
        for (int x = 0; x < bmp.Width; x++)
            for (int y = 0; y < bmp.Height; y++)
            {
                Color color = bmp.GetPixel(x, y);
                colors[i++] = (T)T.Create(color.R, color.G, color.B, color.A);
            }

        return Image<T>.Create(GetColors<T>(bmp), bmp.Width, bmp.Height);
    }

    private static T[] GetColors<T>(Bitmap bmp)
        where T : struct, IColor
    {
        T[] colors = new T[bmp.Width * bmp.Height];
        int i = 0;
        for (int x = 0; x < bmp.Width; x++)
            for (int y = 0; y < bmp.Height; y++)
            {
                Color color = bmp.GetPixel(x, y);
                colors[i++] = (T)T.Create(color.R, color.G, color.B, color.A);
            }

        return colors;
    }

    #endregion
}