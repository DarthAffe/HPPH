using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class MinMaxTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void MinMaxImage3Byte()
    {
    }

    [TestMethod]
    public void MinMaxRefImage3Byte()
    {
    }

    [TestMethod]
    public void MinMaxReadOnlySpan3Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.Get3ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGB> span = data;

            IMinMax reference = ReferencePixelHelper.MinMax(span);
            IMinMax test = span.MinMax();

            Assert.AreEqual(reference.RedMin, test.RedMin, "RedMin differs");
            Assert.AreEqual(reference.GreenMin, test.GreenMin, "GreenMin differs");
            Assert.AreEqual(reference.BlueMin, test.BlueMin, "BlueMin differs");
            Assert.AreEqual(reference.AlphaMin, test.AlphaMin, "AlphaMin differs");

            Assert.AreEqual(reference.RedMax, test.RedMax, "RedMax differs");
            Assert.AreEqual(reference.GreenMax, test.GreenMax, "GreenMax differs");
            Assert.AreEqual(reference.BlueMax, test.BlueMax, "BlueMax differs");
            Assert.AreEqual(reference.AlphaMax, test.AlphaMax, "AlphaMax differs");

            Assert.AreEqual(reference.RedRange, test.RedRange, "RedRange differs");
            Assert.AreEqual(reference.GreenRange, test.GreenRange, "GreenRange differs");
            Assert.AreEqual(reference.BlueRange, test.BlueRange, "BlueRange differs");
            Assert.AreEqual(reference.AlphaRange, test.AlphaRange, "AlphaRange differs");
        }
    }

    [TestMethod]
    public void MinMaxImage4Byte()
    {
    }

    [TestMethod]
    public void MinMaxRefImage4Byte()
    {
    }

    [TestMethod]
    public void MinMaxReadOnlySpan4Byte()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] data = ImageHelper.Get4ByteColorsFromImage(image);
            ReadOnlySpan<ColorRGBA> span = data;

            IMinMax reference = ReferencePixelHelper.MinMax(span);
            IMinMax test = span.MinMax();

            Assert.AreEqual(reference.RedMin, test.RedMin, "RedMin differs");
            Assert.AreEqual(reference.GreenMin, test.GreenMin, "GreenMin differs");
            Assert.AreEqual(reference.BlueMin, test.BlueMin, "BlueMin differs");
            Assert.AreEqual(reference.AlphaMin, test.AlphaMin, "AlphaMin differs");

            Assert.AreEqual(reference.RedMax, test.RedMax, "RedMax differs");
            Assert.AreEqual(reference.GreenMax, test.GreenMax, "GreenMax differs");
            Assert.AreEqual(reference.BlueMax, test.BlueMax, "BlueMax differs");
            Assert.AreEqual(reference.AlphaMax, test.AlphaMax, "AlphaMax differs");

            Assert.AreEqual(reference.RedRange, test.RedRange, "RedRange differs");
            Assert.AreEqual(reference.GreenRange, test.GreenRange, "GreenRange differs");
            Assert.AreEqual(reference.BlueRange, test.BlueRange, "BlueRange differs");
            Assert.AreEqual(reference.AlphaRange, test.AlphaRange, "AlphaRange differs");
        }
    }
}