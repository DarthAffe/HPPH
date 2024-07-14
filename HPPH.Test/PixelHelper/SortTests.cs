using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class SortTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);


    [TestMethod]
    public void SortByRed3Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] referenceData = ImageHelper.Get3ByteColorsFromImage(image);
            ColorRGB[] testData = new ColorRGB[referenceData.Length];
            Span<ColorRGB> referenceSpan = referenceData;
            Span<ColorRGB> testSpan = testData;
            referenceSpan.CopyTo(testSpan);

            ReferencePixelHelper.SortByRed(referenceSpan);
            testSpan.SortByRed();

            for (int i = 0; i < referenceData.Length; i++)
                Assert.AreEqual(referenceSpan[i], testSpan[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void SortByGreen3Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] referenceData = ImageHelper.Get3ByteColorsFromImage(image);
            ColorRGB[] testData = new ColorRGB[referenceData.Length];
            Span<ColorRGB> referenceSpan = referenceData;
            Span<ColorRGB> testSpan = testData;
            referenceSpan.CopyTo(testSpan);

            ReferencePixelHelper.SortByGreen(referenceSpan);
            testSpan.SortByGreen();

            for (int i = 0; i < referenceData.Length; i++)
                Assert.AreEqual(referenceSpan[i], testSpan[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void SortByBlue3Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] referenceData = ImageHelper.Get3ByteColorsFromImage(image);
            ColorRGB[] testData = new ColorRGB[referenceData.Length];
            Span<ColorRGB> referenceSpan = referenceData;
            Span<ColorRGB> testSpan = testData;
            referenceSpan.CopyTo(testSpan);

            ReferencePixelHelper.SortByBlue(referenceSpan);
            testSpan.SortByBlue();

            for (int i = 0; i < referenceData.Length; i++)
                Assert.AreEqual(referenceSpan[i], testSpan[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void SortByRed4Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] referenceData = ImageHelper.Get4ByteColorsFromImage(image);
            ColorRGBA[] testData = new ColorRGBA[referenceData.Length];
            Span<ColorRGBA> referenceSpan = referenceData;
            Span<ColorRGBA> testSpan = testData;
            referenceSpan.CopyTo(testSpan);

            ReferencePixelHelper.SortByRed(referenceSpan);
            testSpan.SortByRed();

            for (int i = 0; i < referenceData.Length; i++)
                Assert.AreEqual(referenceSpan[i], testSpan[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void SortByGreen4Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] referenceData = ImageHelper.Get4ByteColorsFromImage(image);
            ColorRGBA[] testData = new ColorRGBA[referenceData.Length];
            Span<ColorRGBA> referenceSpan = referenceData;
            Span<ColorRGBA> testSpan = testData;
            referenceSpan.CopyTo(testSpan);

            ReferencePixelHelper.SortByGreen(referenceSpan);
            testSpan.SortByGreen();

            for (int i = 0; i < referenceData.Length; i++)
                Assert.AreEqual(referenceSpan[i], testSpan[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void SortByBlue4Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] referenceData = ImageHelper.Get4ByteColorsFromImage(image);
            ColorRGBA[] testData = new ColorRGBA[referenceData.Length];
            Span<ColorRGBA> referenceSpan = referenceData;
            Span<ColorRGBA> testSpan = testData;
            referenceSpan.CopyTo(testSpan);

            ReferencePixelHelper.SortByBlue(referenceSpan);
            testSpan.SortByBlue();

            for (int i = 0; i < referenceData.Length; i++)
                Assert.AreEqual(referenceSpan[i], testSpan[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void SortByAlpha4Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] referenceData = ImageHelper.Get4ByteColorsFromImage(image);
            ColorRGBA[] testData = new ColorRGBA[referenceData.Length];
            Span<ColorRGBA> referenceSpan = referenceData;
            Span<ColorRGBA> testSpan = testData;
            referenceSpan.CopyTo(testSpan);

            ReferencePixelHelper.SortByAlpha(referenceSpan);
            testSpan.SortByAlpha();

            for (int i = 0; i < referenceData.Length; i++)
                Assert.AreEqual(referenceSpan[i], testSpan[i], $"Index {i} differs");
        }
    }
}