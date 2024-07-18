namespace HPPH.Test.Colors;

[TestClass]
public class ColorFormatTests
{
    [TestMethod]
    public void ColorFormatRGBName()
    {
        Assert.AreEqual("RGB", IColorFormat.RGB.Name);
        Assert.AreEqual("RGB", ColorRGB.ColorFormat.Name);
    }

    [TestMethod]
    public void ColorFormatBGRName()
    {
        Assert.AreEqual("BGR", IColorFormat.BGR.Name);
        Assert.AreEqual("BGR", ColorBGR.ColorFormat.Name);
    }

    [TestMethod]
    public void ColorFormatRGBAName()
    {
        Assert.AreEqual("RGBA", IColorFormat.RGBA.Name);
        Assert.AreEqual("RGBA", ColorRGBA.ColorFormat.Name);
    }

    [TestMethod]
    public void ColorFormatBGRAName()
    {
        Assert.AreEqual("BGRA", IColorFormat.BGRA.Name);
        Assert.AreEqual("BGRA", ColorBGRA.ColorFormat.Name);
    }

    [TestMethod]
    public void ColorFormatARGBName()
    {
        Assert.AreEqual("ARGB", IColorFormat.ARGB.Name);
        Assert.AreEqual("ARGB", ColorARGB.ColorFormat.Name);
    }

    [TestMethod]
    public void ColorFormatABGRName()
    {
        Assert.AreEqual("ABGR", IColorFormat.ABGR.Name);
        Assert.AreEqual("ABGR", ColorABGR.ColorFormat.Name);
    }


    [TestMethod]
    public void ColorFormatRGBBPP()
    {
        Assert.AreEqual(3, IColorFormat.RGB.BytesPerPixel);
        Assert.AreEqual(3, ColorRGB.ColorFormat.BytesPerPixel);
    }

    [TestMethod]
    public void ColorFormatBGRBPP()
    {
        Assert.AreEqual(3, IColorFormat.BGR.BytesPerPixel);
        Assert.AreEqual(3, ColorBGR.ColorFormat.BytesPerPixel);
    }

    [TestMethod]
    public void ColorFormatRGBABPP()
    {
        Assert.AreEqual(4, IColorFormat.RGBA.BytesPerPixel);
        Assert.AreEqual(4, ColorRGBA.ColorFormat.BytesPerPixel);
    }

    [TestMethod]
    public void ColorFormatBGRABPP()
    {
        Assert.AreEqual(4, IColorFormat.BGRA.BytesPerPixel);
        Assert.AreEqual(4, ColorBGRA.ColorFormat.BytesPerPixel);
    }

    [TestMethod]
    public void ColorFormatARGBBPP()
    {
        Assert.AreEqual(4, IColorFormat.ARGB.BytesPerPixel);
        Assert.AreEqual(4, ColorARGB.ColorFormat.BytesPerPixel);
    }

    [TestMethod]
    public void ColorFormatABGRBPP()
    {
        Assert.AreEqual(4, IColorFormat.ABGR.BytesPerPixel);
        Assert.AreEqual(4, ColorABGR.ColorFormat.BytesPerPixel);
    }
}