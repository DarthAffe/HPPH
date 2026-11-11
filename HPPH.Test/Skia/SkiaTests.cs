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

    [TestMethod]
    public void ToImageReadsEveryBitmapLayout()
    {
        (SKColorType ColorType, SKAlphaType AlphaType, bool Translucent)[] layouts =
        [
            (SKColorType.Bgra8888, SKAlphaType.Unpremul, true),
            (SKColorType.Rgba8888, SKAlphaType.Unpremul, true),
            (SKColorType.Bgra8888, SKAlphaType.Opaque, false),
            (SKColorType.Bgra8888, SKAlphaType.Premul, false),
            (SKColorType.Rgb565, SKAlphaType.Opaque, false)
        ];

        foreach ((SKColorType colorType, SKAlphaType alphaType, bool translucent) in layouts)
        {
            using SKBitmap bitmap = new(7, 5, colorType, alphaType);
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    bitmap.SetPixel(x, y, new SKColor((byte)(x * 32), (byte)(y * 48), 200, translucent ? (byte)(50 + (x * 20)) : (byte)255));

            IImage<ColorBGRA> image = bitmap.ToImage();

            string name = $"{colorType} {alphaType}";
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    SKColor expected = bitmap.GetPixel(x, y);
                    Assert.AreEqual(new ColorBGRA(expected.Blue, expected.Green, expected.Red, expected.Alpha), image[x, y], $"{name} at {x},{y}");
                }
        }
    }

    [TestMethod]
    public void ToSKBitmapOfRegion()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(40, 30);
        IImage region = ((IImage)image)[5, 3, 21, 17];

        using SKBitmap bitmap = region.ToSKBitmap();

        Assert.AreEqual(SKColorType.Bgra8888, bitmap.ColorType);
        for (int y = 0; y < region.Height; y++)
            for (int x = 0; x < region.Width; x++)
            {
                IColor expected = region[x, y];
                SKColor actual = bitmap.GetPixel(x, y);
                Assert.AreEqual(new SKColor(expected.R, expected.G, expected.B, expected.A), actual, $"{x},{y}");
            }
    }

    [TestMethod]
    public void LoadImageRejectsInvalidData()
    {
        using MemoryStream stream = new([1, 2, 3, 4, 5, 6, 7, 8]);
        Assert.ThrowsExactly<InvalidDataException>(() => SkiaSharp.ImageHelper.LoadImage(stream));
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
        where T : unmanaged, IColor<T>
    {
        IImage<T> expected = SkiaSharp.ImageHelper.LoadImage(image).ConvertTo<T>();

        Assert.AreEqual(expected, SkiaSharp.ImageHelper.LoadImage<T>(image), $"{T.ColorFormat.Name}: {image}");

        using FileStream stream = File.OpenRead(image);
        Assert.AreEqual(expected, SkiaSharp.ImageHelper.LoadImage<T>(stream), $"{T.ColorFormat.Name}: {image} (stream)");
    }
}