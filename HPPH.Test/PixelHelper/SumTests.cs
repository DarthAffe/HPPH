using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class SumTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void SumRefImage()
    {
        foreach (string image in GetTestImages())
        {
            SumRefImage<ColorRGB>(image);
            SumRefImage<ColorBGR>(image);
            SumRefImage<ColorRGBA>(image);
            SumRefImage<ColorBGRA>(image);
            SumRefImage<ColorARGB>(image);
            SumRefImage<ColorABGR>(image);
        }
    }

    private static void SumRefImage<T>(string image)
        where T : unmanaged, IColor<T>
    {
        RefImage<T> data = ImageHelper.GetImage<T>(image).AsRefImage<T>();

        ColorSum reference = ReferencePixelHelper.Sum(data);
        ColorSum test = data.Sum();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void SumGenericImage()
    {
        foreach (string image in GetTestImages())
        {
            SumGenericImage<ColorRGB>(image);
            SumGenericImage<ColorBGR>(image);
            SumGenericImage<ColorRGBA>(image);
            SumGenericImage<ColorBGRA>(image);
            SumGenericImage<ColorARGB>(image);
            SumGenericImage<ColorABGR>(image);
        }
    }

    private static void SumGenericImage<T>(string image)
        where T : unmanaged, IColor<T>
    {
        Image<T> data = ImageHelper.GetImage<T>(image);

        ColorSum reference = ReferencePixelHelper.Sum(data);
        ColorSum test = data.Sum();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void SumImage()
    {
        foreach (string image in GetTestImages())
        {
            SumImage<ColorRGB>(image);
            SumImage<ColorBGR>(image);
            SumImage<ColorRGBA>(image);
            SumImage<ColorBGRA>(image);
            SumImage<ColorARGB>(image);
            SumImage<ColorABGR>(image);
        }
    }

    private static void SumImage<T>(string image)
        where T : unmanaged, IColor<T>
    {
        IImage data = ImageHelper.GetImage<T>(image);

        ColorSum reference = ReferencePixelHelper.Sum(data);
        ColorSum test = data.Sum();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void SumSpan()
    {
        foreach (string image in GetTestImages())
        {
            SumSpan<ColorRGB>(image);
            SumSpan<ColorBGR>(image);
            SumSpan<ColorRGBA>(image);
            SumSpan<ColorBGRA>(image);
            SumSpan<ColorARGB>(image);
            SumSpan<ColorABGR>(image);
        }
    }

    private static void SumSpan<T>(string image)
        where T : unmanaged, IColor<T>
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        Span<T> span = data;

        ColorSum reference = ReferencePixelHelper.Sum(span);
        ColorSum test = span.Sum();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void SumReadOnlySpan()
    {
        foreach (string image in GetTestImages())
        {
            SumReadOnlySpan<ColorRGB>(image);
            SumReadOnlySpan<ColorBGR>(image);
            SumReadOnlySpan<ColorRGBA>(image);
            SumReadOnlySpan<ColorBGRA>(image);
            SumReadOnlySpan<ColorARGB>(image);
            SumReadOnlySpan<ColorABGR>(image);
        }
    }

    private static void SumReadOnlySpan<T>(string image)
        where T : unmanaged, IColor<T>
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        ReadOnlySpan<T> span = data;

        ColorSum reference = ReferencePixelHelper.Sum(span);
        ColorSum test = span.Sum();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void ThreeByteSumAlphaOverflowTest()
    {
        ColorRGB[] colors = new ColorRGB[4096 * 2160];
        Assert.AreEqual(2_256_076_800L, new ReadOnlySpan<ColorRGB>(colors).Sum().A);
    }
}