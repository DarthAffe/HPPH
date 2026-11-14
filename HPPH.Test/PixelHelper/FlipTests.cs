using System.Runtime.InteropServices;
using HPPH.Reference;
using HPPH.Test.Infrastructure;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class FlipTests
{
    private static readonly ImageFlip[] FLIPS = [ImageFlip.None, ImageFlip.Horizontal, ImageFlip.Vertical, ImageFlip.Horizontal | ImageFlip.Vertical];

    [TestMethod]
    public void FlipMatchesReference()
    {
        FlipMatchesReference<ColorRGB>();
        FlipMatchesReference<ColorBGR>();
        FlipMatchesReference<ColorRGBA>();
        FlipMatchesReference<ColorBGRA>();
        FlipMatchesReference<ColorARGB>();
        FlipMatchesReference<ColorABGR>();
    }

    private static void FlipMatchesReference<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((int width, int height) in TestImages.SIZES)
            foreach ((string name, Image<T> image) in TestImages.Variants<T>(width, height, (width * 31) + height))
                foreach (ImageFlip flip in FLIPS)
                    CheckAllOverloads(image, flip, $"{T.ColorFormat.Name} {width}x{height} {name}, {flip}");
    }

    [TestMethod]
    public void FlipFullHD()
    {
        foreach ((string name, Image<ColorBGRA> image) in TestImages.Variants<ColorBGRA>(1920, 1080, 1))
            foreach (ImageFlip flip in FLIPS)
                CheckAllOverloads(image, flip, $"1920x1080 {name}, {flip}");
    }

    private static void CheckAllOverloads<T>(Image<T> image, ImageFlip flip, string name)
        where T : unmanaged, IColor<T>
    {
        Image<T> expected = ReferencePixelHelper.Flip(image.AsRefImage(), flip);
        int pixels = image.Width * image.Height;
        int rowSize = image.Width * T.ColorFormat.BytesPerPixel;

        TestImages.AssertEqual(expected, image.Flip(flip), $"{name}: Image");
        TestImages.AssertEqual(expected, image.AsRefImage().Flip(flip), $"{name}: RefImage");

        IImage fromInterface = ((IImage)image).Flip(flip);
        Assert.IsInstanceOfType<Image<T>>(fromInterface, $"{name}: IImage");
        TestImages.AssertEqual(expected, (Image<T>)fromInterface, $"{name}: IImage");

        T[] span = TestImages.CreateSentinelPixels<T>(pixels);
        image.Flip(flip, span.AsSpan());
        TestImages.AssertCompact(expected, span, $"{name}: Image, Span<T>");

        T[] refSpan = TestImages.CreateSentinelPixels<T>(pixels);
        image.AsRefImage().Flip(flip, refSpan.AsSpan());
        TestImages.AssertCompact(expected, refSpan, $"{name}: RefImage, Span<T>");

        foreach (int padding in (int[])[0, 5])
        {
            int stride = rowSize + padding;
            int length = (Math.Max(0, image.Height - 1) * stride) + rowSize + 7;

            byte[] fromImage = TestImages.CreateSentinelBuffer(length);
            image.Flip(flip, fromImage, stride);
            TestImages.AssertStrided(expected, fromImage, stride, $"{name}, stride {stride}: Image");

            byte[] fromRefImage = TestImages.CreateSentinelBuffer(length);
            image.AsRefImage().Flip(flip, fromRefImage, stride);
            TestImages.AssertStrided(expected, fromRefImage, stride, $"{name}, stride {stride}: RefImage");

            byte[] fromIImage = TestImages.CreateSentinelBuffer(length);
            ((IImage)image).Flip(flip, fromIImage, stride);
            TestImages.AssertStrided(expected, fromIImage, stride, $"{name}, stride {stride}: IImage");
        }
    }

    [TestMethod]
    public void FlipIdentities()
    {
        FlipIdentities<ColorRGB>();
        FlipIdentities<ColorBGRA>();
    }

    private static void FlipIdentities<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((string name, Image<T> image) in TestImages.Variants<T>(31, 17, 3))
        {
            TestImages.AssertEqual(image, image.Flip(ImageFlip.Horizontal).Flip(ImageFlip.Horizontal), $"{name}: horizontal twice");
            TestImages.AssertEqual(image, image.Flip(ImageFlip.Vertical).Flip(ImageFlip.Vertical), $"{name}: vertical twice");
            TestImages.AssertEqual(image, image.Flip(ImageFlip.None), $"{name}: none");

            Image<T> both = image.Flip(ImageFlip.Horizontal | ImageFlip.Vertical);
            TestImages.AssertEqual(image.Flip(ImageFlip.Horizontal).Flip(ImageFlip.Vertical), both, $"{name}: both at once");
            TestImages.AssertEqual(image, both.Flip(ImageFlip.Horizontal | ImageFlip.Vertical), $"{name}: both twice");
        }
    }

    [TestMethod]
    public void FlipOfEmptyImages()
    {
        foreach ((int width, int height) in new[] { (0, 0), (0, 5), (5, 0) })
            foreach (ImageFlip flip in FLIPS)
            {
                Image<ColorRGB> image = Image<ColorRGB>.Create(new ColorRGB[width * height], width, height);
                string name = $"{width}x{height}, {flip}";

                Image<ColorRGB> flipped = image.Flip(flip);
                Assert.AreEqual(width, flipped.Width, name);
                Assert.AreEqual(height, flipped.Height, name);

                IImage fromInterface = ((IImage)image).Flip(flip);
                Assert.AreEqual(width, fromInterface.Width, name);
                Assert.AreEqual(height, fromInterface.Height, name);

                image.Flip(flip, Span<ColorRGB>.Empty);
                image.Flip(flip, Span<byte>.Empty, width * 3);
                ((IImage)image).Flip(flip, Span<byte>.Empty, width * 3);
            }
    }

    [TestMethod]
    public void FlipRejectsInvalidFlips()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(7, 5);
        byte[] buffer = new byte[7 * 5 * 3];

        foreach (ImageFlip flip in (ImageFlip[])[(ImageFlip)4, (ImageFlip)7, (ImageFlip)(-1)])
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Flip(flip), $"{flip}: Image");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.AsRefImage().Flip(flip), $"{flip}: RefImage");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((IImage)image).Flip(flip), $"{flip}: IImage");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Flip(flip, new ColorRGB[35].AsSpan()), $"{flip}: Span<T>");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Flip(flip, buffer, 21), $"{flip}: stride");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((IImage)image).Flip(flip, buffer, 21), $"{flip}: IImage stride");
        }
    }

    [TestMethod]
    public void FlipRejectsNullImages()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Image<ColorRGB>)null!).Flip(ImageFlip.None));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Image<ColorRGB>)null!).Flip(ImageFlip.None, new ColorRGB[1].AsSpan()));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Image<ColorRGB>)null!).Flip(ImageFlip.None, new byte[3], 3));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IImage)null!).Flip(ImageFlip.None));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IImage)null!).Flip(ImageFlip.None, new byte[3], 3));
    }

    [TestMethod]
    public void FlipRejectsSmallDestinations()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(7, 5);

        Assert.ThrowsExactly<ArgumentException>(() => image.Flip(ImageFlip.Horizontal, new ColorRGB[34].AsSpan()), "Span<T>");
        Assert.ThrowsExactly<ArgumentException>(() => image.AsRefImage().Flip(ImageFlip.Horizontal, new ColorRGB[34].AsSpan()), "RefImage, Span<T>");

        // 7 pixels of 3 bytes: the stride needs at least 21 bytes, the buffer at least 4 * 24 + 21 bytes with a stride of 24.
        Assert.ThrowsExactly<ArgumentException>(() => image.Flip(ImageFlip.Horizontal, new byte[200], 20), "stride too small");
        Assert.ThrowsExactly<ArgumentException>(() => image.Flip(ImageFlip.Horizontal, new byte[(4 * 24) + 20], 24), "buffer too short");
        Assert.ThrowsExactly<ArgumentException>(() => ((IImage)image).Flip(ImageFlip.Horizontal, new byte[(4 * 24) + 20], 24), "IImage, buffer too short");
        image.Flip(ImageFlip.Horizontal, new byte[(4 * 24) + 21], 24);
    }

    [TestMethod]
    public void FlipRejectsOverlappingDestinations()
    {
        // The image covers bytes 0 to 4 * 24 + 21 of the buffer, rows are 24 bytes apart.
        byte[] buffer = new byte[400];
        Image<ColorRGB> image = Image<ColorRGB>.Wrap(buffer, 7, 5, 24);
        int covered = (4 * 24) + 21;

        Assert.ThrowsExactly<ArgumentException>(() => image.Flip(ImageFlip.Horizontal, buffer, 24), "same buffer");
        Assert.ThrowsExactly<ArgumentException>(() => image.Flip(ImageFlip.Horizontal, buffer.AsSpan(covered - 1), 24), "overlaps the last byte");
        Assert.ThrowsExactly<ArgumentException>(() => image.Flip(ImageFlip.Horizontal, buffer.AsSpan(30), 21), "starts in the padding");
        Assert.ThrowsExactly<ArgumentException>(() => image.Flip(ImageFlip.Horizontal, MemoryMarshal.Cast<byte, ColorRGB>(buffer.AsSpan(3))), "Span<T>");
        Assert.ThrowsExactly<ArgumentException>(() => ((IImage)image).Flip(ImageFlip.Horizontal, buffer, 24), "IImage");

        image.Flip(ImageFlip.Horizontal, buffer.AsSpan(covered), 24);
        image.Flip(ImageFlip.Horizontal, MemoryMarshal.Cast<byte, ColorRGB>(buffer.AsSpan(covered)));
    }
}
