using HPPH.SkiaSharp;
using SkiaSharp;

namespace HPPH.Test.Skia;

[TestClass]
public class SkiaTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void ImageConversion24Bit()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(1920, 1080);
        using SKImage bitmap = image.ToSKImage();
        IImage image2 = bitmap.ToImage();

        Assert.AreEqual(IColorFormat.BGRA, image2.ColorFormat);

        image2 = image2.ConvertTo<ColorRGB>();

        Assert.AreEqual(image, image2);
    }

    [TestMethod]
    public void ImageConversion32Bit()
    {
        Image<ColorRGBA> image = TestDataHelper.CreateTestImage<ColorRGBA>(1920, 1080);
        using SKImage bitmap = image.ToSKImage();
        IImage image2 = bitmap.ToImage();

        Assert.AreEqual(IColorFormat.BGRA, image2.ColorFormat);

        image2 = image2.ConvertTo<ColorRGBA>();

        Assert.AreEqual(image, image2);
    }

    [TestMethod]
    public void LoadFileToPngLoadStream()
    {
        foreach (string image in GetTestImages())
        {
            IImage img = SkiaSharp.ImageHelper.LoadImage(image);
            byte[] png = img.ToPng();
            using MemoryStream ms = new(png);
            IImage img2 = SkiaSharp.ImageHelper.LoadImage(ms);

            Assert.AreEqual(img, img2);
        }
    }
}