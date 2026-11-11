using System.Drawing;
using HPPH.System.Drawing;

namespace HPPH.Test.SystemDrawing;

[TestClass]
public class SystemDrawingTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void ImageConversion24Bit()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(1920, 1080);
        using Bitmap bitmap = image.ToBitmap();
        IImage image2 = bitmap.ToImage();

        Assert.AreEqual(IColorFormat.BGR, image2.ColorFormat);
        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                Assert.AreEqual(image[x, y].A, image2[x, y].A, $"{x}-{y}");
                Assert.AreEqual(image[x, y].R, image2[x, y].R, $"{x}-{y}");
                Assert.AreEqual(image[x, y].G, image2[x, y].G, $"{x}-{y}");
                Assert.AreEqual(image[x, y].B, image2[x, y].B, $"{x}-{y}");
            }

        image2 = image2.ConvertTo<ColorRGB>();

        Assert.AreEqual(image, image2);
    }

    [TestMethod]
    public void ImageConversion32Bit()
    {
        Image<ColorRGBA> image = TestDataHelper.CreateTestImage<ColorRGBA>(1920, 1080);
        using Bitmap bitmap = image.ToBitmap();
        IImage image2 = bitmap.ToImage();

        Assert.AreEqual(IColorFormat.BGRA, image2.ColorFormat);
        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                Assert.AreEqual(image[x, y].A, image2[x, y].A);
                Assert.AreEqual(image[x, y].R, image2[x, y].R);
                Assert.AreEqual(image[x, y].G, image2[x, y].G);
                Assert.AreEqual(image[x, y].B, image2[x, y].B);
            }

        image2 = image2.ConvertTo<ColorRGBA>();

        Assert.AreEqual(image, image2);
    }

    [TestMethod]
    public void LoadFileToPngLoadStream()
    {
        foreach (string image in GetTestImages())
        {
            IImage img = System.Drawing.ImageHelper.LoadImage(image);
            byte[] png = img.ToPng();
            using MemoryStream ms = new(png);
            IImage img2 = System.Drawing.ImageHelper.LoadImage(ms);

            Assert.AreEqual(img, img2);
        }
    }

    [TestMethod]
    public void ToImageAcceptsOtherPixelFormats()
    {
        foreach (global::System.Drawing.Imaging.PixelFormat format in new[] { global::System.Drawing.Imaging.PixelFormat.Format32bppRgb, global::System.Drawing.Imaging.PixelFormat.Format32bppPArgb, global::System.Drawing.Imaging.PixelFormat.Format16bppRgb565 })
        {
            using Bitmap bitmap = new(7, 5, format);
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    bitmap.SetPixel(x, y, Color.FromArgb(255, x * 32, y * 48, 200));

            IImage image = bitmap.ToImage();

            Assert.AreEqual(IColorFormat.BGRA, image.ColorFormat, format.ToString());
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color expected = bitmap.GetPixel(x, y);
                    IColor actual = image[x, y];
                    Assert.AreEqual(expected.R, actual.R, $"{format} R at {x},{y}");
                    Assert.AreEqual(expected.G, actual.G, $"{format} G at {x},{y}");
                    Assert.AreEqual(expected.B, actual.B, $"{format} B at {x},{y}");
                    Assert.AreEqual(expected.A, actual.A, $"{format} A at {x},{y}");
                }
        }
    }

    [TestMethod]
    public void LoadImageIntoEveryFormat()
    {
        foreach (string image in GetTestImages())
        {
            LoadImageInto<ColorRGB>(image);
            LoadImageInto<ColorBGR>(image);
            LoadImageInto<ColorRGBA>(image);
            LoadImageInto<ColorBGRA>(image);
            LoadImageInto<ColorARGB>(image);
            LoadImageInto<ColorABGR>(image);
        }
    }

    private static void LoadImageInto<T>(string image)
        where T : unmanaged, IColor
    {
        IImage<T> expected = System.Drawing.ImageHelper.LoadImage(image).ConvertTo<T>();

        Assert.AreEqual(expected, System.Drawing.ImageHelper.LoadImage<T>(image), $"{T.ColorFormat.Name}: {image}");

        using FileStream stream = File.OpenRead(image);
        Assert.AreEqual(expected, System.Drawing.ImageHelper.LoadImage<T>(stream), $"{T.ColorFormat.Name}: {image} (stream)");
    }
}
