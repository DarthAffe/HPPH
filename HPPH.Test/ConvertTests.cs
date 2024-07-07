namespace HPPH.Test;

[TestClass]
public class ConvertTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void Convert3ByteSameBppRGBToBGR()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.Get3ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGB> referenceData = data;

            Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
            referenceData.CopyTo(sourceData);

            Span<ColorBGR> result = PixelHelper.Convert<ColorRGB, ColorBGR>(sourceData);

            Assert.AreEqual(referenceData.Length, result.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                ColorRGB reference = referenceData[i];
                ColorBGR test = result[i];

                Assert.AreEqual(reference.R, test.R, "R differs");
                Assert.AreEqual(reference.G, test.G, "G differs");
                Assert.AreEqual(reference.B, test.B, "B differs");
                Assert.AreEqual(reference.A, test.A, "A differs");
            }
        }
    }

    [TestMethod]
    public void Convert4ByteSameBppRGBAToARGB()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] data = ImageHelper.Get4ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGBA> referenceData = data;

            Span<ColorRGBA> sourceData = new ColorRGBA[referenceData.Length];
            referenceData.CopyTo(sourceData);

            Span<ColorARGB> result = PixelHelper.Convert<ColorRGBA, ColorARGB>(sourceData);

            Assert.AreEqual(referenceData.Length, result.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                ColorRGBA reference = referenceData[i];
                ColorARGB test = result[i];

                Assert.AreEqual(reference.R, test.R, "R differs");
                Assert.AreEqual(reference.G, test.G, "G differs");
                Assert.AreEqual(reference.B, test.B, "B differs");
                Assert.AreEqual(reference.A, test.A, "A differs");
            }
        }
    }

    [TestMethod]
    public void Convert4ByteSameBppRGBAToBGRA()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] data = ImageHelper.Get4ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGBA> referenceData = data;

            Span<ColorRGBA> sourceData = new ColorRGBA[referenceData.Length];
            referenceData.CopyTo(sourceData);

            Span<ColorBGRA> result = PixelHelper.Convert<ColorRGBA, ColorBGRA>(sourceData);

            Assert.AreEqual(referenceData.Length, result.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                ColorRGBA reference = referenceData[i];
                ColorBGRA test = result[i];

                Assert.AreEqual(reference.R, test.R, "R differs");
                Assert.AreEqual(reference.G, test.G, "G differs");
                Assert.AreEqual(reference.B, test.B, "B differs");
                Assert.AreEqual(reference.A, test.A, "A differs");
            }
        }
    }
}