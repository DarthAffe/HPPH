using HPPH.Reference;
using HPPH.Test.Infrastructure;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class TransformFuzzTests
{
    private const int ITERATIONS = 100;

    [TestMethod]
    public void FlipOfRandomImages()
    {
        Random random = new(4711);

        CheckFlip<ColorRGB>(random);
        CheckFlip<ColorBGR>(random);
        CheckFlip<ColorRGBA>(random);
        CheckFlip<ColorARGB>(random);
    }

    private static void CheckFlip<T>(Random random)
        where T : unmanaged, IColor<T>
    {
        for (int iteration = 0; iteration < ITERATIONS; iteration++)
        {
            Image<T> image = CreateRandomImage<T>(random, out string name);
            ImageFlip flip = (ImageFlip)random.Next(4);

            Image<T> expected = ReferencePixelHelper.Flip(image.AsRefImage(), flip);
            byte[] destination = CreateRandomDestination<T>(random, expected.Width, expected.Height, out int stride);
            image.Flip(flip, destination, stride);

            TestImages.AssertStrided(expected, destination, stride, $"{name}, {flip}, destination stride {stride}");
        }
    }

    private static Image<T> CreateRandomImage<T>(Random random, out string name)
        where T : unmanaged, IColor<T>
    {
        int width = random.Next(1, 301);
        int height = random.Next(1, 301);
        int padding = random.Next(0, 3) == 0 ? 0 : random.Next(1, 70);

        Image<T> image = TestImages.Create<T>(random, width, height, padding);
        name = $"{T.ColorFormat.Name} {width}x{height}, padding {padding}";

        if (random.Next(2) == 0) return image;

        int x = random.Next(0, width);
        int y = random.Next(0, height);
        int regionWidth = random.Next(1, (width - x) + 1);
        int regionHeight = random.Next(1, (height - y) + 1);
        name += $", region {x},{y} {regionWidth}x{regionHeight}";

        return image.Slice(x, y, regionWidth, regionHeight);
    }

    private static byte[] CreateRandomDestination<T>(Random random, int width, int height, out int stride)
        where T : unmanaged, IColor<T>
    {
        int rowSize = width * T.ColorFormat.BytesPerPixel;
        stride = rowSize + (random.Next(0, 3) == 0 ? 0 : random.Next(1, 40));

        return TestImages.CreateSentinelBuffer(((height - 1) * stride) + rowSize + random.Next(0, 5));
    }
}
