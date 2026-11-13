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
        where T : unmanaged, IColor<T>
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
        where T : unmanaged, IColor<T>
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
        where T : unmanaged, IColor<T>
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
        where T : unmanaged, IColor<T>
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
        where T : unmanaged, IColor<T>
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

    [TestMethod]
    public void AverageRoundsExactly()
    {
        AverageRoundsExactly<ColorRGB>();
        AverageRoundsExactly<ColorBGR>();
        AverageRoundsExactly<ColorRGBA>();
        AverageRoundsExactly<ColorBGRA>();
        AverageRoundsExactly<ColorARGB>();
        AverageRoundsExactly<ColorABGR>();
    }

    private static void AverageRoundsExactly<T>()
        where T : unmanaged, IColor<T>
    {
        // 32769 pixels of k + 1 and 32768 of k average to k + 0.5000076, which rounds to k + 1.
        // Dividing in float gave exactly k + 0.5 there, which rounded to k for even k.
        foreach (byte k in (byte[])[200, 254])
            AssertAverage<T>([.. Enumerable.Repeat((byte)(k + 1), 32769), .. Enumerable.Repeat(k, 32768)], (byte)(k + 1));

        // Exact ties go to the even value.
        AssertAverage<T>([0, 1], 0);
        AssertAverage<T>([1, 2], 2);
        AssertAverage<T>([0, 0, 1, 1], 0);
        AssertAverage<T>([1, 1, 2, 2], 2);
    }

    private static void AssertAverage<T>(byte[] values, byte expected)
        where T : unmanaged, IColor<T>
    {
        T[] colors = [.. values.Select(value => T.Create(value, value, value, value))];
        T expectedColor = T.Create(expected, expected, expected, expected);
        Image<T> image = Image<T>.Create(colors, colors.Length, 1);
        string name = $"{T.ColorFormat.Name}, {values.Length} px";

        Assert.AreEqual(expectedColor, new ReadOnlySpan<T>(colors).Average(), $"{name}: ReadOnlySpan");
        Assert.AreEqual(expectedColor, image.Average(), $"{name}: Image");
        Assert.AreEqual(expectedColor, image.AsRefImage().Average(), $"{name}: RefImage");
        Assert.AreEqual(expectedColor, ReferencePixelHelper.Average(image.AsRefImage()), $"{name}: Reference");

        IColor fromInterface = ((IImage)image).Average();
        Assert.AreEqual(expectedColor.R, fromInterface.R, $"{name}: IImage R");
        Assert.AreEqual(expectedColor.G, fromInterface.G, $"{name}: IImage G");
        Assert.AreEqual(expectedColor.B, fromInterface.B, $"{name}: IImage B");
        Assert.AreEqual(expectedColor.A, fromInterface.A, $"{name}: IImage A");
    }
}