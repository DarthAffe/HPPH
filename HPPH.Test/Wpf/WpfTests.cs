using System.Windows.Media;
using System.Windows.Media.Imaging;
using HPPH.WPF;

namespace HPPH.Test.Wpf;

[TestClass]
public class WpfTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void ImageConversion24Bit()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(1920, 1080);
        BitmapSource bitmap = image.ToBitmapSource();
        IImage image2 = bitmap.ToImage();

        Assert.AreEqual(PixelFormats.Rgb24, bitmap.Format);
        Assert.AreEqual(IColorFormat.RGB, image2.ColorFormat);
        Assert.IsTrue(image.ContentEquals(image2));
    }

    [TestMethod]
    public void ImageConversion32Bit()
    {
        Image<ColorRGBA> image = TestDataHelper.CreateTestImage<ColorRGBA>(1920, 1080);
        BitmapSource bitmap = image.ToBitmapSource();
        IImage image2 = bitmap.ToImage();

        Assert.AreEqual(PixelFormats.Bgra32, bitmap.Format);
        Assert.AreEqual(IColorFormat.BGRA, image2.ColorFormat);

        image2 = image2.ConvertTo<ColorRGBA>();

        Assert.IsTrue(image.ContentEquals(image2));
    }

    [TestMethod]
    public void LoadFileToPngLoadStream()
    {
        foreach (string image in GetTestImages())
        {
            IImage img = WPF.ImageHelper.LoadImage(image);
            byte[] png = img.ToPng();
            using MemoryStream ms = new(png);
            IImage img2 = WPF.ImageHelper.LoadImage(ms);

            Assert.IsTrue(img.ContentEquals(img2), image);
        }
    }

    [TestMethod]
    public void LoadImageMatchesSystemDrawing()
    {
        foreach (string image in GetTestImages())
        {
            Image<ColorRGBA> expected = System.Drawing.ImageHelper.LoadImage<ColorRGBA>(image);

            Assert.IsTrue(expected.ContentEquals(WPF.ImageHelper.LoadImage<ColorRGBA>(image)), image);
        }
    }

    [TestMethod]
    public void ToImageAcceptsOtherPixelFormats()
    {
        const int WIDTH = 7;
        const int HEIGHT = 5;

        ColorRGBA[] expected = new ColorRGBA[WIDTH * HEIGHT];
        byte[] bgr32 = new byte[WIDTH * HEIGHT * 4];
        byte[] pbgra32 = new byte[WIDTH * HEIGHT * 4];
        byte[] gray8 = new byte[WIDTH * HEIGHT];
        for (int i = 0; i < expected.Length; i++)
        {
            byte r = (byte)((i % WIDTH) * 32);
            byte g = (byte)((i / WIDTH) * 48);
            expected[i] = new ColorRGBA(r, g, 200, 255);

            bgr32[(i * 4) + 0] = 200;
            bgr32[(i * 4) + 1] = g;
            bgr32[(i * 4) + 2] = r;
            bgr32[(i * 4) + 3] = 0;

            pbgra32[(i * 4) + 0] = 200;
            pbgra32[(i * 4) + 1] = g;
            pbgra32[(i * 4) + 2] = r;
            pbgra32[(i * 4) + 3] = 255;

            gray8[i] = r;
        }

        AssertToImage(Image<ColorRGBA>.Create(expected, WIDTH, HEIGHT), BitmapSource.Create(WIDTH, HEIGHT, 96, 96, PixelFormats.Bgr32, null, bgr32, WIDTH * 4), "Bgr32");
        AssertToImage(Image<ColorRGBA>.Create(expected, WIDTH, HEIGHT), BitmapSource.Create(WIDTH, HEIGHT, 96, 96, PixelFormats.Pbgra32, null, pbgra32, WIDTH * 4), "Pbgra32");

        ColorRGBA[] expectedGray = expected.Select(x => new ColorRGBA(x.R, x.R, x.R, 255)).ToArray();
        AssertToImage(Image<ColorRGBA>.Create(expectedGray, WIDTH, HEIGHT), BitmapSource.Create(WIDTH, HEIGHT, 96, 96, PixelFormats.Gray8, null, gray8, WIDTH), "Gray8");
    }

    private static void AssertToImage(Image<ColorRGBA> expected, BitmapSource bitmap, string name)
    {
        IImage image = bitmap.ToImage();

        Assert.AreEqual(IColorFormat.BGRA, image.ColorFormat, name);
        Assert.IsTrue(expected.ContentEquals(image.ConvertTo<ColorRGBA>()), name);
        Assert.IsTrue(expected.ContentEquals(bitmap.ToImage<ColorRGBA>()), $"{name}: ToImage<T>");
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

        AssertExported(image, image.ToBitmapSource(), image.ToWriteableBitmap(), image.ToPng(), "Image<T>");
        AssertExported(image, image.AsRefImage().ToBitmapSource(), image.AsRefImage().ToWriteableBitmap(), image.AsRefImage().ToPng(), "RefImage<T>");
        AssertExported(image, asIImage.ToBitmapSource(), asIImage.ToWriteableBitmap(), asIImage.ToPng(), "IImage");
    }

    private static void AssertExported<T>(Image<T> expected, BitmapSource bitmap, WriteableBitmap writeable, byte[] png, string source)
        where T : unmanaged, IColor<T>
    {
        string name = $"{T.ColorFormat.Name} from {source}";

        PixelFormat expectedFormat = typeof(T) == typeof(ColorRGB) ? PixelFormats.Rgb24
                                   : typeof(T) == typeof(ColorBGR) ? PixelFormats.Bgr24
                                   : PixelFormats.Bgra32;

        Assert.AreEqual(expectedFormat, bitmap.Format, name);
        Assert.IsTrue(bitmap.IsFrozen, $"{name}: frozen");
        Assert.IsTrue(expected.ContentEquals(bitmap.ToImage<T>()), $"{name}: ToBitmapSource");

        Assert.AreEqual(expectedFormat, writeable.Format, name);
        Assert.IsFalse(writeable.IsFrozen, $"{name}: writeable not frozen");
        Assert.IsTrue(expected.ContentEquals(writeable.ToImage<T>()), $"{name}: ToWriteableBitmap");

        using MemoryStream stream = new(png);
        Assert.IsTrue(expected.ContentEquals(WPF.ImageHelper.LoadImage<T>(stream)), $"{name}: ToPng");
    }

    [TestMethod]
    public void CopyToEveryPixelFormat()
    {
        // 21 pixels wide, so WPF pads the rows of the 3-byte formats.
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(300, 560).Slice(260, 520, 21, 17);
        IImage asIImage = image;

        foreach (PixelFormat format in new[] { PixelFormats.Bgra32, PixelFormats.Bgr32, PixelFormats.Bgr24, PixelFormats.Rgb24 })
        {
            WriteableBitmap fromImage = new(image.Width, image.Height, 96, 96, format, null);
            image.CopyTo(fromImage);
            Assert.IsTrue(image.ContentEquals(fromImage.ToImage<ColorRGB>()), $"{format} from Image<T>");

            WriteableBitmap fromRefImage = new(image.Width, image.Height, 96, 96, format, null);
            image.AsRefImage().CopyTo(fromRefImage);
            Assert.IsTrue(image.ContentEquals(fromRefImage.ToImage<ColorRGB>()), $"{format} from RefImage<T>");

            WriteableBitmap fromIImage = new(image.Width, image.Height, 96, 96, format, null);
            asIImage.CopyTo(fromIImage);
            Assert.IsTrue(image.ContentEquals(fromIImage.ToImage<ColorRGB>()), $"{format} from IImage");
        }
    }

    [TestMethod]
    public void CopyToKeepsAlpha()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(300, 560).Slice(260, 520, 21, 17);

        WriteableBitmap bitmap = new(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null);
        image.CopyTo(bitmap);

        Assert.IsTrue(image.ContentEquals(bitmap.ToImage<ColorARGB>()));
    }

    [TestMethod]
    public void CopyToValidatesBitmap()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 8);

        Assert.ThrowsExactly<ArgumentNullException>(() => image.CopyTo((WriteableBitmap)null!));
        Assert.ThrowsExactly<ArgumentException>(() => image.CopyTo(new WriteableBitmap(10, 9, 96, 96, PixelFormats.Bgra32, null)));
        Assert.ThrowsExactly<ArgumentException>(() => image.CopyTo(new WriteableBitmap(11, 8, 96, 96, PixelFormats.Bgra32, null)));
        Assert.ThrowsExactly<NotSupportedException>(() => image.CopyTo(new WriteableBitmap(10, 8, 96, 96, PixelFormats.Pbgra32, null)));
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
        Image<T> expected = WPF.ImageHelper.LoadImage(image).ConvertTo<T>();

        Assert.IsTrue(expected.ContentEquals(WPF.ImageHelper.LoadImage<T>(image)), $"{T.ColorFormat.Name}: {image}");

        using FileStream stream = File.OpenRead(image);
        Assert.IsTrue(expected.ContentEquals(WPF.ImageHelper.LoadImage<T>(stream)), $"{T.ColorFormat.Name}: {image} (stream)");
    }
}
