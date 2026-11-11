using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class ReductionFuzzTests
{
    private const int ITERATIONS = 150;

    [TestMethod]
    public void ReductionsOfRandomImagesAndRegions()
    {
        Random random = new(815);

        Check<ColorRGB>(random);
        Check<ColorBGR>(random);
        Check<ColorRGBA>(random);
        Check<ColorARGB>(random);
    }

    [TestMethod]
    public void SumOfSaturatedPixelsDoesNotOverflow()
    {
        // The 4-byte Sum collects 16-bit sums and widens them every 256 vectors (of 16, 32 or 64 bytes);
        // all-255 data around those boundaries and far beyond them must not wrap.
        foreach (int length in new[] { 1023, 1024, 1025, 2047, 2048, 2049, 4095, 4096, 4097, 1_000_003 })
        {
            ColorRGBA[] colors = new ColorRGBA[length];
            colors.AsSpan().Fill(new ColorRGBA(255, 255, 255, 255));

            long expected = (long)length * 255;
            Assert.AreEqual(new ColorSum(expected, expected, expected, expected), new ReadOnlySpan<ColorRGBA>(colors).Sum(), $"{length} px");
        }

        // An image whose rows each end with a partial vector, many rows long.
        const int WIDTH = 1001;
        const int HEIGHT = 700;
        const int STRIDE = (WIDTH * 4) + 12;
        const long expectedImage = (long)WIDTH * HEIGHT * 255;

        byte[] buffer = new byte[STRIDE * HEIGHT];
        buffer.AsSpan().Fill(255);
        Assert.AreEqual(new ColorSum(expectedImage, expectedImage, expectedImage, expectedImage), Image<ColorBGRA>.Wrap(buffer, WIDTH, HEIGHT, STRIDE).Sum(), "image");
    }

    private static void Check<T>(Random random)
        where T : unmanaged, IColor
    {
        int bpp = T.ColorFormat.BytesPerPixel;

        for (int iteration = 0; iteration < ITERATIONS; iteration++)
        {
            // Mostly narrow images, some wide ones.
            int width = iteration % 5 == 0 ? random.Next(1, 700) : random.Next(1, 120);
            int height = random.Next(1, 12);
            int padding = random.Next(0, 3) == 0 ? 0 : random.Next(1, 70);
            int stride = (width * bpp) + padding;

            byte[] buffer = new byte[((height - 1) * stride) + (width * bpp)];
            random.NextBytes(buffer);

            // Narrow value ranges in some images, so min and max aren't always 0 and 255.
            if (random.Next(2) == 0)
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = (byte)(100 + (buffer[i] % 50));

            Image<T> image = Image<T>.Wrap(buffer, width, height, stride);

            int x = random.Next(0, width);
            int y = random.Next(0, height);
            RefImage<T> region = image.AsRefImage()[x, y, random.Next(1, (width - x) + 1), random.Next(1, (height - y) + 1)];

            string name = $"{T.ColorFormat.Name} {width}x{height} stride {stride}";

            Assert.AreEqual(ReferencePixelHelper.Sum(image.AsRefImage()), image.Sum(), $"{name}: Sum");
            Assert.AreEqual(ReferencePixelHelper.MinMax(image.AsRefImage()), image.MinMax(), $"{name}: MinMax");
            Assert.AreEqual(ReferencePixelHelper.Average(image.AsRefImage()), image.Average(), $"{name}: Average");

            string regionName = $"{name}, region {x},{y} {region.Width}x{region.Height}";
            Assert.AreEqual(ReferencePixelHelper.Sum(region), region.Sum(), $"{regionName}: Sum");
            Assert.AreEqual(ReferencePixelHelper.MinMax(region), region.MinMax(), $"{regionName}: MinMax");
            Assert.AreEqual(ReferencePixelHelper.Average(region), region.Average(), $"{regionName}: Average");

            ReadOnlySpan<T> span = region.ToArray();
            Assert.AreEqual(ReferencePixelHelper.Sum(span), span.Sum(), $"{regionName}: span Sum");
            Assert.AreEqual(ReferencePixelHelper.MinMax(span), span.MinMax(), $"{regionName}: span MinMax");
        }
    }
}
