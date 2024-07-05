using HPPH.Reference;

namespace HPPH.Test;

[TestClass]
public class AverageTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void AverageImage3Byte()
    {
    }

    [TestMethod]
    public void AverageRefImage3Byte()
    {
    }

    [TestMethod]
    public void AverageReadOnlySpan3Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.Get3ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGB> span = data;

            ColorRGB reference = ReferencePixelHelper.Average(span);
            ColorRGB test = PixelHelper.Average(span);

            Assert.AreEqual(reference.R, test.R, "R differs");
            Assert.AreEqual(reference.G, test.G, "G differs");
            Assert.AreEqual(reference.B, test.B, "B differs");
            Assert.AreEqual(reference.A, test.A, "A differs");
        }
    }

    [TestMethod]
    public void AverageImage4Byte()
    {
    }

    [TestMethod]
    public void AverageRefImage4Byte()
    {
    }

    [TestMethod]
    public void AverageReadOnlySpan4Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] data = ImageHelper.Get4ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGBA> span = data;

            ColorRGBA reference = ReferencePixelHelper.Average(span);
            ColorRGBA test = PixelHelper.Average(span);

            Assert.AreEqual(reference.R, test.R, "R differs");
            Assert.AreEqual(reference.G, test.G, "G differs");
            Assert.AreEqual(reference.B, test.B, "B differs");
            Assert.AreEqual(reference.A, test.A, "A differs");
        }
    }
}