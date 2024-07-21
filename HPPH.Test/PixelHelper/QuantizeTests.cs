using HPPH.Reference;

namespace HPPH.Test.PixelHelper;

[TestClass]
public class CreateSimpleColorPaletteTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    private static int[] SimpleSizes => [0, 1, 2, 16, 64];
    private static int[] Sizes => [0, 1, 2, 9, 16, 53];

    private static readonly Dictionary<string, Dictionary<Type, Dictionary<int, IColor[]>>> _simpleReference = [];
    private static readonly Dictionary<string, Dictionary<Type, Dictionary<int, IColor[]>>> _reference = [];

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        foreach (string image in GetTestImages())
        {
            _simpleReference[image] = [];

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
        _simpleReference[image][typeof(T)] = [];

        Image<T> img = ImageHelper.GetImage<T>(image);

        foreach (int size in SimpleSizes)
            _simpleReference[image][typeof(T)][size] = [.. ReferencePixelHelper.CreateSimpleColorPalette(img, size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        foreach (int size in Sizes)
            _reference[image][typeof(T)][size] = [.. ReferencePixelHelper.CreateColorPalette(img, size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];
    }

    [TestMethod]
    public void CreateSimpleColorPaletteReadOnlySpan()
    {
        ReadOnlySpan<int> sizes = SimpleSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateSimpleColorPaletteReadOnlySpan<ColorRGB>(image, size);
                CreateSimpleColorPaletteReadOnlySpan<ColorBGR>(image, size);
                CreateSimpleColorPaletteReadOnlySpan<ColorRGBA>(image, size);
                CreateSimpleColorPaletteReadOnlySpan<ColorBGRA>(image, size);
                CreateSimpleColorPaletteReadOnlySpan<ColorARGB>(image, size);
                CreateSimpleColorPaletteReadOnlySpan<ColorABGR>(image, size);
            }
        }
    }

    private void CreateSimpleColorPaletteReadOnlySpan<T>(string image, int size)
        where T : unmanaged, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        ReadOnlySpan<T> span = data;

        T[] test = [.. span.CreateSimpleColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _simpleReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateSimpleColorPaletteSpan()
    {
        ReadOnlySpan<int> sizes = SimpleSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateSimpleColorPaletteSpan<ColorRGB>(image, size);
                CreateSimpleColorPaletteSpan<ColorBGR>(image, size);
                CreateSimpleColorPaletteSpan<ColorRGBA>(image, size);
                CreateSimpleColorPaletteSpan<ColorBGRA>(image, size);
                CreateSimpleColorPaletteSpan<ColorARGB>(image, size);
                CreateSimpleColorPaletteSpan<ColorABGR>(image, size);
            }
        }
    }

    private void CreateSimpleColorPaletteSpan<T>(string image, int size)
        where T : unmanaged, IColor
    {
        T[] data = ImageHelper.GetColorsFromImage<T>(image);
        Span<T> span = data;

        T[] test = [.. span.CreateSimpleColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _simpleReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateSimpleColorPaletteImage()
    {
        ReadOnlySpan<int> sizes = SimpleSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateSimpleColorPaletteImage<ColorRGB>(image, size);
                CreateSimpleColorPaletteImage<ColorBGR>(image, size);
                CreateSimpleColorPaletteImage<ColorRGBA>(image, size);
                CreateSimpleColorPaletteImage<ColorBGRA>(image, size);
                CreateSimpleColorPaletteImage<ColorARGB>(image, size);
                CreateSimpleColorPaletteImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateSimpleColorPaletteImage<T>(string image, int size)
        where T : struct, IColor
    {
        IImage data = ImageHelper.GetImage<T>(image);

        IColor[] test = [.. data.CreateSimpleColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _simpleReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateSimpleColorPaletteGenericImage()
    {
        ReadOnlySpan<int> sizes = SimpleSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateSimpleColorPaletteGenericImage<ColorRGB>(image, size);
                CreateSimpleColorPaletteGenericImage<ColorBGR>(image, size);
                CreateSimpleColorPaletteGenericImage<ColorRGBA>(image, size);
                CreateSimpleColorPaletteGenericImage<ColorBGRA>(image, size);
                CreateSimpleColorPaletteGenericImage<ColorARGB>(image, size);
                CreateSimpleColorPaletteGenericImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateSimpleColorPaletteGenericImage<T>(string image, int size)
        where T : unmanaged, IColor
    {
        Image<T> data = ImageHelper.GetImage<T>(image);

        T[] test = [.. data.CreateSimpleColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _simpleReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    [TestMethod]
    public void CreateSimpleColorPaletteRefImage()
    {
        ReadOnlySpan<int> sizes = SimpleSizes;
        foreach (string image in GetTestImages())
        {
            foreach (int size in sizes)
            {
                CreateSimpleColorPaletteRefImage<ColorRGB>(image, size);
                CreateSimpleColorPaletteRefImage<ColorBGR>(image, size);
                CreateSimpleColorPaletteRefImage<ColorRGBA>(image, size);
                CreateSimpleColorPaletteRefImage<ColorBGRA>(image, size);
                CreateSimpleColorPaletteRefImage<ColorARGB>(image, size);
                CreateSimpleColorPaletteRefImage<ColorABGR>(image, size);
            }
        }
    }

    private void CreateSimpleColorPaletteRefImage<T>(string image, int size)
        where T : unmanaged, IColor
    {
        RefImage<T> data = ImageHelper.GetImage<T>(image).AsRefImage();

        T[] test = [.. data.CreateSimpleColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

        IColor[] reference = _simpleReference[image][typeof(T)][size];
        Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

        for (int i = 0; i < reference.Length; i++)
            Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    }

    //[TestMethod]
    //public void CreateColorPaletteReadOnlySpan()
    //{
    //    ReadOnlySpan<int> sizes = SimpleSizes;
    //    foreach (string image in GetTestImages())
    //    {
    //        foreach (int size in sizes)
    //        {
    //            CreateColorPaletteReadOnlySpan<ColorRGB>(image, size);
    //            CreateColorPaletteReadOnlySpan<ColorBGR>(image, size);
    //            CreateColorPaletteReadOnlySpan<ColorRGBA>(image, size);
    //            CreateColorPaletteReadOnlySpan<ColorBGRA>(image, size);
    //            CreateColorPaletteReadOnlySpan<ColorARGB>(image, size);
    //            CreateColorPaletteReadOnlySpan<ColorABGR>(image, size);
    //        }
    //    }
    //}

    //private void CreateColorPaletteReadOnlySpan<T>(string image, int size)
    //    where T : unmanaged, IColor
    //{
    //    T[] data = ImageHelper.GetColorsFromImage<T>(image);
    //    ReadOnlySpan<T> span = data;

    //    T[] test = [.. span.CreateColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

    //    IColor[] reference = _simpleReference[image][typeof(T)][size];
    //    Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

    //    for (int i = 0; i < reference.Length; i++)
    //        Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    //}

    //[TestMethod]
    //public void CreateColorPaletteSpan()
    //{
    //    ReadOnlySpan<int> sizes = SimpleSizes;
    //    foreach (string image in GetTestImages())
    //    {
    //        foreach (int size in sizes)
    //        {
    //            CreateColorPaletteSpan<ColorRGB>(image, size);
    //            CreateColorPaletteSpan<ColorBGR>(image, size);
    //            CreateColorPaletteSpan<ColorRGBA>(image, size);
    //            CreateColorPaletteSpan<ColorBGRA>(image, size);
    //            CreateColorPaletteSpan<ColorARGB>(image, size);
    //            CreateColorPaletteSpan<ColorABGR>(image, size);
    //        }
    //    }
    //}

    //private void CreateColorPaletteSpan<T>(string image, int size)
    //    where T : unmanaged, IColor
    //{
    //    T[] data = ImageHelper.GetColorsFromImage<T>(image);
    //    Span<T> span = data;

    //    T[] test = [.. span.CreateColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

    //    IColor[] reference = _simpleReference[image][typeof(T)][size];
    //    Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

    //    for (int i = 0; i < reference.Length; i++)
    //        Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    //}

    //[TestMethod]
    //public void CreateColorPaletteImage()
    //{
    //    ReadOnlySpan<int> sizes = SimpleSizes;
    //    foreach (string image in GetTestImages())
    //    {
    //        foreach (int size in sizes)
    //        {
    //            CreateColorPaletteImage<ColorRGB>(image, size);
    //            CreateColorPaletteImage<ColorBGR>(image, size);
    //            CreateColorPaletteImage<ColorRGBA>(image, size);
    //            CreateColorPaletteImage<ColorBGRA>(image, size);
    //            CreateColorPaletteImage<ColorARGB>(image, size);
    //            CreateColorPaletteImage<ColorABGR>(image, size);
    //        }
    //    }
    //}

    //private void CreateColorPaletteImage<T>(string image, int size)
    //    where T : struct, IColor
    //{
    //    IImage data = ImageHelper.GetImage<T>(image);

    //    IColor[] test = [.. data.CreateColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

    //    IColor[] reference = _simpleReference[image][typeof(T)][size];
    //    Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

    //    for (int i = 0; i < reference.Length; i++)
    //        Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    //}

    //[TestMethod]
    //public void CreateColorPaletteGenericImage()
    //{
    //    ReadOnlySpan<int> sizes = SimpleSizes;
    //    foreach (string image in GetTestImages())
    //    {
    //        foreach (int size in sizes)
    //        {
    //            CreateColorPaletteGenericImage<ColorRGB>(image, size);
    //            CreateColorPaletteGenericImage<ColorBGR>(image, size);
    //            CreateColorPaletteGenericImage<ColorRGBA>(image, size);
    //            CreateColorPaletteGenericImage<ColorBGRA>(image, size);
    //            CreateColorPaletteGenericImage<ColorARGB>(image, size);
    //            CreateColorPaletteGenericImage<ColorABGR>(image, size);
    //        }
    //    }
    //}

    //private void CreateColorPaletteGenericImage<T>(string image, int size)
    //    where T : unmanaged, IColor
    //{
    //    Image<T> data = ImageHelper.GetImage<T>(image);

    //    T[] test = [.. data.CreateColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

    //    IColor[] reference = _simpleReference[image][typeof(T)][size];
    //    Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

    //    for (int i = 0; i < reference.Length; i++)
    //        Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    //}

    //[TestMethod]
    //public void CreateColorPaletteRefImage()
    //{
    //    ReadOnlySpan<int> sizes = SimpleSizes;
    //    foreach (string image in GetTestImages())
    //    {
    //        foreach (int size in sizes)
    //        {
    //            CreateColorPaletteRefImage<ColorRGB>(image, size);
    //            CreateColorPaletteRefImage<ColorBGR>(image, size);
    //            CreateColorPaletteRefImage<ColorRGBA>(image, size);
    //            CreateColorPaletteRefImage<ColorBGRA>(image, size);
    //            CreateColorPaletteRefImage<ColorARGB>(image, size);
    //            CreateColorPaletteRefImage<ColorABGR>(image, size);
    //        }
    //    }
    //}

    //private void CreateColorPaletteRefImage<T>(string image, int size)
    //    where T : unmanaged, IColor
    //{
    //    RefImage<T> data = ImageHelper.GetImage<T>(image).AsRefImage();

    //    T[] test = [.. data.CreateColorPalette(size).OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ThenBy(x => x.A)];

    //    IColor[] reference = _simpleReference[image][typeof(T)][size];
    //    Assert.AreEqual(reference.Length, test.Length, $"Palette Size differs for image {image}, size {size}");

    //    for (int i = 0; i < reference.Length; i++)
    //        Assert.AreEqual(reference[i], test[i], $"Index {i} differs for image {image}, size {size}");
    //}
}