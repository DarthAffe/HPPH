using System.Runtime.InteropServices;
using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class QuantizeTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    private static int[] MedianCutSizes => [0, 1, 2, 16, 64];
    private static int[] WuSizes => [0, 1, 2, 9, 16, 53, 256];

    private static readonly Dictionary<string, Dictionary<Type, Dictionary<int, IColor[]>>> _medianCutReference = [];
    private static readonly Dictionary<string, Dictionary<Type, Dictionary<int, IColor[]>>> _wuReference = [];

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        foreach (string image in GetTestImages())
        {
            _medianCutReference[image] = [];
            _wuReference[image] = [];

            Initialize<ColorRGB>(image);
            Initialize<ColorBGR>(image);
            Initialize<ColorRGBA>(image);
            Initialize<ColorBGRA>(image);
            Initialize<ColorARGB>(image);
            Initialize<ColorABGR>(image);
        }
    }

    private static void Initialize<T>(string image)
        where T : unmanaged, IColor
    {
        _medianCutReference[image][typeof(T)] = [];
        _wuReference[image][typeof(T)] = [];

        Image<T> img = ImageHelper.GetImage<T>(image);

        foreach (int size in MedianCutSizes)
            _medianCutReference[image][typeof(T)][size] = [.. ReferencePixelHelper.CreateMedianCutColorPalette(img, size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        foreach (int size in WuSizes)
            _wuReference[image][typeof(T)][size] = [.. ReferencePixelHelper.CreateWuColorPalette(img, size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];
    }

    [TestMethod]
    public void CreateMedianCutColorPaletteReadOnlySpan()
    {
        ReadOnlySpan<int> sizes = MedianCutSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateMedianCutColorPaletteReadOnlySpan<ColorRGB>(image, size);
                CreateMedianCutColorPaletteReadOnlySpan<ColorBGR>(image, size);
                CreateMedianCutColorPaletteReadOnlySpan<ColorRGBA>(image, size);
                CreateMedianCutColorPaletteReadOnlySpan<ColorBGRA>(image, size);
                CreateMedianCutColorPaletteReadOnlySpan<ColorARGB>(image, size);
                CreateMedianCutColorPaletteReadOnlySpan<ColorABGR>(image, size);
            }
        }
    }

    private void CreateMedianCutColorPaletteReadOnlySpan<T>(string image, int size)
        where T : unmanaged, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        ReadOnlySpan<T> span = data;

        T[] test = [.. span.CreateMedianCutColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _medianCutReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateMedianCutColorPaletteSpan()
    {
        ReadOnlySpan<int> sizes = MedianCutSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateMedianCutColorPaletteSpan<ColorRGB>(image, size);
                CreateMedianCutColorPaletteSpan<ColorBGR>(image, size);
                CreateMedianCutColorPaletteSpan<ColorRGBA>(image, size);
                CreateMedianCutColorPaletteSpan<ColorBGRA>(image, size);
                CreateMedianCutColorPaletteSpan<ColorARGB>(image, size);
                CreateMedianCutColorPaletteSpan<ColorABGR>(image, size);
            }
        }
    }

    private void CreateMedianCutColorPaletteSpan<T>(string image, int size)
        where T : unmanaged, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        Span<T> span = data;

        T[] test = [.. span.CreateMedianCutColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _medianCutReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateMedianCutColorPaletteImage()
    {
        ReadOnlySpan<int> sizes = MedianCutSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateMedianCutColorPaletteImage<ColorRGB>(image, size);
                CreateMedianCutColorPaletteImage<ColorBGR>(image, size);
                CreateMedianCutColorPaletteImage<ColorRGBA>(image, size);
                CreateMedianCutColorPaletteImage<ColorBGRA>(image, size);
                CreateMedianCutColorPaletteImage<ColorARGB>(image, size);
                CreateMedianCutColorPaletteImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateMedianCutColorPaletteImage<T>(string image, int size)
        where T : unmanaged, IColor
    {
        IImage data = ImageHelper.GetImage<T>(image);

        IColor[] test = [.. data.CreateMedianCutColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _medianCutReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateMedianCutColorPaletteGenericImage()
    {
        ReadOnlySpan<int> sizes = MedianCutSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateMedianCutColorPaletteGenericImage<ColorRGB>(image, size);
                CreateMedianCutColorPaletteGenericImage<ColorBGR>(image, size);
                CreateMedianCutColorPaletteGenericImage<ColorRGBA>(image, size);
                CreateMedianCutColorPaletteGenericImage<ColorBGRA>(image, size);
                CreateMedianCutColorPaletteGenericImage<ColorARGB>(image, size);
                CreateMedianCutColorPaletteGenericImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateMedianCutColorPaletteGenericImage<T>(string image, int size)
        where T : unmanaged, IColor
    {
        Image<T> data = ImageHelper.GetImage<T>(image);

        T[] test = [.. data.CreateMedianCutColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _medianCutReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateMedianCutColorPaletteRefImage()
    {
        ReadOnlySpan<int> sizes = MedianCutSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateMedianCutColorPaletteRefImage<ColorRGB>(image, size);
                CreateMedianCutColorPaletteRefImage<ColorBGR>(image, size);
                CreateMedianCutColorPaletteRefImage<ColorRGBA>(image, size);
                CreateMedianCutColorPaletteRefImage<ColorBGRA>(image, size);
                CreateMedianCutColorPaletteRefImage<ColorARGB>(image, size);
                CreateMedianCutColorPaletteRefImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateMedianCutColorPaletteRefImage<T>(string image, int size)
        where T : unmanaged, IColor
    {
        RefImage<T> data = ImageHelper.GetImage<T>(image).AsRefImage();

        T[] test = [.. data.CreateMedianCutColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _medianCutReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void MedianCutPaletteHasNoPadding()
    {
        ColorRGB[] colors = [new(10, 0, 0), new(200, 0, 0)];

        ColorRGB[] palette = new ReadOnlySpan<ColorRGB>(colors).CreateMedianCutColorPalette(4);

        Assert.AreSequenceEqual([new ColorRGB(10, 0, 0), new ColorRGB(200, 0, 0)], palette, SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public void MedianCutPaletteKeepsInput()
    {
        // Small (stack buffer) and large (pooled buffer) inputs.
        foreach (int length in new[] { 4, 5000 })
        {
            ColorRGB[] colors = new ColorRGB[length];
            new Random(length).NextBytes(MemoryMarshal.AsBytes(colors.AsSpan()));
            ColorRGB[] original = [.. colors];

            _ = new ReadOnlySpan<ColorRGB>(colors).CreateMedianCutColorPalette(2);
            Assert.AreSequenceEqual(original, colors, $"ReadOnlySpan, {length} colors");

            _ = new Span<ColorRGB>(colors).CreateMedianCutColorPalette(2);
            Assert.AreSequenceEqual(original, colors, $"Span, {length} colors");

            _ = colors.CreateMedianCutColorPalette(2);
            Assert.AreSequenceEqual(original, colors, $"array, {length} colors");
        }
    }

    [TestMethod]
    public void SpansAndArraysUseTheReadOnlySpanOverloads()
    {
        ColorRGB[] colors = new ColorRGB[5000];
        new Random(9).NextBytes(MemoryMarshal.AsBytes(colors.AsSpan()));
        ReadOnlySpan<ColorRGB> readOnlySpan = colors;

        Assert.AreSequenceEqual(readOnlySpan.CreateMedianCutColorPalette(16), new Span<ColorRGB>(colors).CreateMedianCutColorPalette(16), "median cut, Span");
        Assert.AreSequenceEqual(readOnlySpan.CreateMedianCutColorPalette(16), colors.CreateMedianCutColorPalette(16), "median cut, array");
        Assert.AreSequenceEqual(readOnlySpan.CreateWuColorPalette(16), new Span<ColorRGB>(colors).CreateWuColorPalette(16), "Wu, Span");
        Assert.AreSequenceEqual(readOnlySpan.CreateWuColorPalette(16), colors.CreateWuColorPalette(16), "Wu, array");
        Assert.AreSequenceEqual(readOnlySpan.CreateColorPalette(16), colors.CreateColorPalette(16), "default, array");
    }

    [TestMethod]
    public void MedianCutPaletteOfEmptyInputIsEmpty()
    {
        Assert.IsEmpty(ReadOnlySpan<ColorRGB>.Empty.CreateMedianCutColorPalette(16));
        Assert.ThrowsExactly<ArgumentException>(() => ReadOnlySpan<ColorRGB>.Empty.CreateMedianCutColorPalette(3));
    }

    [TestMethod]
    public void LargeMedianCutPaletteSplitInParallelMatchesReference()
    {
        ColorRGBA[] colors = new ColorRGBA[1 << 20];
        new Random(6).NextBytes(MemoryMarshal.AsBytes(colors.AsSpan()));

        ColorRGBA[] expected = [.. ReferencePixelHelper.CreateMedianCutColorPalette(colors, 16).OrderBy(c => c.R).ThenBy(c => c.G).ThenBy(c => c.B).ThenBy(c => c.A)];
        ColorRGBA[] actual = [.. new ReadOnlySpan<ColorRGBA>(colors).CreateMedianCutColorPalette(16).OrderBy(c => c.R).ThenBy(c => c.G).ThenBy(c => c.B).ThenBy(c => c.A)];

        Assert.AreSequenceEqual(expected, actual);
    }

    [TestMethod]
    public void CreateWuColorPaletteReadOnlySpan()
    {
        ReadOnlySpan<int> sizes = WuSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateWuColorPaletteReadOnlySpan<ColorRGB>(image, size);
                CreateWuColorPaletteReadOnlySpan<ColorBGR>(image, size);
                CreateWuColorPaletteReadOnlySpan<ColorRGBA>(image, size);
                CreateWuColorPaletteReadOnlySpan<ColorBGRA>(image, size);
                CreateWuColorPaletteReadOnlySpan<ColorARGB>(image, size);
                CreateWuColorPaletteReadOnlySpan<ColorABGR>(image, size);
            }
        }
    }

    private void CreateWuColorPaletteReadOnlySpan<T>(string image, int size)
        where T : unmanaged, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        ReadOnlySpan<T> span = data;

        T[] test = [.. span.CreateWuColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _wuReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateWuColorPaletteSpan()
    {
        ReadOnlySpan<int> sizes = WuSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateWuColorPaletteSpan<ColorRGB>(image, size);
                CreateWuColorPaletteSpan<ColorBGR>(image, size);
                CreateWuColorPaletteSpan<ColorRGBA>(image, size);
                CreateWuColorPaletteSpan<ColorBGRA>(image, size);
                CreateWuColorPaletteSpan<ColorARGB>(image, size);
                CreateWuColorPaletteSpan<ColorABGR>(image, size);
            }
        }
    }

    private void CreateWuColorPaletteSpan<T>(string image, int size)
        where T : unmanaged, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        Span<T> span = data;

        T[] test = [.. span.CreateWuColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _wuReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateWuColorPaletteImage()
    {
        ReadOnlySpan<int> sizes = WuSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateWuColorPaletteImage<ColorRGB>(image, size);
                CreateWuColorPaletteImage<ColorBGR>(image, size);
                CreateWuColorPaletteImage<ColorRGBA>(image, size);
                CreateWuColorPaletteImage<ColorBGRA>(image, size);
                CreateWuColorPaletteImage<ColorARGB>(image, size);
                CreateWuColorPaletteImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateWuColorPaletteImage<T>(string image, int size)
        where T : unmanaged, IColor
    {
        IImage data = ImageHelper.GetImage<T>(image);

        IColor[] test = [.. data.CreateWuColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _wuReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateWuColorPaletteGenericImage()
    {
        ReadOnlySpan<int> sizes = WuSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateWuColorPaletteGenericImage<ColorRGB>(image, size);
                CreateWuColorPaletteGenericImage<ColorBGR>(image, size);
                CreateWuColorPaletteGenericImage<ColorRGBA>(image, size);
                CreateWuColorPaletteGenericImage<ColorBGRA>(image, size);
                CreateWuColorPaletteGenericImage<ColorARGB>(image, size);
                CreateWuColorPaletteGenericImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateWuColorPaletteGenericImage<T>(string image, int size)
        where T : unmanaged, IColor
    {
        Image<T> data = ImageHelper.GetImage<T>(image);

        T[] test = [.. data.CreateWuColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _wuReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateWuColorPaletteRefImage()
    {
        ReadOnlySpan<int> sizes = WuSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateWuColorPaletteRefImage<ColorRGB>(image, size);
                CreateWuColorPaletteRefImage<ColorBGR>(image, size);
                CreateWuColorPaletteRefImage<ColorRGBA>(image, size);
                CreateWuColorPaletteRefImage<ColorBGRA>(image, size);
                CreateWuColorPaletteRefImage<ColorARGB>(image, size);
                CreateWuColorPaletteRefImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateWuColorPaletteRefImage<T>(string image, int size)
        where T : unmanaged, IColor
    {
        RefImage<T> data = ImageHelper.GetImage<T>(image).AsRefImage();

        T[] test = [.. data.CreateWuColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _wuReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void DefaultColorPaletteIsWu()
    {
        ColorRGBA[] colors = new ColorRGBA[200 * 100];
        new Random(8).NextBytes(MemoryMarshal.AsBytes(colors.AsSpan()));
        Image<ColorRGBA> image = Image<ColorRGBA>.Create(colors, 200, 100);

        ColorRGBA[] expected = new ReadOnlySpan<ColorRGBA>(colors).CreateWuColorPalette(16);

        Assert.AreSequenceEqual(expected, new ReadOnlySpan<ColorRGBA>(colors).CreateColorPalette(16), "ReadOnlySpan");
        Assert.AreSequenceEqual(expected, new Span<ColorRGBA>(colors).CreateColorPalette(16), "Span");
        Assert.AreSequenceEqual(expected, image.AsRefImage().CreateColorPalette(16), "RefImage");
        Assert.AreSequenceEqual(expected, image.CreateColorPalette(16), "Image<T>");
        Assert.AreSequenceEqual(expected.Cast<IColor>(), ((IImage)image).CreateColorPalette(16), "IImage");
    }

    [TestMethod]
    public void PaletteKeepsInput()
    {
        ColorRGB[] colors = new ColorRGB[5000];
        new Random(1).NextBytes(MemoryMarshal.AsBytes(colors.AsSpan()));
        ColorRGB[] original = [.. colors];

        _ = new ReadOnlySpan<ColorRGB>(colors).CreateWuColorPalette(16);
        Assert.AreSequenceEqual(original, colors);

        _ = new Span<ColorRGB>(colors).CreateWuColorPalette(16);
        Assert.AreSequenceEqual(original, colors);

        _ = colors.CreateWuColorPalette(16);
        Assert.AreSequenceEqual(original, colors);
    }

    [TestMethod]
    public void NegativePaletteSizeThrows()
    {
        ColorRGB[] colors = [new(1, 2, 3)];

        Assert.ThrowsExactly<ArgumentException>(() => new ReadOnlySpan<ColorRGB>(colors).CreateWuColorPalette(-1));
        Assert.ThrowsExactly<ArgumentException>(() => Image<ColorRGB>.Create(colors, 1, 1).CreateWuColorPalette(-1));
    }

    [TestMethod]
    public void EmptyInputGivesEmptyPalette()
        => Assert.IsEmpty(ReadOnlySpan<ColorRGB>.Empty.CreateWuColorPalette(16));

    [TestMethod]
    public void PaletteOfFewDistinctColorsHasNoPadding()
    {
        ColorRGB[] distinct = [new(10, 0, 0), new(200, 0, 0), new(0, 120, 255)];
        ColorRGB[] colors = [.. Enumerable.Range(0, 1000).Select(i => distinct[i % distinct.Length])];

        ColorRGB[] palette = new ReadOnlySpan<ColorRGB>(colors).CreateWuColorPalette(16);

        Assert.AreSequenceEqual(distinct, palette, SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public void PaletteOfFewPixelsIsExact()
    {
        // At most paletteSize pixels: the distinct colors are returned, even if they share a histogram cell.
        ColorRGB[] colors = [new(10, 0, 0), new(12, 0, 0), new(10, 0, 0)];

        ColorRGB[] palette = new ReadOnlySpan<ColorRGB>(colors).CreateWuColorPalette(4);

        Assert.AreSequenceEqual([new ColorRGB(10, 0, 0), new ColorRGB(12, 0, 0)], palette);
    }

    [TestMethod]
    public void ColorsInOneCellAreMerged()
    {
        // Documents the 5 bit grid: both colors land in the same cell and can't be separated.
        ColorRGB[] colors = [.. Enumerable.Range(0, 10).Select(i => (i % 2) == 0 ? new ColorRGB(10, 0, 0) : new ColorRGB(12, 0, 0))];

        ColorRGB[] palette = new ReadOnlySpan<ColorRGB>(colors).CreateWuColorPalette(4);

        Assert.AreSequenceEqual([new ColorRGB(11, 0, 0)], palette);
    }

    [TestMethod]
    public void PaletteOfSizeOneIsTheAverage()
    {
        ColorRGBA[] colors = new ColorRGBA[10_000];
        new Random(2).NextBytes(MemoryMarshal.AsBytes(colors.AsSpan()));

        ColorRGBA[] palette = new ReadOnlySpan<ColorRGBA>(colors).CreateWuColorPalette(1);

        Assert.AreSequenceEqual([new ReadOnlySpan<ColorRGBA>(colors).Average()], palette);
    }

    [TestMethod]
    public void PaletteAveragesAlpha()
    {
        ColorRGBA[] colors = [.. Enumerable.Range(0, 10).Select(i => new ColorRGBA(100, 100, 100, (byte)((i % 2) == 0 ? 0 : 100)))];

        ColorRGBA[] palette = new ReadOnlySpan<ColorRGBA>(colors).CreateWuColorPalette(4);

        Assert.AreSequenceEqual([new ColorRGBA(100, 100, 100, 50)], palette);
    }

    [TestMethod]
    public void HugePaletteSizeIsLimitedByTheHistogram()
    {
        ColorRGB[] colors = new ColorRGB[1 << 18];
        new Random(3).NextBytes(MemoryMarshal.AsBytes(colors.AsSpan()));

        // Smaller than the amount of pixels (no distinct colors shortcut), but larger than the amount of histogram cells.
        ColorRGB[] palette = new ReadOnlySpan<ColorRGB>(colors).CreateWuColorPalette(100_000);

        Assert.IsLessThanOrEqualTo(32768, palette.Length);
        Assert.IsGreaterThan(30000, palette.Length);
        Assert.HasCount(palette.Length, palette.Distinct());
    }

    [TestMethod]
    public void MedianCutMergesDuplicateColors()
    {
        ColorRGB red = new(255, 0, 0);
        ColorRGB blue = new(0, 0, 255);

        ColorRGB[] colors = new ColorRGB[64];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = (i % 2) == 0 ? red : blue;

        ColorRGB[] palette = colors.CreateMedianCutColorPalette(8);

        Assert.HasCount(2, palette);
        Assert.Contains(red, palette);
        Assert.Contains(blue, palette);
    }

    [TestMethod]
    public void PaletteIsOrderedByPopulation()
    {
        ColorRGB red = new(255, 0, 0);
        ColorRGB green = new(0, 255, 0);
        ColorRGB blue = new(0, 0, 255);

        // Least common first, so the order isn't just the order of appearance.
        ColorRGB[] colors = [.. Enumerable.Repeat(blue, 2), .. Enumerable.Repeat(green, 6), .. Enumerable.Repeat(red, 8)];
        ColorRGB[] expected = [red, green, blue];

        Assert.AreSequenceEqual(expected, colors.CreateWuColorPalette(8), "Wu");
        Assert.AreSequenceEqual(expected, colors.CreateMedianCutColorPalette(8), "MedianCut");
        Assert.AreSequenceEqual(expected, colors.CreateColorPalette(8), "Default");

        // Fewer pixels than the palette size takes the distinct colors shortcut.
        Assert.AreSequenceEqual(expected, colors.CreateWuColorPalette(32), "Wu, distinct colors");
        Assert.AreSequenceEqual(expected, colors.CreateMedianCutColorPalette(32), "MedianCut, distinct colors");
    }

    [TestMethod]
    public void PaletteLargerThan256MatchesReference()
    {
        ColorRGB[] colors = new ColorRGB[8000];
        new Random(7).NextBytes(MemoryMarshal.AsBytes(colors.AsSpan()));

        foreach (int size in new[] { 257, 600 })
            Assert.AreSequenceEqual(ReferencePixelHelper.CreateWuColorPalette<ColorRGB>(colors, size), new ReadOnlySpan<ColorRGB>(colors).CreateWuColorPalette(size), $"size {size}");
    }

    [TestMethod]
    public void PaletteOfRegionMatchesCopiedRegion()
    {
        const int WIDTH = 300;
        const int HEIGHT = 200;
        const int STRIDE = (WIDTH * 4) + 20;

        byte[] buffer = new byte[STRIDE * HEIGHT];
        new Random(4).NextBytes(buffer);

        RefImage<ColorBGRA> region = Image<ColorBGRA>.Wrap(buffer, WIDTH, HEIGHT, STRIDE).AsRefImage()[17, 9, 250, 150];
        ColorBGRA[] copy = region.ToArray();

        foreach (int size in new[] { 3, 16, 100 })
        {
            Assert.AreSequenceEqual(new ReadOnlySpan<ColorBGRA>(copy).CreateWuColorPalette(size), region.CreateWuColorPalette(size), $"size {size}");
            Assert.AreSequenceEqual(ReferencePixelHelper.CreateWuColorPalette(region, size), region.CreateWuColorPalette(size), $"reference, size {size}");
        }
    }

    [TestMethod]
    public void LargePaletteHistogramInParallelMatchesReference()
    {
        ColorRGBA[] colors = new ColorRGBA[1 << 21];
        new Random(5).NextBytes(MemoryMarshal.AsBytes(colors.AsSpan()));

        foreach (int size in new[] { 16, 255 })
            Assert.AreSequenceEqual(ReferencePixelHelper.CreateWuColorPalette<ColorRGBA>(colors, size), new ReadOnlySpan<ColorRGBA>(colors).CreateWuColorPalette(size), $"size {size}");
    }

    [TestMethod]
    public void LargeRegionHistogramInParallelMatchesReference()
    {
        const int WIDTH = 2000;
        const int HEIGHT = 1100;
        const int STRIDE = (WIDTH * 3) + 7;

        byte[] buffer = new byte[STRIDE * HEIGHT];
        new Random(6).NextBytes(buffer);

        RefImage<ColorRGB> region = Image<ColorRGB>.Wrap(buffer, WIDTH, HEIGHT, STRIDE).AsRefImage()[3, 1, 1990, 1090];

        Assert.AreSequenceEqual(ReferencePixelHelper.CreateWuColorPalette(region, 64), region.CreateWuColorPalette(64));
    }

    [TestMethod]
    public void PalettesOfRandomImagesAndRegionsMatchReference()
    {
        Random random = new(815);

        FuzzPalette<ColorRGB>(random);
        FuzzPalette<ColorBGR>(random);
        FuzzPalette<ColorRGBA>(random);
        FuzzPalette<ColorARGB>(random);
    }

    private static void FuzzPalette<T>(Random random)
        where T : unmanaged, IColor
    {
        int bpp = T.ColorFormat.BytesPerPixel;

        for (int iteration = 0; iteration < 100; iteration++)
        {
            int width = random.Next(1, 120);
            int height = random.Next(1, 40);
            int stride = (width * bpp) + random.Next(0, 20);

            byte[] buffer = new byte[((height - 1) * stride) + (width * bpp)];
            random.NextBytes(buffer);

            // Narrow value ranges in some images, so many colors share cells.
            if (random.Next(2) == 0)
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = (byte)(100 + (buffer[i] % 20));

            Image<T> image = Image<T>.Wrap(buffer, width, height, stride);
            int size = random.Next(1, 65);

            string name = $"{T.ColorFormat.Name} {width}x{height} stride {stride}, size {size}";
            Assert.AreSequenceEqual(ReferencePixelHelper.CreateWuColorPalette(image, size), image.CreateWuColorPalette(size), name);

            int x = random.Next(0, width);
            int y = random.Next(0, height);
            RefImage<T> region = image.AsRefImage()[x, y, random.Next(1, (width - x) + 1), random.Next(1, (height - y) + 1)];

            Assert.AreSequenceEqual(ReferencePixelHelper.CreateWuColorPalette(region, size), region.CreateWuColorPalette(size), $"{name}, region {x},{y} {region.Width}x{region.Height}");
        }
    }
}
