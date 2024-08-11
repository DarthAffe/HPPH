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
}