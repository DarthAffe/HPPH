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

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
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

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
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

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
            }
        }
    }

    [TestMethod]
    public void ConvertNarrow4ByteRGBAToRGB()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] data = ImageHelper.Get4ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGBA> referenceData = data;

            Span<ColorRGBA> sourceData = new ColorRGBA[referenceData.Length];
            referenceData.CopyTo(sourceData);

            Span<ColorRGB> result = PixelHelper.Convert<ColorRGBA, ColorRGB>(sourceData);

            Assert.AreEqual(referenceData.Length, result.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                ColorRGBA reference = referenceData[i];
                ColorRGB test = result[i];

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
            }
        }
    }

    [TestMethod]
    public void ConvertNarrow4ByteRGBAToBGR()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] data = ImageHelper.Get4ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGBA> referenceData = data;

            Span<ColorRGBA> sourceData = new ColorRGBA[referenceData.Length];
            referenceData.CopyTo(sourceData);

            Span<ColorBGR> result = PixelHelper.Convert<ColorRGBA, ColorBGR>(sourceData);

            Assert.AreEqual(referenceData.Length, result.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                ColorRGBA reference = referenceData[i];
                ColorBGR test = result[i];

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
            }
        }
    }

    [TestMethod]
    public void ConvertWiden3ByteRGBToRGBA()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.Get3ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGB> referenceData = data;

            Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
            referenceData.CopyTo(sourceData);

            Span<ColorRGBA> result = PixelHelper.Convert<ColorRGB, ColorRGBA>(sourceData);

            Assert.AreEqual(referenceData.Length, result.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                ColorRGB reference = referenceData[i];
                ColorRGBA test = result[i];

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
            }
        }
    }

    [TestMethod]
    public void ConvertWiden3ByteRGBToARGB()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.Get3ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGB> referenceData = data;

            Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
            referenceData.CopyTo(sourceData);

            Span<ColorARGB> result = PixelHelper.Convert<ColorRGB, ColorARGB>(sourceData);

            Assert.AreEqual(referenceData.Length, result.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                ColorRGB reference = referenceData[i];
                ColorARGB test = result[i];

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
            }
        }
    }

    [TestMethod]
    public void ConvertWiden3ByteRGBToBGRA()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.Get3ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGB> referenceData = data;

            Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
            referenceData.CopyTo(sourceData);

            Span<ColorBGRA> result = PixelHelper.Convert<ColorRGB, ColorBGRA>(sourceData);

            Assert.AreEqual(referenceData.Length, result.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                ColorRGB reference = referenceData[i];
                ColorBGRA test = result[i];

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
            }
        }
    }

    [TestMethod]
    public void ConvertWiden3ByteRGBToABGR()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.Get3ByteColorsFromImage(image).SkipLast(1).ToArray();
            ReadOnlySpan<ColorRGB> referenceData = data;

            Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
            referenceData.CopyTo(sourceData);

            Span<ColorABGR> result = PixelHelper.Convert<ColorRGB, ColorABGR>(sourceData);

            Assert.AreEqual(referenceData.Length, result.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                ColorRGB reference = referenceData[i];
                ColorABGR test = result[i];

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
            }
        }
    }
}