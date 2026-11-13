namespace HPPH.Test.Colors;

/// <summary>
/// Colors that are equal through Equals(IColor) hash equally, whatever their format.
/// </summary>
[TestClass]
public class ColorHashTests
{
    [TestMethod]
    public void EqualColorsHashEquallyAcrossFormats()
    {
        Random random = new(99);
        for (int i = 0; i < 1000; i++)
        {
            byte r = (byte)random.Next(256), g = (byte)random.Next(256), b = (byte)random.Next(256), a = (byte)random.Next(256);

            IColor[] opaque = [new ColorRGB(r, g, b), new ColorBGR(b, g, r), new ColorRGBA(r, g, b, 255), new ColorBGRA(b, g, r, 255), new ColorARGB(255, r, g, b), new ColorABGR(255, b, g, r)];
            IColor[] translucent = [new ColorRGBA(r, g, b, a), new ColorBGRA(b, g, r, a), new ColorARGB(a, r, g, b), new ColorABGR(a, b, g, r)];

            foreach (IColor[] colors in new[] { opaque, translucent })
                foreach (IColor color in colors)
                {
                    Assert.IsTrue(color.Equals(colors[0]), $"{color} equals {colors[0]}");
                    Assert.IsTrue(((object)color).Equals(colors[0]), $"{color} equals {colors[0]} as object");
                    Assert.AreEqual(colors[0].GetHashCode(), color.GetHashCode(), $"{color} hash");
                }

            HashSet<IColor> set = [.. opaque];
            Assert.HasCount(1, set, "one entry for the same color in six formats");
            Assert.IsTrue(set.Contains(new ColorABGR(255, b, g, r)));

            HashSet<object> objectSet = [.. opaque];
            Assert.HasCount(1, objectSet, "one entry for the same color in six formats as object");
        }
    }

    [TestMethod]
    public void DifferentColorsAreNotEqualAcrossFormats()
    {
        object rgba = new ColorRGBA(1, 2, 3, 4);
        Assert.IsFalse(rgba.Equals(new ColorBGRA(3, 2, 1, 5)));
        Assert.IsFalse(rgba.Equals(new ColorRGB(1, 2, 3)));
        Assert.IsTrue(new ColorRGB(1, 2, 3).Equals((object)new ColorRGBA(1, 2, 3, 255)));
        Assert.IsFalse(rgba.Equals("not a color"));
        Assert.IsFalse(rgba.Equals(null));
    }

    [TestMethod]
    public void SameTypeEqualityComparesAllChannels()
    {
        ColorBGRA color = new(1, 2, 3, 4);
        Assert.IsTrue(color.Equals(new ColorBGRA(1, 2, 3, 4)));
        Assert.IsFalse(color.Equals(new ColorBGRA(1, 2, 3, 5)));
        Assert.IsFalse(color.Equals(new ColorBGRA(0, 2, 3, 4)));
        Assert.IsTrue(color == new ColorBGRA(1, 2, 3, 4));
        Assert.IsTrue(color != new ColorBGRA(1, 2, 4, 4));

        ColorRGB rgb = new(1, 2, 3);
        Assert.IsTrue(rgb.Equals(new ColorRGB(1, 2, 3)));
        Assert.IsFalse(rgb.Equals(new ColorRGB(1, 2, 4)));
    }
}
