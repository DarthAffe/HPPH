using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class MinMaxTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void MinMaxRefImage()
    {
        foreach (string image in GetTestImages())
        {
            MinMaxRefImage<ColorRGB>(image);
            MinMaxRefImage<ColorBGR>(image);
            MinMaxRefImage<ColorRGBA>(image);
            MinMaxRefImage<ColorBGRA>(image);
            MinMaxRefImage<ColorARGB>(image);
            MinMaxRefImage<ColorABGR>(image);
        }
    }

    private static void MinMaxRefImage<T>(string image)
        where T : struct, IColor
    {
        RefImage<T> data = ImageHelper.GetImage<T>(image).AsRefImage<T>();

        IMinMax reference = ReferencePixelHelper.MinMax(data);
        IMinMax test = data.MinMax();

        Assert.AreEqual(reference.RedMin, test.RedMin, $"RedMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenMin, test.GreenMin, $"GreenMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueMin, test.BlueMin, $"BlueMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaMin, test.AlphaMin, $"AlphaMin differs in type '{T.ColorFormat.Name}'");

        Assert.AreEqual(reference.RedMax, test.RedMax, $"RedMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenMax, test.GreenMax, $"GreenMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueMax, test.BlueMax, $"BlueMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaMax, test.AlphaMax, $"AlphaMax differs in type '{T.ColorFormat.Name}'");

        Assert.AreEqual(reference.RedRange, test.RedRange, $"RedRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenRange, test.GreenRange, $"GreenRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueRange, test.BlueRange, $"BlueRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaRange, test.AlphaRange, $"AlphaRange differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void MinMaxGenericImage()
    {
        foreach (string image in GetTestImages())
        {
            MinMaxGenericImage<ColorRGB>(image);
            MinMaxGenericImage<ColorBGR>(image);
            MinMaxGenericImage<ColorRGBA>(image);
            MinMaxGenericImage<ColorBGRA>(image);
            MinMaxGenericImage<ColorARGB>(image);
            MinMaxGenericImage<ColorABGR>(image);
        }
    }

    private static void MinMaxGenericImage<T>(string image)
        where T : struct, IColor
    {
        Image<T> data = ImageHelper.GetImage<T>(image);

        IMinMax reference = ReferencePixelHelper.MinMax(data);
        IMinMax test = data.MinMax();

        Assert.AreEqual(reference.RedMin, test.RedMin, $"RedMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenMin, test.GreenMin, $"GreenMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueMin, test.BlueMin, $"BlueMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaMin, test.AlphaMin, $"AlphaMin differs in type '{T.ColorFormat.Name}'");

        Assert.AreEqual(reference.RedMax, test.RedMax, $"RedMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenMax, test.GreenMax, $"GreenMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueMax, test.BlueMax, $"BlueMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaMax, test.AlphaMax, $"AlphaMax differs in type '{T.ColorFormat.Name}'");

        Assert.AreEqual(reference.RedRange, test.RedRange, $"RedRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenRange, test.GreenRange, $"GreenRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueRange, test.BlueRange, $"BlueRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaRange, test.AlphaRange, $"AlphaRange differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void MinMaxImage()
    {
        foreach (string image in GetTestImages())
        {
            MinMaxImage<ColorRGB>(image);
            MinMaxImage<ColorBGR>(image);
            MinMaxImage<ColorRGBA>(image);
            MinMaxImage<ColorBGRA>(image);
            MinMaxImage<ColorARGB>(image);
            MinMaxImage<ColorABGR>(image);
        }
    }

    private static void MinMaxImage<T>(string image)
        where T : struct, IColor
    {
        IImage data = ImageHelper.GetImage<T>(image);

        IMinMax reference = ReferencePixelHelper.MinMax(data);
        IMinMax test = data.MinMax();

        Assert.AreEqual(reference.RedMin, test.RedMin, $"RedMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenMin, test.GreenMin, $"GreenMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueMin, test.BlueMin, $"BlueMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaMin, test.AlphaMin, $"AlphaMin differs in type '{T.ColorFormat.Name}'");

        Assert.AreEqual(reference.RedMax, test.RedMax, $"RedMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenMax, test.GreenMax, $"GreenMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueMax, test.BlueMax, $"BlueMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaMax, test.AlphaMax, $"AlphaMax differs in type '{T.ColorFormat.Name}'");

        Assert.AreEqual(reference.RedRange, test.RedRange, $"RedRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenRange, test.GreenRange, $"GreenRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueRange, test.BlueRange, $"BlueRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaRange, test.AlphaRange, $"AlphaRange differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void MinMaxSpan()
    {
        foreach (string image in GetTestImages())
        {
            MinMaxSpan<ColorRGB>(image);
            MinMaxSpan<ColorBGR>(image);
            MinMaxSpan<ColorRGBA>(image);
            MinMaxSpan<ColorBGRA>(image);
            MinMaxSpan<ColorARGB>(image);
            MinMaxSpan<ColorABGR>(image);
        }
    }

    private static void MinMaxSpan<T>(string image)
        where T : struct, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        Span<T> span = data;

        IMinMax reference = ReferencePixelHelper.MinMax(span);
        IMinMax test = span.MinMax();

        Assert.AreEqual(reference.RedMin, test.RedMin, $"RedMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenMin, test.GreenMin, $"GreenMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueMin, test.BlueMin, $"BlueMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaMin, test.AlphaMin, $"AlphaMin differs in type '{T.ColorFormat.Name}'");

        Assert.AreEqual(reference.RedMax, test.RedMax, $"RedMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenMax, test.GreenMax, $"GreenMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueMax, test.BlueMax, $"BlueMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaMax, test.AlphaMax, $"AlphaMax differs in type '{T.ColorFormat.Name}'");

        Assert.AreEqual(reference.RedRange, test.RedRange, $"RedRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenRange, test.GreenRange, $"GreenRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueRange, test.BlueRange, $"BlueRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaRange, test.AlphaRange, $"AlphaRange differs in type '{T.ColorFormat.Name}'");
    }

    [TestMethod]
    public void MinMaxReadOnlySpan()
    {
        foreach (string image in GetTestImages())
        {
            MinMaxReadOnlySpan<ColorRGB>(image);
            MinMaxReadOnlySpan<ColorBGR>(image);
            MinMaxReadOnlySpan<ColorRGBA>(image);
            MinMaxReadOnlySpan<ColorBGRA>(image);
            MinMaxReadOnlySpan<ColorARGB>(image);
            MinMaxReadOnlySpan<ColorABGR>(image);
        }
    }

    private static void MinMaxReadOnlySpan<T>(string image)
        where T : struct, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        ReadOnlySpan<T> span = data;

        IMinMax reference = ReferencePixelHelper.MinMax(span);
        IMinMax test = span.MinMax();

        Assert.AreEqual(reference.RedMin, test.RedMin, $"RedMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenMin, test.GreenMin, $"GreenMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueMin, test.BlueMin, $"BlueMin differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaMin, test.AlphaMin, $"AlphaMin differs in type '{T.ColorFormat.Name}'");

        Assert.AreEqual(reference.RedMax, test.RedMax, $"RedMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenMax, test.GreenMax, $"GreenMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueMax, test.BlueMax, $"BlueMax differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaMax, test.AlphaMax, $"AlphaMax differs in type '{T.ColorFormat.Name}'");

        Assert.AreEqual(reference.RedRange, test.RedRange, $"RedRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.GreenRange, test.GreenRange, $"GreenRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.BlueRange, test.BlueRange, $"BlueRange differs in type '{T.ColorFormat.Name}'");
        Assert.AreEqual(reference.AlphaRange, test.AlphaRange, $"AlphaRange differs in type '{T.ColorFormat.Name}'");
    }
}