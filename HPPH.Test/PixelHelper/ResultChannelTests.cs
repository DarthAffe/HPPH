namespace HPPH.Test.PixelHelper;

/// <summary>
/// ColorSum and ColorMinMax map the byte order of every format to the right channels,
/// for spans, images and non-generic images.
/// </summary>
[TestClass]
public class ResultChannelTests
{
    [TestMethod]
    public void ChannelsArePlacedByFormat()
    {
        Check<ColorRGB>();
        Check<ColorBGR>();
        Check<ColorRGBA>();
        Check<ColorBGRA>();
        Check<ColorARGB>();
        Check<ColorABGR>();
    }

    private static void Check<T>()
        where T : unmanaged, IColor<T>
    {
        string name = T.ColorFormat.Name;
        bool hasAlpha = T.ColorFormat.BytesPerPixel == 4;

        T[] pixels = [T.Create(10, 20, 30, 40), T.Create(11, 22, 33, 44)];
        byte alphaMin = hasAlpha ? (byte)40 : byte.MaxValue;
        byte alphaMax = hasAlpha ? (byte)44 : byte.MaxValue;

        ColorSum expectedSum = new(21, 42, 63, alphaMin + alphaMax);
        ColorMinMax expectedMinMax = new(10, 11, 20, 22, 30, 33, alphaMin, alphaMax);

        Image<T> image = Image<T>.Create(pixels, 2, 1);

        Assert.AreEqual(expectedSum, new ReadOnlySpan<T>(pixels).Sum(), $"{name}: span Sum");
        Assert.AreEqual(expectedSum, image.Sum(), $"{name}: image Sum");
        Assert.AreEqual(expectedSum, ((IImage)image).Sum(), $"{name}: IImage Sum");

        Assert.AreEqual(expectedMinMax, new ReadOnlySpan<T>(pixels).MinMax(), $"{name}: span MinMax");
        Assert.AreEqual(expectedMinMax, image.MinMax(), $"{name}: image MinMax");
        Assert.AreEqual(expectedMinMax, ((IImage)image).MinMax(), $"{name}: IImage MinMax");

        Assert.AreEqual(1, expectedMinMax.RedRange);
        Assert.AreEqual(hasAlpha ? 4 : 0, expectedMinMax.AlphaRange);

        IColor average = ((IImage)image).Average();
        Assert.IsInstanceOfType<T>(average, name);
        Assert.AreEqual(image.Average(), (T)average, $"{name}: IImage Average");
    }
}
