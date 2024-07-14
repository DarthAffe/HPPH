using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class SumTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void SumImage3Byte()
    {
    }

    [TestMethod]
    public void SumRefImage3Byte()
    {
    }

    [TestMethod]
    public void SumReadOnlySpan3Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.Get3ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGB> span = data;

            ISum reference = ReferencePixelHelper.Sum(span);
            ISum test = span.Sum();

            Assert.AreEqual(reference.R, test.R, "R differs");
            Assert.AreEqual(reference.G, test.G, "G differs");
            Assert.AreEqual(reference.B, test.B, "B differs");
            Assert.AreEqual(reference.A, test.A, "A differs");
        }
    }

    [TestMethod]
    public void SumImage4Byte()
    {
    }

    [TestMethod]
    public void SumRefImage4Byte()
    {
    }

    [TestMethod]
    public void SumReadOnlySpan4Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] data = ImageHelper.Get4ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGBA> span = data;

            ISum reference = ReferencePixelHelper.Sum(span);
            ISum test = span.Sum();

            Assert.AreEqual(reference.R, test.R, "R differs");
            Assert.AreEqual(reference.G, test.G, "G differs");
            Assert.AreEqual(reference.B, test.B, "B differs");
            Assert.AreEqual(reference.A, test.A, "A differs");
        }
    }
}