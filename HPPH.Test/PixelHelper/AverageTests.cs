using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class AverageTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void AverageRefImage()
    {
        foreach (string image in GetTestImages())
        {
            AverageRefImage<ColorRGB>(image);
            AverageRefImage<ColorBGR>(image);
            AverageRefImage<ColorRGBA>(image);
            AverageRefImage<ColorBGRA>(image);
            AverageRefImage<ColorARGB>(image);
            AverageRefImage<ColorABGR>(image);
        }
    }

    private static void AverageRefImage<T>(string image)
        where T : struct, IColor
    {
        RefImage<T> data = ImageHelper.GetImage<T>(image).AsRefImage<T>();

        T reference = ReferencePixelHelper.Average(data);
        T test = data.Average();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void AverageGenericImage()
    {
        foreach (string image in GetTestImages())
        {
            AverageGenericImage<ColorRGB>(image);
            AverageGenericImage<ColorBGR>(image);
            AverageGenericImage<ColorRGBA>(image);
            AverageGenericImage<ColorBGRA>(image);
            AverageGenericImage<ColorARGB>(image);
            AverageGenericImage<ColorABGR>(image);
        }
    }

    private static void AverageGenericImage<T>(string image)
        where T : struct, IColor
    {
        Image<T> data = ImageHelper.GetImage<T>(image);

        IColor reference = ReferencePixelHelper.Average(data);
        T test = data.Average();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void AverageImage()
    {
        foreach (string image in GetTestImages())
        {
            AverageImage<ColorRGB>(image);
            AverageImage<ColorBGR>(image);
            AverageImage<ColorRGBA>(image);
            AverageImage<ColorBGRA>(image);
            AverageImage<ColorARGB>(image);
            AverageImage<ColorABGR>(image);
        }
    }

    private static void AverageImage<T>(string image)
        where T : struct, IColor
    {
        IImage data = ImageHelper.GetImage<T>(image);

        IColor reference = ReferencePixelHelper.Average(data);
        IColor test = data.Average();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void AverageSpan()
    {
        foreach (string image in GetTestImages())
        {
            AverageSpan<ColorRGB>(image);
            AverageSpan<ColorBGR>(image);
            AverageSpan<ColorRGBA>(image);
            AverageSpan<ColorBGRA>(image);
            AverageSpan<ColorARGB>(image);
            AverageSpan<ColorABGR>(image);
        }
    }

    private static void AverageSpan<T>(string image)
        where T : struct, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        Span<T> span = data;

        T reference = ReferencePixelHelper.Average(span);
        T test = span.Average();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void AverageReadOnlySpan()
    {
        foreach (string image in GetTestImages())
        {
            AverageReadOnlySpan<ColorRGB>(image);
            AverageReadOnlySpan<ColorBGR>(image);
            AverageReadOnlySpan<ColorRGBA>(image);
            AverageReadOnlySpan<ColorBGRA>(image);
            AverageReadOnlySpan<ColorARGB>(image);
            AverageReadOnlySpan<ColorABGR>(image);
        }
    }

    private static void AverageReadOnlySpan<T>(string image)
        where T : struct, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        ReadOnlySpan<T> span = data;

        T reference = ReferencePixelHelper.Average(span);
        T test = span.Average();

        Assert.AreEqual(reference.R, test.R, $"R differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.G, test.G, $"G differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.B, test.B, $"B differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.A, test.A, $"A differs in type '{T.ColorFormat.Name}'");
    }
}