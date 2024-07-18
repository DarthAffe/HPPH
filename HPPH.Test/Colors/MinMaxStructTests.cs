using System.Runtime.InteropServices;

namespace HPPH.Test.Colors;

[TestClass]
public class MinMaxStructTests
{
    [TestMethod]
    public void MinMaxRGBCreate()
    {
        IMinMax color = new MinMaxRGB(1, 3, 4, 7, 16, 31);

        Assert.AreEqual(1, color.RedMin);
        Assert.AreEqual(3, color.RedMax);
        Assert.AreEqual(2, color.RedRange);

        Assert.AreEqual(4, color.GreenMin);
        Assert.AreEqual(7, color.GreenMax);
        Assert.AreEqual(3, color.GreenRange);

        Assert.AreEqual(16, color.BlueMin);
        Assert.AreEqual(31, color.BlueMax);
        Assert.AreEqual(15, color.BlueRange);

        Assert.AreEqual(byte.MaxValue, color.AlphaMin);
        Assert.AreEqual(byte.MaxValue, color.AlphaMax);
        Assert.AreEqual(0, color.AlphaRange);
    }

    [TestMethod]
    public void MinMaxBGRCreate()
    {
        IMinMax color = new MinMaxBGR(1, 3, 4, 7, 16, 31);
        
        Assert.AreEqual(1, color.BlueMin);
        Assert.AreEqual(3, color.BlueMax);
        Assert.AreEqual(2, color.BlueRange);

        Assert.AreEqual(4, color.GreenMin);
        Assert.AreEqual(7, color.GreenMax);
        Assert.AreEqual(3, color.GreenRange);

        Assert.AreEqual(16, color.RedMin);
        Assert.AreEqual(31, color.RedMax);
        Assert.AreEqual(15, color.RedRange);

        Assert.AreEqual(byte.MaxValue, color.AlphaMin);
        Assert.AreEqual(byte.MaxValue, color.AlphaMax);
        Assert.AreEqual(0, color.AlphaRange);
    }

    [TestMethod]
    public void MinMaxRGBACreate()
    {
        IMinMax color = new MinMaxRGBA(1, 3, 4, 7, 16, 31, 64, 127);

        Assert.AreEqual(1, color.RedMin);
        Assert.AreEqual(3, color.RedMax);
        Assert.AreEqual(2, color.RedRange);

        Assert.AreEqual(4, color.GreenMin);
        Assert.AreEqual(7, color.GreenMax);
        Assert.AreEqual(3, color.GreenRange);

        Assert.AreEqual(16, color.BlueMin);
        Assert.AreEqual(31, color.BlueMax);
        Assert.AreEqual(15, color.BlueRange);

        Assert.AreEqual(64, color.AlphaMin);
        Assert.AreEqual(127, color.AlphaMax);
        Assert.AreEqual(63, color.AlphaRange);
    }

    [TestMethod]
    public void MinMaxBGRACreate()
    {
        IMinMax color = new MinMaxBGRA(1, 3, 4, 7, 16, 31, 64, 127);

        Assert.AreEqual(1, color.BlueMin);
        Assert.AreEqual(3, color.BlueMax);
        Assert.AreEqual(2, color.BlueRange);

        Assert.AreEqual(4, color.GreenMin);
        Assert.AreEqual(7, color.GreenMax);
        Assert.AreEqual(3, color.GreenRange);

        Assert.AreEqual(16, color.RedMin);
        Assert.AreEqual(31, color.RedMax);
        Assert.AreEqual(15, color.RedRange);

        Assert.AreEqual(64, color.AlphaMin);
        Assert.AreEqual(127, color.AlphaMax);
        Assert.AreEqual(63, color.AlphaRange);
    }

    [TestMethod]
    public void MinMaxARGBCreate()
    {
        IMinMax color = new MinMaxARGB(1, 3, 4, 7, 16, 31, 64, 127);

        Assert.AreEqual(1, color.AlphaMin);
        Assert.AreEqual(3, color.AlphaMax);
        Assert.AreEqual(2, color.AlphaRange);

        Assert.AreEqual(4, color.RedMin);
        Assert.AreEqual(7, color.RedMax);
        Assert.AreEqual(3, color.RedRange);

        Assert.AreEqual(16, color.GreenMin);
        Assert.AreEqual(31, color.GreenMax);
        Assert.AreEqual(15, color.GreenRange);

        Assert.AreEqual(64, color.BlueMin);
        Assert.AreEqual(127, color.BlueMax);
        Assert.AreEqual(63, color.BlueRange);
    }

    [TestMethod]
    public void MinMaxABGRCreate()
    {
        IMinMax color = new MinMaxABGR(1, 3, 4, 7, 16, 31, 64, 127);

        Assert.AreEqual(1, color.AlphaMin);
        Assert.AreEqual(3, color.AlphaMax);
        Assert.AreEqual(2, color.AlphaRange);

        Assert.AreEqual(4, color.BlueMin);
        Assert.AreEqual(7, color.BlueMax);
        Assert.AreEqual(3, color.BlueRange);

        Assert.AreEqual(16, color.GreenMin);
        Assert.AreEqual(31, color.GreenMax);
        Assert.AreEqual(15, color.GreenRange);

        Assert.AreEqual(64, color.RedMin);
        Assert.AreEqual(127, color.RedMax);
        Assert.AreEqual(63, color.RedRange);
    }


    [TestMethod]
    public void MinMaxRGBToString()
    {
        Assert.AreEqual("[A: 255-255, R: 1-3, G: 4-7, B: 16-31]", new MinMaxRGB(1, 3, 4, 7, 16, 31).ToString());
    }

    [TestMethod]
    public void MinMaxBGRToString()
    {
        Assert.AreEqual("[A: 255-255, R: 16-31, G: 4-7, B: 1-3]", new MinMaxBGR(1, 3, 4, 7, 16, 31).ToString());
    }

    [TestMethod]
    public void MinMaxRGBAToString()
    {
        Assert.AreEqual("[A: 64-127, R: 1-3, G: 4-7, B: 16-31]", new MinMaxRGBA(1, 3, 4, 7, 16, 31, 64, 127).ToString());
    }

    [TestMethod]
    public void MinMaxBGRAToString()
    {
        Assert.AreEqual("[A: 64-127, R: 16-31, G: 4-7, B: 1-3]", new MinMaxBGRA(1, 3, 4, 7, 16, 31, 64, 127).ToString());
    }

    [TestMethod]
    public void MinMaxARGBToString()
    {
        Assert.AreEqual("[A: 1-3, R: 4-7, G: 16-31, B: 64-127]", new MinMaxARGB(1, 3, 4, 7, 16, 31, 64, 127).ToString());
    }

    [TestMethod]
    public void MinMaxABGRToString()
    {
        Assert.AreEqual("[A: 1-3, R: 64-127, G: 16-31, B: 4-7]", new MinMaxABGR(1, 3, 4, 7, 16, 31, 64, 127).ToString());
    }
}