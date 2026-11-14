using System.Runtime.InteropServices;
using HPPH.Reference;
using HPPH.Test.Infrastructure;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class RotateTests
{
    private static readonly ImageRotation[] ROTATIONS = [ImageRotation.None, ImageRotation.Rotate90, ImageRotation.Rotate180, ImageRotation.Rotate270];

    [TestMethod]
    public void RotateMatchesReference()
    {
        RotateMatchesReference<ColorRGB>();
        RotateMatchesReference<ColorBGR>();
        RotateMatchesReference<ColorRGBA>();
        RotateMatchesReference<ColorBGRA>();
        RotateMatchesReference<ColorARGB>();
        RotateMatchesReference<ColorABGR>();
    }

    private static void RotateMatchesReference<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((int width, int height) in TestImages.SIZES)
            foreach ((string name, Image<T> image) in TestImages.Variants<T>(width, height, (width * 31) + height))
                foreach (ImageRotation rotation in ROTATIONS)
                    CheckAllOverloads(image, rotation, $"{T.ColorFormat.Name} {width}x{height} {name}, {rotation}");
    }

    [TestMethod]
    public void RotateFullHD()
    {
        foreach ((string name, Image<ColorBGRA> image) in TestImages.Variants<ColorBGRA>(1920, 1080, 1))
            foreach (ImageRotation rotation in ROTATIONS)
                CheckAllOverloads(image, rotation, $"1920x1080 {name}, {rotation}");
    }

    private static void CheckAllOverloads<T>(Image<T> image, ImageRotation rotation, string name)
        where T : unmanaged, IColor<T>
    {
        Image<T> expected = ReferencePixelHelper.Rotate(image.AsRefImage(), rotation);
        int pixels = expected.Width * expected.Height;
        int rowSize = expected.Width * T.ColorFormat.BytesPerPixel;

        TestImages.AssertEqual(expected, image.Rotate(rotation), $"{name}: Image");
        TestImages.AssertEqual(expected, image.AsRefImage().Rotate(rotation), $"{name}: RefImage");

        IImage fromInterface = ((IImage)image).Rotate(rotation);
        Assert.IsInstanceOfType<Image<T>>(fromInterface, $"{name}: IImage");
        TestImages.AssertEqual(expected, (Image<T>)fromInterface, $"{name}: IImage");

        T[] span = TestImages.CreateSentinelPixels<T>(pixels);
        image.Rotate(rotation, span.AsSpan());
        TestImages.AssertCompact(expected, span, $"{name}: Image, Span<T>");

        T[] refSpan = TestImages.CreateSentinelPixels<T>(pixels);
        image.AsRefImage().Rotate(rotation, refSpan.AsSpan());
        TestImages.AssertCompact(expected, refSpan, $"{name}: RefImage, Span<T>");

        foreach (int padding in (int[])[0, 5])
        {
            int stride = rowSize + padding;
            int length = (Math.Max(0, expected.Height - 1) * stride) + rowSize + 7;

            byte[] fromImage = TestImages.CreateSentinelBuffer(length);
            image.Rotate(rotation, fromImage, stride);
            TestImages.AssertStrided(expected, fromImage, stride, $"{name}, stride {stride}: Image");

            byte[] fromRefImage = TestImages.CreateSentinelBuffer(length);
            image.AsRefImage().Rotate(rotation, fromRefImage, stride);
            TestImages.AssertStrided(expected, fromRefImage, stride, $"{name}, stride {stride}: RefImage");

            byte[] fromIImage = TestImages.CreateSentinelBuffer(length);
            ((IImage)image).Rotate(rotation, fromIImage, stride);
            TestImages.AssertStrided(expected, fromIImage, stride, $"{name}, stride {stride}: IImage");
        }
    }

    [TestMethod]
    public void RotateIdentities()
    {
        RotateIdentities<ColorRGB>();
        RotateIdentities<ColorBGRA>();
    }

    private static void RotateIdentities<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((string name, Image<T> image) in TestImages.Variants<T>(31, 17, 3))
        {
            Image<T> rotated90 = image.Rotate(ImageRotation.Rotate90);
            Assert.AreEqual(image.Height, rotated90.Width, $"{name}: width after 90");
            Assert.AreEqual(image.Width, rotated90.Height, $"{name}: height after 90");

            TestImages.AssertEqual(image, rotated90.Rotate(ImageRotation.Rotate90).Rotate(ImageRotation.Rotate90).Rotate(ImageRotation.Rotate90), $"{name}: 90 four times");
            TestImages.AssertEqual(image, rotated90.Rotate(ImageRotation.Rotate270), $"{name}: 90 then 270");
            TestImages.AssertEqual(image, image.Rotate(ImageRotation.Rotate270).Rotate(ImageRotation.Rotate90), $"{name}: 270 then 90");
            TestImages.AssertEqual(image.Rotate(ImageRotation.Rotate180), rotated90.Rotate(ImageRotation.Rotate90), $"{name}: 90 twice");
            TestImages.AssertEqual(image.Rotate(ImageRotation.Rotate180), image.Flip(ImageFlip.Horizontal | ImageFlip.Vertical), $"{name}: 180 is both flips");
            TestImages.AssertEqual(image, image.Rotate(ImageRotation.None), $"{name}: none");
        }
    }

    [TestMethod]
    public void RotateOfEmptyImages()
    {
        foreach ((int width, int height) in new[] { (0, 0), (0, 5), (5, 0) })
            foreach (ImageRotation rotation in ROTATIONS)
            {
                Image<ColorRGB> image = Image<ColorRGB>.Create(new ColorRGB[width * height], width, height);
                bool quarter = rotation is ImageRotation.Rotate90 or ImageRotation.Rotate270;
                int rotatedWidth = quarter ? height : width;
                int rotatedHeight = quarter ? width : height;
                string name = $"{width}x{height}, {rotation}";

                Image<ColorRGB> rotated = image.Rotate(rotation);
                Assert.AreEqual(rotatedWidth, rotated.Width, name);
                Assert.AreEqual(rotatedHeight, rotated.Height, name);

                IImage fromInterface = ((IImage)image).Rotate(rotation);
                Assert.AreEqual(rotatedWidth, fromInterface.Width, name);
                Assert.AreEqual(rotatedHeight, fromInterface.Height, name);

                image.Rotate(rotation, Span<ColorRGB>.Empty);
                image.Rotate(rotation, Span<byte>.Empty, rotatedWidth * 3);
                ((IImage)image).Rotate(rotation, Span<byte>.Empty, rotatedWidth * 3);
            }
    }

    [TestMethod]
    public void RotateRejectsInvalidRotations()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(7, 5);
        byte[] buffer = new byte[7 * 5 * 3];

        foreach (ImageRotation rotation in (ImageRotation[])[(ImageRotation)45, (ImageRotation)1, (ImageRotation)(-90), (ImageRotation)360])
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Rotate(rotation), $"{rotation}: Image");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.AsRefImage().Rotate(rotation), $"{rotation}: RefImage");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((IImage)image).Rotate(rotation), $"{rotation}: IImage");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Rotate(rotation, new ColorRGB[35].AsSpan()), $"{rotation}: Span<T>");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Rotate(rotation, buffer, 21), $"{rotation}: stride");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((IImage)image).Rotate(rotation, buffer, 21), $"{rotation}: IImage stride");
        }
    }

    [TestMethod]
    public void RotateRejectsNullImages()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Image<ColorRGB>)null!).Rotate(ImageRotation.Rotate90));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Image<ColorRGB>)null!).Rotate(ImageRotation.Rotate90, new ColorRGB[1].AsSpan()));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Image<ColorRGB>)null!).Rotate(ImageRotation.Rotate90, new byte[3], 3));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IImage)null!).Rotate(ImageRotation.Rotate90));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IImage)null!).Rotate(ImageRotation.Rotate90, new byte[3], 3));
    }

    [TestMethod]
    public void RotateRejectsSmallDestinations()
    {
        // 7x5 rotated by 90 degrees is 5x7: rows of 15 bytes, the buffer needs 6 * stride + 15 bytes.
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(7, 5);

        Assert.ThrowsExactly<ArgumentException>(() => image.Rotate(ImageRotation.Rotate90, new ColorRGB[34].AsSpan()), "Span<T>");
        Assert.ThrowsExactly<ArgumentException>(() => image.AsRefImage().Rotate(ImageRotation.Rotate90, new ColorRGB[34].AsSpan()), "RefImage, Span<T>");

        Assert.ThrowsExactly<ArgumentException>(() => image.Rotate(ImageRotation.Rotate90, new byte[200], 14), "stride too small");
        Assert.ThrowsExactly<ArgumentException>(() => image.Rotate(ImageRotation.Rotate90, new byte[(6 * 16) + 14], 16), "buffer too short");
        Assert.ThrowsExactly<ArgumentException>(() => ((IImage)image).Rotate(ImageRotation.Rotate90, new byte[(6 * 16) + 14], 16), "IImage, buffer too short");
        image.Rotate(ImageRotation.Rotate90, new byte[(6 * 16) + 15], 16);

        // 5x7 rotated is 7x5: the 15 byte rows of the source are too short for the result, even though the buffer is big enough.
        Image<ColorRGB> narrow = TestDataHelper.CreateTestImage<ColorRGB>(5, 7);
        Assert.ThrowsExactly<ArgumentException>(() => narrow.Rotate(ImageRotation.Rotate90, new byte[400], 15), "stride of the source");
        narrow.Rotate(ImageRotation.Rotate180, new byte[400], 15);
    }

    [TestMethod]
    public void RotateRejectsOverlappingDestinations()
    {
        // The image covers bytes 0 to 4 * 24 + 21 of the buffer, rows are 24 bytes apart.
        byte[] buffer = new byte[400];
        Image<ColorRGB> image = Image<ColorRGB>.Wrap(buffer, 7, 5, 24);
        int covered = (4 * 24) + 21;

        Assert.ThrowsExactly<ArgumentException>(() => image.Rotate(ImageRotation.Rotate90, buffer, 15), "same buffer");
        Assert.ThrowsExactly<ArgumentException>(() => image.Rotate(ImageRotation.Rotate90, buffer.AsSpan(covered - 1), 15), "overlaps the last byte");
        Assert.ThrowsExactly<ArgumentException>(() => image.Rotate(ImageRotation.Rotate90, buffer.AsSpan(30), 15), "starts in the padding");
        Assert.ThrowsExactly<ArgumentException>(() => image.Rotate(ImageRotation.Rotate90, MemoryMarshal.Cast<byte, ColorRGB>(buffer.AsSpan(3))), "Span<T>");
        Assert.ThrowsExactly<ArgumentException>(() => ((IImage)image).Rotate(ImageRotation.Rotate90, buffer, 15), "IImage");

        // Directly after the image is fine.
        image.Rotate(ImageRotation.Rotate90, buffer.AsSpan(covered), 15);
        image.Rotate(ImageRotation.Rotate90, MemoryMarshal.Cast<byte, ColorRGB>(buffer.AsSpan(covered)));
    }
}
