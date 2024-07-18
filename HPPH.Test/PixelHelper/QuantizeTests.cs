using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class CreateColorPaletteTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void CreateColorPaletteReadOnlySpan3ByteSize1()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.GetColorsFromImage<ColorRGB>(image);
            Span<ColorRGB> span = data;

            ColorRGB[] reference = [.. ReferencePixelHelper.CreateColorPalette(span, 1).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];
            ColorRGB[] test = [.. span.CreateColorPalette(1).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

            Assert.AreEqual(reference.Length, test.Length, "Palette Size differs");

            for (int i = 0; i < reference.Length; i++)
                Assert.AreEqual(reference[i], test[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void CreateColorPaletteReadOnlySpan3ByteSize4()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.GetColorsFromImage<ColorRGB>(image);
            Span<ColorRGB> span = data;

            ColorRGB[] reference = [.. ReferencePixelHelper.CreateColorPalette(span, 2).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];
            ColorRGB[] test = [.. span.CreateColorPalette(2).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

            Assert.AreEqual(reference.Length, test.Length, "Palette Size differs");

            for (int i = 0; i < reference.Length; i++)
                Assert.AreEqual(reference[i], test[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void CreateColorPaletteReadOnlySpan3ByteSize16()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGB[] data = ImageHelper.GetColorsFromImage<ColorRGB>(image);
            Span<ColorRGB> span = data;

            ColorRGB[] reference = [.. ReferencePixelHelper.CreateColorPalette(span, 16).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];
            ColorRGB[] test = [.. span.CreateColorPalette(16).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

            Assert.AreEqual(reference.Length, test.Length, "Palette Size differs");

            for (int i = 0; i < reference.Length; i++)
                Assert.AreEqual(reference[i], test[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void CreateColorPaletteReadOnlySpan4ByteSize1()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] data = ImageHelper.GetColorsFromImage<ColorRGBA>(image);
            Span<ColorRGBA> span = data;

            ColorRGBA[] reference = [.. ReferencePixelHelper.CreateColorPalette(span, 1).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];
            ColorRGBA[] test = [.. span.CreateColorPalette(1).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

            Assert.AreEqual(reference.Length, test.Length, "Palette Size differs");

            for (int i = 0; i < reference.Length; i++)
                Assert.AreEqual(reference[i], test[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void CreateColorPaletteReadOnlySpan4ByteSize4()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] data = ImageHelper.GetColorsFromImage<ColorRGBA>(image);
            Span<ColorRGBA> span = data;

            ColorRGBA[] reference = [.. ReferencePixelHelper.CreateColorPalette(span, 2).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];
            ColorRGBA[] test = [.. span.CreateColorPalette(2).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

            Assert.AreEqual(reference.Length, test.Length, "Palette Size differs");

            for (int i = 0; i < reference.Length; i++)
                Assert.AreEqual(reference[i], test[i], $"Index {i} differs");
        }
    }

    [TestMethod]
    public void CreateColorPaletteReadOnlySpan4ByteSize16()
    {
        foreach (string image in GetTestImages())
        {
            ColorRGBA[] data = ImageHelper.GetColorsFromImage<ColorRGBA>(image);
            Span<ColorRGBA> span = data;

            ColorRGBA[] reference = [.. ReferencePixelHelper.CreateColorPalette(span, 16).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];
            ColorRGBA[] test = [.. span.CreateColorPalette(16).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

            Assert.AreEqual(reference.Length, test.Length, "Palette Size differs");

            for (int i = 0; i < reference.Length; i++)
                Assert.AreEqual(reference[i], test[i], $"Index {i} differs");
        }
    }
}