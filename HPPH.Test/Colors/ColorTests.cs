using System.Runtime.InteropServices;

namespace HPPH.Test.Colors;

[TestClass]
public class ColorTests
{
    [TestMethod]
    public void ColorRGBLayout()
    {
        Span<byte> data = [1, 2, 3, 4];
        ColorRGB color = MemoryMarshal.Cast<byte, ColorRGB>(data)[0];

        Assert.AreEqual(data[0], color.R);
        Assert.AreEqual(data[1], color.G);
        Assert.AreEqual(data[2], color.B);
        Assert.AreEqual(byte.MaxValue, color.A);
    }

    [TestMethod]
    public void ColorBGRLayout()
    {
        Span<byte> data = [1, 2, 3, 4];
        ColorBGR color = MemoryMarshal.Cast<byte, ColorBGR>(data)[0];

        Assert.AreEqual(data[0], color.B);
        Assert.AreEqual(data[1], color.G);
        Assert.AreEqual(data[2], color.R);
        Assert.AreEqual(byte.MaxValue, color.A);
    }

    [TestMethod]
    public void ColorRGBALayout()
    {
        Span<byte> data = [1, 2, 3, 4];
        ColorRGBA color = MemoryMarshal.Cast<byte, ColorRGBA>(data)[0];

        Assert.AreEqual(data[0], color.R);
        Assert.AreEqual(data[1], color.G);
        Assert.AreEqual(data[2], color.B);
        Assert.AreEqual(data[3], color.A);
    }

    [TestMethod]
    public void ColorBGRALayout()
    {
        Span<byte> data = [1, 2, 3, 4];
        ColorBGRA color = MemoryMarshal.Cast<byte, ColorBGRA>(data)[0];

        Assert.AreEqual(data[0], color.B);
        Assert.AreEqual(data[1], color.G);
        Assert.AreEqual(data[2], color.R);
        Assert.AreEqual(data[3], color.A);
    }

    [TestMethod]
    public void ColorARGBLayout()
    {
        Span<byte> data = [1, 2, 3, 4];
        ColorARGB color = MemoryMarshal.Cast<byte, ColorARGB>(data)[0];

        Assert.AreEqual(data[0], color.A);
        Assert.AreEqual(data[1], color.R);
        Assert.AreEqual(data[2], color.G);
        Assert.AreEqual(data[3], color.B);
    }

    [TestMethod]
    public void ColorABGRLayout()
    {
        Span<byte> data = [1, 2, 3, 4];
        ColorABGR color = MemoryMarshal.Cast<byte, ColorABGR>(data)[0];

        Assert.AreEqual(data[0], color.A);
        Assert.AreEqual(data[1], color.B);
        Assert.AreEqual(data[2], color.G);
        Assert.AreEqual(data[3], color.R);
    }


    [TestMethod]
    public void ColorRGBCreate()
    {
        IColor color = ColorRGB.Create(1, 2, 3, 4);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(byte.MaxValue, color.A);
    }

    [TestMethod]
    public void ColorBGRCreate()
    {
        IColor color = ColorBGR.Create(1, 2, 3, 4);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(byte.MaxValue, color.A);
    }

    [TestMethod]
    public void ColorRGBACreate()
    {
        IColor color = ColorRGBA.Create(1, 2, 3, 4);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(4, color.A);
    }

    [TestMethod]
    public void ColorBGRACreate()
    {
        IColor color = ColorBGRA.Create(1, 2, 3, 4);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(4, color.A);
    }

    [TestMethod]
    public void ColorARGBCreate()
    {
        IColor color = ColorARGB.Create(1, 2, 3, 4);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(4, color.A);
    }

    [TestMethod]
    public void ColorABGRCreate()
    {
        IColor color = ColorABGR.Create(1, 2, 3, 4);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(4, color.A);
    }


    [TestMethod]
    public void ColorRGBToString()
    {
        Assert.AreEqual("[A: 255, R: 1, G: 2, B: 3]", new ColorRGB(1, 2, 3).ToString());
    }

    [TestMethod]
    public void ColorBGRToString()
    {
        Assert.AreEqual("[A: 255, R: 1, G: 2, B: 3]", new ColorBGR(3, 2, 1).ToString());
    }

    [TestMethod]
    public void ColorRGBAToString()
    {
        Assert.AreEqual("[A: 4, R: 1, G: 2, B: 3]", new ColorRGBA(1, 2, 3, 4).ToString());
    }

    [TestMethod]
    public void ColorBGRAToString()
    {
        Assert.AreEqual("[A: 4, R: 1, G: 2, B: 3]", new ColorBGRA(3, 2, 1, 4).ToString());
    }

    [TestMethod]
    public void ColorARGBToString()
    {
        Assert.AreEqual("[A: 4, R: 1, G: 2, B: 3]", new ColorARGB(4, 1, 2, 3).ToString());
    }

    [TestMethod]
    public void ColorABGRToString()
    {
        Assert.AreEqual("[A: 4, R: 1, G: 2, B: 3]", new ColorABGR(4, 3, 2, 1).ToString());
    }
}