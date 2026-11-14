using System.Runtime.InteropServices;
using HPPH.Reference;
using HPPH.Test.Infrastructure;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class DownscaleTests
{
    private static readonly (int X, int Y)[] FACTORS = [(1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (8, 8), (16, 16), (32, 32), (2, 3), (4, 1), (1, 4)];

    [TestMethod]
    public void DownscaleMatchesReference()
    {
        DownscaleMatchesReference<ColorRGB>();
        DownscaleMatchesReference<ColorBGR>();
        DownscaleMatchesReference<ColorRGBA>();
        DownscaleMatchesReference<ColorBGRA>();
        DownscaleMatchesReference<ColorARGB>();
        DownscaleMatchesReference<ColorABGR>();
    }

    private static void DownscaleMatchesReference<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((int width, int height) in TestImages.SIZES)
            foreach ((string name, Image<T> image) in TestImages.Variants<T>(width, height, (width * 31) + height))
                foreach ((int factorX, int factorY) in FACTORS)
                    CheckAllOverloads(image, factorX, factorY, $"{T.ColorFormat.Name} {width}x{height} {name}, {factorX}x{factorY}");
    }

    [TestMethod]
    public void DownscaleWideImages()
    {
        // Rows wider than the segments of 4096 bytes the rows are processed in.
        DownscaleWideImages<ColorRGB>();
        DownscaleWideImages<ColorBGRA>();
    }

    private static void DownscaleWideImages<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((int factorX, int factorY) in (ReadOnlySpan<(int, int)>)[(2, 2), (3, 3), (4, 4), (5, 2), (6, 6), (8, 8), (1, 3), (7, 1), (9, 3), (10, 2), (12, 12), (16, 16), (20, 20), (37, 5)])
            foreach ((string name, Image<T> image) in TestImages.Variants<T>(1503, 41, factorX))
                CheckAllOverloads(image, factorX, factorY, $"{T.ColorFormat.Name} 1503x41 {name}, {factorX}x{factorY}");
    }

    [TestMethod]
    public void DownscaleInSegmentsAndBands()
    {
        DownscaleInSegmentsAndBands<ColorRGB>();
        DownscaleInSegmentsAndBands<ColorBGRA>();
    }

    private static void DownscaleInSegmentsAndBands<T>()
        where T : unmanaged, IColor<T>
    {
        // Wide blocks are processed in segments of 256 output pixels and bands of up to 8 rows: 262 pixels leave a segment of 6 pixels.
        foreach ((int width, int height, int factorX, int factorY) in (ReadOnlySpan<(int, int, int, int)>)[(1310, 40, 5, 17), (2096, 70, 8, 33), (1100, 20, 9, 9),
                                                                                                             (300, 300, 2, 100), (300, 300, 4, 128), (40, 300, 1, 257)])
            foreach ((string name, Image<T> image) in TestImages.Variants<T>(width, height, factorX + factorY))
                CheckAllOverloads(image, factorX, factorY, $"{T.ColorFormat.Name} {width}x{height} {name}, {factorX}x{factorY}");
    }

    [TestMethod]
    public void DownscaleFullHD()
    {
        foreach ((string name, Image<ColorBGRA> image) in TestImages.Variants<ColorBGRA>(1920, 1080, 1))
            foreach (int factor in (int[])[2, 3, 4, 8])
                CheckAllOverloads(image, factor, factor, $"1920x1080 {name}, {factor}");
    }

    [TestMethod]
    public void DownscaleLargeBlocks()
    {
        DownscaleLargeBlocks<ColorRGB>();
        DownscaleLargeBlocks<ColorBGRA>();
    }

    private static void DownscaleLargeBlocks<T>()
        where T : unmanaged, IColor<T>
    {
        // Blocks of more than 257 pixels, more than 257 rows, wider than 4096 bytes, and with counts that are too large to divide in vectors.
        foreach ((int width, int height, int factorX, int factorY) in (ReadOnlySpan<(int, int, int, int)>)[(600, 600, 256, 256), (300, 50, 17, 17), (40, 700, 2, 300),
                                                                                                           (2500, 5, 1200, 2), (700, 700, 300, 300), (1500, 3, 1500, 1),
                                                                                                           (1040, 130, 129, 129), (2100, 260, 260, 260)])
            foreach ((string name, Image<T> image) in TestImages.Variants<T>(width, height, factorY))
                CheckAllOverloads(image, factorX, factorY, $"{T.ColorFormat.Name} {width}x{height} {name}, {factorX}x{factorY}");
    }

    private static void CheckAllOverloads<T>(Image<T> image, int factorX, int factorY, string name)
        where T : unmanaged, IColor<T>
    {
        Image<T> expected = ReferencePixelHelper.Downscale(image.AsRefImage(), factorX, factorY);
        int pixels = expected.Width * expected.Height;
        int rowSize = expected.Width * T.ColorFormat.BytesPerPixel;

        TestImages.AssertEqual(expected, image.Downscale(factorX, factorY), $"{name}: Image");
        TestImages.AssertEqual(expected, image.AsRefImage().Downscale(factorX, factorY), $"{name}: RefImage");

        IImage fromInterface = ((IImage)image).Downscale(factorX, factorY);
        Assert.IsInstanceOfType<Image<T>>(fromInterface, $"{name}: IImage");
        TestImages.AssertEqual(expected, (Image<T>)fromInterface, $"{name}: IImage");

        T[] span = TestImages.CreateSentinelPixels<T>(pixels);
        image.Downscale(factorX, factorY, span.AsSpan());
        TestImages.AssertCompact(expected, span, $"{name}: Image, Span<T>");

        T[] refSpan = TestImages.CreateSentinelPixels<T>(pixels);
        image.AsRefImage().Downscale(factorX, factorY, refSpan.AsSpan());
        TestImages.AssertCompact(expected, refSpan, $"{name}: RefImage, Span<T>");

        foreach (int padding in (int[])[0, 5])
        {
            int stride = rowSize + padding;
            int length = (Math.Max(0, expected.Height - 1) * stride) + rowSize + 7;

            byte[] fromImage = TestImages.CreateSentinelBuffer(length);
            image.Downscale(factorX, factorY, fromImage, stride);
            TestImages.AssertStrided(expected, fromImage, stride, $"{name}, stride {stride}: Image");

            byte[] fromRefImage = TestImages.CreateSentinelBuffer(length);
            image.AsRefImage().Downscale(factorX, factorY, fromRefImage, stride);
            TestImages.AssertStrided(expected, fromRefImage, stride, $"{name}, stride {stride}: RefImage");

            byte[] fromIImage = TestImages.CreateSentinelBuffer(length);
            ((IImage)image).Downscale(factorX, factorY, fromIImage, stride);
            TestImages.AssertStrided(expected, fromIImage, stride, $"{name}, stride {stride}: IImage");
        }

        if (factorX != factorY) return;

        TestImages.AssertEqual(expected, image.Downscale(factorX), $"{name}: Image, one factor");
        TestImages.AssertEqual(expected, image.AsRefImage().Downscale(factorX), $"{name}: RefImage, one factor");
        TestImages.AssertEqual(expected, (Image<T>)((IImage)image).Downscale(factorX), $"{name}: IImage, one factor");

        T[] singleSpan = TestImages.CreateSentinelPixels<T>(pixels);
        image.Downscale(factorX, singleSpan.AsSpan());
        TestImages.AssertCompact(expected, singleSpan, $"{name}: Image, Span<T>, one factor");

        T[] singleRefSpan = TestImages.CreateSentinelPixels<T>(pixels);
        image.AsRefImage().Downscale(factorX, singleRefSpan.AsSpan());
        TestImages.AssertCompact(expected, singleRefSpan, $"{name}: RefImage, Span<T>, one factor");

        int singleStride = rowSize + 3;
        int singleLength = (Math.Max(0, expected.Height - 1) * singleStride) + rowSize;

        byte[] singleImage = TestImages.CreateSentinelBuffer(singleLength);
        image.Downscale(factorX, singleImage, singleStride);
        TestImages.AssertStrided(expected, singleImage, singleStride, $"{name}: Image, stride, one factor");

        byte[] singleRefImage = TestImages.CreateSentinelBuffer(singleLength);
        image.AsRefImage().Downscale(factorX, singleRefImage, singleStride);
        TestImages.AssertStrided(expected, singleRefImage, singleStride, $"{name}: RefImage, stride, one factor");

        byte[] singleIImage = TestImages.CreateSentinelBuffer(singleLength);
        ((IImage)image).Downscale(factorX, singleIImage, singleStride);
        TestImages.AssertStrided(expected, singleIImage, singleStride, $"{name}: IImage, stride, one factor");
    }

    [TestMethod]
    public void DownscaleRoundsTiesToEven()
    {
        // 0, 0, 1, 1 averages to 0.5 and 1, 1, 2, 2 to 1.5: both round to the even value.
        Image<ColorRGB> low = Image<ColorRGB>.Create([new ColorRGB(0, 1, 0), new ColorRGB(0, 1, 0), new ColorRGB(1, 2, 0), new ColorRGB(1, 2, 0)], 2, 2);
        Assert.AreEqual(new ColorRGB(0, 2, 0), low.Downscale(2)[0, 0]);

        // A block of 6 pixels, so the sums aren't divided by a power of two.
        Image<ColorBGRA> six = Image<ColorBGRA>.Create([new ColorBGRA(0, 1, 0, 0), new ColorBGRA(0, 1, 0, 0), new ColorBGRA(0, 1, 0, 0),
                                                        new ColorBGRA(1, 2, 0, 0), new ColorBGRA(1, 2, 0, 0), new ColorBGRA(1, 2, 0, 0)], 6, 1);
        Assert.AreEqual(new ColorBGRA(0, 2, 0, 0), six.Downscale(6, 1)[0, 0]);
    }

    [TestMethod]
    public void DownscaleRoundsTiesToEvenInWideRows()
    {
        // Only two values per channel, so many blocks are ties. The rows are wide enough for every vectorized path.
        DownscaleRoundsTiesToEvenInWideRows<ColorRGB>();
        DownscaleRoundsTiesToEvenInWideRows<ColorBGRA>();
    }

    private static void DownscaleRoundsTiesToEvenInWideRows<T>()
        where T : unmanaged, IColor<T>
    {
        Random random = new(42);
        foreach (byte value in (byte[])[0, 127, 254])
        {
            const int width = 287;
            const int height = 41;

            byte[] data = new byte[width * height * T.ColorFormat.BytesPerPixel];
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(value + random.Next(2));

            Image<T> image = Image<T>.Wrap(data, width, height, width * T.ColorFormat.BytesPerPixel);
            foreach ((int factorX, int factorY) in (ReadOnlySpan<(int, int)>)[(2, 1), (1, 2), (2, 2), (2, 3), (3, 2), (4, 4), (6, 1), (5, 2), (16, 16), (20, 2)])
                TestImages.AssertEqual(ReferencePixelHelper.Downscale(image.AsRefImage(), factorX, factorY), image.Downscale(factorX, factorY), $"{T.ColorFormat.Name} {value}, {factorX}x{factorY}");
        }
    }

    [TestMethod]
    public void DownscaleToOnePixelIsAverage()
    {
        DownscaleToOnePixelIsAverage<ColorRGB>();
        DownscaleToOnePixelIsAverage<ColorBGR>();
        DownscaleToOnePixelIsAverage<ColorRGBA>();
        DownscaleToOnePixelIsAverage<ColorBGRA>();
        DownscaleToOnePixelIsAverage<ColorARGB>();
        DownscaleToOnePixelIsAverage<ColorABGR>();
    }

    private static void DownscaleToOnePixelIsAverage<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((int width, int height) in (ReadOnlySpan<(int, int)>)[(1, 1), (7, 13), (64, 36), (300, 2), (2, 300), (400, 300)])
            foreach ((string name, Image<T> image) in TestImages.Variants<T>(width, height, width + height))
            {
                Image<T> downscaled = image.Downscale(int.MaxValue);
                Assert.AreEqual(1, downscaled.Width, $"{T.ColorFormat.Name} {width}x{height} {name}: width");
                Assert.AreEqual(1, downscaled.Height, $"{T.ColorFormat.Name} {width}x{height} {name}: height");
                Assert.AreEqual(image.Average(), downscaled[0, 0], $"{T.ColorFormat.Name} {width}x{height} {name}");
            }
    }

    [TestMethod]
    public void DownscaleKeepsSingleColors()
    {
        foreach (int factor in (int[])[2, 3, 7, 16, 300])
        {
            ColorBGRA color = new(13, 200, 255, 77);
            Image<ColorBGRA> image = Image<ColorBGRA>.Create(Enumerable.Repeat(color, 600 * 500).ToArray(), 600, 500);

            Image<ColorBGRA> downscaled = image.Downscale(factor);
            for (int y = 0; y < downscaled.Height; y++)
                for (int x = 0; x < downscaled.Width; x++)
                    Assert.AreEqual(color, downscaled[x, y], $"factor {factor} at ({x}, {y})");
        }
    }

    [TestMethod]
    public void DownscaleOfLargeSums()
    {
        // 4096 * 4096 * 255 is just below 2^32, 4200 * 4200 * 255 above.
        Assert.AreEqual(new ColorBGRA(255, 255, 255, 255), CreateWhite<ColorBGRA>(4096, 4096).Downscale(4096)[0, 0]);
        Assert.AreEqual(new ColorRGB(255, 255, 255), CreateWhite<ColorRGB>(4200, 4200).Downscale(4200)[0, 0]);

        // The largest blocks that are added up row by row: 257 rows of 4096 bytes.
        Assert.AreEqual(new ColorBGRA(255, 255, 255, 255), CreateWhite<ColorBGRA>(1024, 257).Downscale(1024, 257)[0, 0]);
    }

    private static Image<T> CreateWhite<T>(int width, int height)
        where T : unmanaged, IColor<T>
    {
        byte[] data = new byte[width * height * T.ColorFormat.BytesPerPixel];
        data.AsSpan().Fill(255);
        return Image<T>.Wrap(data, width, height, width * T.ColorFormat.BytesPerPixel);
    }

    [TestMethod]
    public void DownscaleOfEmptyImages()
    {
        foreach ((int width, int height) in new[] { (0, 0), (0, 5), (5, 0) })
            foreach (int factor in (int[])[1, 2, 7])
            {
                Image<ColorRGB> image = Image<ColorRGB>.Create(new ColorRGB[width * height], width, height);
                (int downscaledWidth, int downscaledHeight) = HPPH.PixelHelper.GetDownscaledSize(width, height, factor);
                string name = $"{width}x{height}, {factor}";

                Assert.AreEqual(width == 0 ? 0 : Math.Max(1, width / factor), downscaledWidth, name);
                Assert.AreEqual(height == 0 ? 0 : Math.Max(1, height / factor), downscaledHeight, name);

                Image<ColorRGB> downscaled = image.Downscale(factor);
                Assert.AreEqual(downscaledWidth, downscaled.Width, name);
                Assert.AreEqual(downscaledHeight, downscaled.Height, name);

                IImage fromInterface = ((IImage)image).Downscale(factor);
                Assert.AreEqual(downscaledWidth, fromInterface.Width, name);
                Assert.AreEqual(downscaledHeight, fromInterface.Height, name);

                image.Downscale(factor, Span<ColorRGB>.Empty);
                image.Downscale(factor, Span<byte>.Empty, downscaledWidth * 3);
                ((IImage)image).Downscale(factor, Span<byte>.Empty, downscaledWidth * 3);
            }
    }

    [TestMethod]
    public void GetDownscaledSize()
    {
        Assert.AreEqual((960, 540), HPPH.PixelHelper.GetDownscaledSize(1920, 1080, 2));
        Assert.AreEqual((640, 360), HPPH.PixelHelper.GetDownscaledSize(1920, 1080, 3));
        Assert.AreEqual((274, 154), HPPH.PixelHelper.GetDownscaledSize(1920, 1080, 7));
        Assert.AreEqual((1, 1), HPPH.PixelHelper.GetDownscaledSize(1920, 1080, 4000));
        Assert.AreEqual((1, 1080), HPPH.PixelHelper.GetDownscaledSize(1920, 1080, int.MaxValue, 1));
        Assert.AreEqual((960, 1080), HPPH.PixelHelper.GetDownscaledSize(1920, 1080, 2, 1));
        Assert.AreEqual((0, 2), HPPH.PixelHelper.GetDownscaledSize(0, 5, 2));
        Assert.AreEqual((1, 0), HPPH.PixelHelper.GetDownscaledSize(5, 0, 7));
    }

    [TestMethod]
    public void DownscaleRejectsInvalidFactors()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(7, 5);
        byte[] buffer = new byte[7 * 5 * 3];

        foreach ((int factorX, int factorY) in (ReadOnlySpan<(int, int)>)[(0, 1), (1, 0), (-1, 2), (2, int.MinValue)])
        {
            string name = $"{factorX}x{factorY}";
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Downscale(factorX, factorY), $"{name}: Image");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.AsRefImage().Downscale(factorX, factorY), $"{name}: RefImage");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((IImage)image).Downscale(factorX, factorY), $"{name}: IImage");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Downscale(factorX, factorY, new ColorRGB[35].AsSpan()), $"{name}: Span<T>");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Downscale(factorX, factorY, buffer, 21), $"{name}: stride");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((IImage)image).Downscale(factorX, factorY, buffer, 21), $"{name}: IImage stride");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => HPPH.PixelHelper.GetDownscaledSize(7, 5, factorX, factorY), $"{name}: GetDownscaledSize");
        }

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Downscale(0), "one factor");
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => HPPH.PixelHelper.GetDownscaledSize(-1, 5, 2), "negative width");
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => HPPH.PixelHelper.GetDownscaledSize(5, -1, 2), "negative height");
    }

    [TestMethod]
    public void DownscaleRejectsNullImages()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Image<ColorRGB>)null!).Downscale(2));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Image<ColorRGB>)null!).Downscale(2, 3));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Image<ColorRGB>)null!).Downscale(2, new ColorRGB[1].AsSpan()));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Image<ColorRGB>)null!).Downscale(2, new byte[3], 3));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IImage)null!).Downscale(2));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IImage)null!).Downscale(2, 3));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IImage)null!).Downscale(2, new byte[3], 3));
    }

    [TestMethod]
    public void DownscaleRejectsSmallDestinations()
    {
        // 15x10 downscaled by 2 is 7x5: rows of 21 bytes, the buffer needs 4 * stride + 21 bytes.
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(15, 10);

        Assert.ThrowsExactly<ArgumentException>(() => image.Downscale(2, new ColorRGB[34].AsSpan()), "Span<T>");
        Assert.ThrowsExactly<ArgumentException>(() => image.AsRefImage().Downscale(2, new ColorRGB[34].AsSpan()), "RefImage, Span<T>");

        Assert.ThrowsExactly<ArgumentException>(() => image.Downscale(2, new byte[200], 20), "stride too small");
        Assert.ThrowsExactly<ArgumentException>(() => image.Downscale(2, new byte[(4 * 24) + 20], 24), "buffer too short");
        Assert.ThrowsExactly<ArgumentException>(() => ((IImage)image).Downscale(2, new byte[(4 * 24) + 20], 24), "IImage, buffer too short");
        image.Downscale(2, new byte[(4 * 24) + 21], 24);
        image.Downscale(2, new ColorRGB[35].AsSpan());
    }

    [TestMethod]
    public void DownscaleRejectsOverlappingDestinations()
    {
        // The image covers bytes 0 to 4 * 24 + 21 of the buffer, rows are 24 bytes apart.
        byte[] buffer = new byte[400];
        Image<ColorRGB> image = Image<ColorRGB>.Wrap(buffer, 7, 5, 24);
        const int covered = (4 * 24) + 21;

        Assert.ThrowsExactly<ArgumentException>(() => image.Downscale(2, buffer, 9), "same buffer");
        Assert.ThrowsExactly<ArgumentException>(() => image.Downscale(2, buffer.AsSpan(covered - 1), 9), "overlaps the last byte");
        Assert.ThrowsExactly<ArgumentException>(() => image.Downscale(2, buffer.AsSpan(22), 9), "starts in the padding");
        Assert.ThrowsExactly<ArgumentException>(() => image.Downscale(2, MemoryMarshal.Cast<byte, ColorRGB>(buffer.AsSpan(3))), "Span<T>");
        Assert.ThrowsExactly<ArgumentException>(() => ((IImage)image).Downscale(2, buffer, 9), "IImage");

        // Directly after the image is fine.
        image.Downscale(2, buffer.AsSpan(covered), 9);
        image.Downscale(2, MemoryMarshal.Cast<byte, ColorRGB>(buffer.AsSpan(covered)));
    }
}
