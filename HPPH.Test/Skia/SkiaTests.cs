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

        Assert.IsTrue(image.ContentEquals(image2));
    }

    [TestMethod]
    public void ImageConversion32Bit()
    {
        Image<ColorRGBA> image = TestDataHelper.CreateTestImage<ColorRGBA>(1920, 1080);
        using SKImage bitmap = image.ToSKImage();
        IImage image2 = bitmap.ToImage();

        Assert.AreEqual(IColorFormat.BGRA, image2.ColorFormat);

        image2 = image2.ConvertTo<ColorRGBA>();

        Assert.IsTrue(image.ContentEquals(image2));
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

            Assert.IsTrue(img.ContentEquals(img2), image);
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
            (SKColorType.Rgb565, SKAlphaType.Opaque, false),
            (SKColorType.Gray8, SKAlphaType.Opaque, false)
        ];

        foreach ((SKColorType colorType, SKAlphaType alphaType, bool translucent) in layouts)
        {
            using SKBitmap bitmap = new(7, 5, colorType, alphaType);
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    bitmap.SetPixel(x, y, new SKColor((byte)(x * 32), (byte)(y * 48), 200, translucent ? (byte)(50 + (x * 20)) : (byte)255));

            Image<ColorBGRA> image = bitmap.ToImage();

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
    public void ToImageIntoEveryFormat()
    {
        foreach (string file in GetTestImages())
        {
            byte[] encoded = File.ReadAllBytes(file);

            // LoadImage decodes straight to unpremultiplied pixels, so it has the exact values of the file.
            Image<ColorBGRA> expected = SkiaSharp.ImageHelper.LoadImage(file);
            SKImageInfo info = new(expected.Width, expected.Height);

            using SKBitmap straight = SKBitmap.Decode(encoded, info.WithColorType(SKColorType.Rgba8888).WithAlphaType(SKAlphaType.Unpremul));
            using SKBitmap premultiplied = SKBitmap.Decode(encoded, info.WithColorType(SKColorType.Bgra8888).WithAlphaType(SKAlphaType.Premul));
            using SKImage rasterStraight = SKImage.FromBitmap(straight);
            using SKImage rasterPremultiplied = SKImage.FromBitmap(premultiplied);
            using SKImage lazy = SKImage.FromEncodedData(encoded);

            Sources sources = new(file, straight, premultiplied, rasterStraight, rasterPremultiplied, lazy);
            ToImageInto(expected.ConvertTo<ColorRGB>(), sources);
            ToImageInto(expected.ConvertTo<ColorBGR>(), sources);
            ToImageInto(expected.ConvertTo<ColorRGBA>(), sources);
            ToImageInto(expected, sources);
            ToImageInto(expected.ConvertTo<ColorARGB>(), sources);
            ToImageInto(expected.ConvertTo<ColorABGR>(), sources);
        }
    }

    private sealed record Sources(string File, SKBitmap Straight, SKBitmap Premultiplied, SKImage RasterStraight, SKImage RasterPremultiplied, SKImage Lazy);

    private static void ToImageInto<T>(Image<T> expected, Sources sources)
        where T : unmanaged, IColor<T>
    {
        string name = $"{T.ColorFormat.Name}: {sources.File}";

        Assert.IsTrue(expected.ContentEquals(sources.Straight.ToImage<T>()), $"{name} (bitmap)");
        Assert.IsTrue(expected.ContentEquals(sources.RasterStraight.ToImage<T>()), $"{name} (raster image)");

        // Premultiplying loses precision, so these can be off by 1.
        AssertSimilar(expected, sources.Premultiplied.ToImage<T>(), $"{name} (premultiplied bitmap)");
        AssertSimilar(expected, sources.RasterPremultiplied.ToImage<T>(), $"{name} (premultiplied raster image)");
        AssertSimilar(expected, sources.Lazy.ToImage<T>(), $"{name} (encoded image)");
    }

    private static void AssertSimilar<T>(Image<T> expected, Image<T> actual, string message)
        where T : unmanaged, IColor<T>
    {
        Assert.AreEqual(expected.Width, actual.Width, message);
        Assert.AreEqual(expected.Height, actual.Height, message);

        byte[] expectedBytes = expected.ToRawArray();
        byte[] actualBytes = actual.ToRawArray();
        for (int i = 0; i < expectedBytes.Length; i++)
            if (Math.Abs(expectedBytes[i] - actualBytes[i]) > 1)
                Assert.Fail($"{message}: byte {i} is {actualBytes[i]}, expected {expectedBytes[i]}");
    }

    [TestMethod]
    public void ToImageOfEmptyBitmap()
    {
        using SKBitmap bitmap = new();
        Image<ColorBGRA> image = bitmap.ToImage();

        Assert.AreEqual(0, image.Width);
        Assert.AreEqual(0, image.Height);
    }

    [TestMethod]
    public void ToSKBitmapOfRegion()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(40, 30);
        IImage region = ((IImage)image).Slice(5, 3, 21, 17);

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
    public void ExportFromEveryFormat()
    {
        ExportFrom<ColorRGB>();
        ExportFrom<ColorBGR>();
        ExportFrom<ColorRGBA>();
        ExportFrom<ColorBGRA>();
        ExportFrom<ColorARGB>();
        ExportFrom<ColorABGR>();
    }

    private static void ExportFrom<T>()
        where T : unmanaged, IColor<T>
    {
        // G and A of the test data are the high bytes of x and y, so the region starts beyond them to give every channel its own value.
        Image<T> image = TestDataHelper.CreateTestImage<T>(300, 560).Slice(260, 520, 21, 17);
        IImage asIImage = image;

        AssertExported(image, image.ToSKBitmap(), image.ToSKImage(), image.ToPng(), "Image<T>");
        AssertExported(image, image.AsRefImage().ToSKBitmap(), image.AsRefImage().ToSKImage(), image.AsRefImage().ToPng(), "RefImage<T>");
        AssertExported(image, asIImage.ToSKBitmap(), asIImage.ToSKImage(), asIImage.ToPng(), "IImage");
    }

    private static void AssertExported<T>(Image<T> expected, SKBitmap bitmap, SKImage skImage, byte[] png, string source)
        where T : unmanaged, IColor<T>
    {
        string name = $"{T.ColorFormat.Name} from {source}";

        using (bitmap)
        {
            Assert.AreEqual(typeof(T) == typeof(ColorRGBA) ? SKColorType.Rgba8888 : SKColorType.Bgra8888, bitmap.ColorType, name);
            Assert.IsTrue(expected.ContentEquals(bitmap.ToImage<T>()), $"{name}: ToSKBitmap");
        }

        using (skImage)
            Assert.IsTrue(expected.ContentEquals(skImage.ToImage<T>()), $"{name}: ToSKImage");

        using MemoryStream stream = new(png);
        Assert.IsTrue(expected.ContentEquals(SkiaSharp.ImageHelper.LoadImage<T>(stream)), $"{name}: ToPng");
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
        Image<T> expected = SkiaSharp.ImageHelper.LoadImage(image).ConvertTo<T>();

        Assert.IsTrue(expected.ContentEquals(SkiaSharp.ImageHelper.LoadImage<T>(image)), $"{T.ColorFormat.Name}: {image}");

        using FileStream stream = File.OpenRead(image);
        Assert.IsTrue(expected.ContentEquals(SkiaSharp.ImageHelper.LoadImage<T>(stream)), $"{T.ColorFormat.Name}: {image} (stream)");
    }
}