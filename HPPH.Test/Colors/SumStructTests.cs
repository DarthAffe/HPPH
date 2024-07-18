using System.Runtime.InteropServices;

namespace HPPH.Test.Colors;

[TestClass]
public class SumStructTests
{
    [TestMethod]
    public void SumRGBLayout()
    {
        Span<long> data = [1, 2, 3, 4];
        SumRGB color = MemoryMarshal.Cast<long, SumRGB>(data)[0];

        Assert.AreEqual(data[0], color.R);
        Assert.AreEqual(data[1], color.G);
        Assert.AreEqual(data[2], color.B);
        Assert.AreEqual(data[3], color.A);
    }

    [TestMethod]
    public void SumBGRLayout()
    {
        Span<long> data = [1, 2, 3, 4];
        SumBGR color = MemoryMarshal.Cast<long, SumBGR>(data)[0];

        Assert.AreEqual(data[0], color.B);
        Assert.AreEqual(data[1], color.G);
        Assert.AreEqual(data[2], color.R);
        Assert.AreEqual(data[3], color.A);
    }

    [TestMethod]
    public void SumRGBALayout()
    {
        Span<long> data = [1, 2, 3, 4];
        SumRGBA color = MemoryMarshal.Cast<long, SumRGBA>(data)[0];

        Assert.AreEqual(data[0], color.R);
        Assert.AreEqual(data[1], color.G);
        Assert.AreEqual(data[2], color.B);
        Assert.AreEqual(data[3], color.A);
    }

    [TestMethod]
    public void SumBGRALayout()
    {
        Span<long> data = [1, 2, 3, 4];
        SumBGRA color = MemoryMarshal.Cast<long, SumBGRA>(data)[0];

        Assert.AreEqual(data[0], color.B);
        Assert.AreEqual(data[1], color.G);
        Assert.AreEqual(data[2], color.R);
        Assert.AreEqual(data[3], color.A);
    }

    [TestMethod]
    public void SumARGBLayout()
    {
        Span<long> data = [1, 2, 3, 4];
        SumARGB color = MemoryMarshal.Cast<long, SumARGB>(data)[0];

        Assert.AreEqual(data[0], color.A);
        Assert.AreEqual(data[1], color.R);
        Assert.AreEqual(data[2], color.G);
        Assert.AreEqual(data[3], color.B);
    }

    [TestMethod]
    public void SumABGRLayout()
    {
        Span<long> data = [1, 2, 3, 4];
        SumABGR color = MemoryMarshal.Cast<long, SumABGR>(data)[0];

        Assert.AreEqual(data[0], color.A);
        Assert.AreEqual(data[1], color.B);
        Assert.AreEqual(data[2], color.G);
        Assert.AreEqual(data[3], color.R);
    }


    [TestMethod]
    public void SumRGBCreate()
    {
        ISum color = new SumRGB(1, 2, 3, 4);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(4, color.A);
    }

    [TestMethod]
    public void SumBGRCreate()
    {
        ISum color = new SumBGR(3, 2, 1, 4);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(4, color.A);
    }

    [TestMethod]
    public void SumRGBACreate()
    {
        ISum color = new SumRGBA(1, 2, 3, 4);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(4, color.A);
    }

    [TestMethod]
    public void SumBGRACreate()
    {
        ISum color = new SumBGRA(3, 2, 1, 4);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(4, color.A);
    }

    [TestMethod]
    public void SumARGBCreate()
    {
        ISum color = new SumARGB(4, 1, 2, 3);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(4, color.A);
    }

    [TestMethod]
    public void SumABGRCreate()
    {
        ISum color = new SumABGR(4, 3, 2, 1);

        Assert.AreEqual(1, color.R);
        Assert.AreEqual(2, color.G);
        Assert.AreEqual(3, color.B);
        Assert.AreEqual(4, color.A);
    }


    [TestMethod]
    public void SumRGBToString()
    {
        Assert.AreEqual("[A: 4, R: 1, G: 2, B: 3]", new SumRGB(1, 2, 3, 4).ToString());
    }

    [TestMethod]
    public void SumBGRToString()
    {
        Assert.AreEqual("[A: 4, R: 1, G: 2, B: 3]", new SumBGR(3, 2, 1, 4).ToString());
    }

    [TestMethod]
    public void SumRGBAToString()
    {
        Assert.AreEqual("[A: 4, R: 1, G: 2, B: 3]", new SumRGBA(1, 2, 3, 4).ToString());
    }

    [TestMethod]
    public void SumBGRAToString()
    {
        Assert.AreEqual("[A: 4, R: 1, G: 2, B: 3]", new SumBGRA(3, 2, 1, 4).ToString());
    }

    [TestMethod]
    public void SumARGBToString()
    {
        Assert.AreEqual("[A: 4, R: 1, G: 2, B: 3]", new SumARGB(4, 1, 2, 3).ToString());
    }

    [TestMethod]
    public void SumABGRToString()
    {
        Assert.AreEqual("[A: 4, R: 1, G: 2, B: 3]", new SumABGR(4, 3, 2, 1).ToString());
    }
}