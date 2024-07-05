using System.Drawing;

namespace HPPH.Test;

internal static class ImageHelper
{
    #region Methods

    public static ColorRGB[] Get3ByteColorsFromImage(string file)
    {
        using FileStream stream = File.OpenRead(file);
        using Bitmap bmp = new(stream);

        ColorRGB[] colors = new ColorRGB[bmp.Width * bmp.Height];
        int i = 0;
        for (int x = 0; x < bmp.Width; x++)
            for (int y = 0; y < bmp.Height; y++)
            {
                Color color = bmp.GetPixel(x, y);
                colors[i++] = new ColorRGB(color.R, color.G, color.B);
            }

        return colors;
    }

    public static ColorRGBA[] Get4ByteColorsFromImage(string file)
    {
        using FileStream stream = File.OpenRead(file);
        using Bitmap bmp = new(stream);

        ColorRGBA[] colors = new ColorRGBA[bmp.Width * bmp.Height];
        int i = 0;
        for (int x = 0; x < bmp.Width; x++)
            for (int y = 0; y < bmp.Height; y++)
            {
                Color color = bmp.GetPixel(x, y);
                colors[i++] = new ColorRGBA(color.R, color.G, color.B, color.A);
            }

        return colors;
    }

    #endregion
}