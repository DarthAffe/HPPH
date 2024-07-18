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
        where T : struct, IColor
    {
        RefImage<T> data = ImageHelper.GetImage<T>(image).AsRefImage<T>();

        ISum reference = ReferencePixelHelper.Sum(data);
        ISum test = data.Sum();

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
        where T : struct, IColor
    {
        Image<T> data = ImageHelper.GetImage<T>(image);

        ISum reference = ReferencePixelHelper.Sum(data);
        ISum test = data.Sum();

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
        where T : struct, IColor
    {
        IImage data = ImageHelper.GetImage<T>(image);

        ISum reference = ReferencePixelHelper.Sum(data);
        ISum test = data.Sum();

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
        where T : struct, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        Span<T> span = data;

        ISum reference = ReferencePixelHelper.Sum(span);
        ISum test = span.Sum();

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
        where T : struct, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        ReadOnlySpan<T> span = data;

        ISum reference = ReferencePixelHelper.Sum(span);
        ISum test = span.Sum();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }
}