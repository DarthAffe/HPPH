using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class CreateColorPaletteTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    private static int[] Sizes => [0, 1, 2, 16, 64];

    private static readonly Dictionary<string, Dictionary<Type, Dictionary<int, IColor[]>>> _reference = [];

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        ReadOnlySpan<int> sizes = Sizes;
        foreach (string image in GetTestImages())
        {
            _reference[image] = [];

            Initialize<ColorRGB>(image, sizes);
            Initialize<ColorBGR>(image, sizes);
            Initialize<ColorRGBA>(image, sizes);
            Initialize<ColorBGRA>(image, sizes);
            Initialize<ColorARGB>(image, sizes);
            Initialize<ColorABGR>(image, sizes);
        }
    }

    private static void Initialize<T>(string image, ReadOnlySpan<int> sizes)
        where T : unmanaged, IColor
    {
        _reference[image][typeof(T)] = [];

        Image<T> img = ImageHelper.GetImage<T>(image);
        foreach (int size in sizes)
            _reference[image][typeof(T)][size] = [.. ReferencePixelHelper.CreateColorPalette(img, size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];
    }

    [TestMethod]
    public void CreateColorPaletteReadOnlySpan()
    {
        ReadOnlySpan<int> sizes = Sizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateColorPaletteReadOnlySpan<ColorRGB>(image, size);
                CreateColorPaletteReadOnlySpan<ColorBGR>(image, size);
                CreateColorPaletteReadOnlySpan<ColorRGBA>(image, size);
                CreateColorPaletteReadOnlySpan<ColorBGRA>(image, size);
                CreateColorPaletteReadOnlySpan<ColorARGB>(image, size);
                CreateColorPaletteReadOnlySpan<ColorABGR>(image, size);
            }
        }
    }

    private void CreateColorPaletteReadOnlySpan<T>(string image, int size)
        where T : unmanaged, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        ReadOnlySpan<T> span = data;

        T[] test = [.. span.CreateColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _reference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateColorPaletteSpan()
    {
        ReadOnlySpan<int> sizes = Sizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateColorPaletteSpan<ColorRGB>(image, size);
                CreateColorPaletteSpan<ColorBGR>(image, size);
                CreateColorPaletteSpan<ColorRGBA>(image, size);
                CreateColorPaletteSpan<ColorBGRA>(image, size);
                CreateColorPaletteSpan<ColorARGB>(image, size);
                CreateColorPaletteSpan<ColorABGR>(image, size);
            }
        }
    }

    private void CreateColorPaletteSpan<T>(string image, int size)
        where T : unmanaged, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        Span<T> span = data;

        T[] test = [.. span.CreateColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _reference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateColorPaletteImage()
    {
        ReadOnlySpan<int> sizes = Sizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateColorPaletteImage<ColorRGB>(image, size);
                CreateColorPaletteImage<ColorBGR>(image, size);
                CreateColorPaletteImage<ColorRGBA>(image, size);
                CreateColorPaletteImage<ColorBGRA>(image, size);
                CreateColorPaletteImage<ColorARGB>(image, size);
                CreateColorPaletteImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateColorPaletteImage<T>(string image, int size)
        where T : struct, IColor
    {
        IImage data = ImageHelper.GetImage<T>(image);

        IColor[] test = [.. data.CreateColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _reference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateColorPaletteGenericImage()
    {
        ReadOnlySpan<int> sizes = Sizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateColorPaletteGenericImage<ColorRGB>(image, size);
                CreateColorPaletteGenericImage<ColorBGR>(image, size);
                CreateColorPaletteGenericImage<ColorRGBA>(image, size);
                CreateColorPaletteGenericImage<ColorBGRA>(image, size);
                CreateColorPaletteGenericImage<ColorARGB>(image, size);
                CreateColorPaletteGenericImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateColorPaletteGenericImage<T>(string image, int size)
        where T : unmanaged, IColor
    {
        Image<T> data = ImageHelper.GetImage<T>(image);

        T[] test = [.. data.CreateColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _reference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateColorPaletteRefImage()
    {
        ReadOnlySpan<int> sizes = Sizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateColorPaletteRefImage<ColorRGB>(image, size);
                CreateColorPaletteRefImage<ColorBGR>(image, size);
                CreateColorPaletteRefImage<ColorRGBA>(image, size);
                CreateColorPaletteRefImage<ColorBGRA>(image, size);
                CreateColorPaletteRefImage<ColorARGB>(image, size);
                CreateColorPaletteRefImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateColorPaletteRefImage<T>(string image, int size)
        where T : unmanaged, IColor
    {
        RefImage<T> data = ImageHelper.GetImage<T>(image).AsRefImage();

        T[] test = [.. data.CreateColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _reference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }
}