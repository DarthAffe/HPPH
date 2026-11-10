using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HPPH.Reference;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class MinMaxBenchmarks
{
    #region Properties & Fields

    private readonly List<ColorRGB[]> _colors3bpp;
    private readonly List<ColorRGBA[]> _colors4bpp;
    private readonly List<IImage<ColorRGB>> _images3bpp;
    private readonly List<IImage<ColorRGBA>> _images4bpp;

    #endregion

    #region Constructors

    public MinMaxBenchmarks()
    {
        _colors3bpp = BenchmarkHelper.GetSampleData<ColorRGB>();
        _colors4bpp = BenchmarkHelper.GetSampleData<ColorRGBA>();

        _images3bpp = BenchmarkHelper.GetSampleDataImages<ColorRGB>();
        _images4bpp = BenchmarkHelper.GetSampleDataImages<ColorRGBA>();
    }

    #endregion

    #region Methods

    [Benchmark]
    public ColorMinMax[] PixelHelper_3BPP()
    {
        ColorMinMax[] minMax = new ColorMinMax[_colors3bpp.Count];
        for (int i = 0; i < _colors3bpp.Count; i++)
            minMax[i] = new ReadOnlySpan<ColorRGB>(_colors3bpp[i]).MinMax();

        return minMax;
    }

    [Benchmark]
    public ColorMinMax[] PixelHelper_4BPP()
    {
        ColorMinMax[] minMax = new ColorMinMax[_colors4bpp.Count];
        for (int i = 0; i < _colors4bpp.Count; i++)
            minMax[i] = new ReadOnlySpan<ColorRGBA>(_colors4bpp[i]).MinMax();

        return minMax;
    }

    [Benchmark]
    public ColorMinMax[] PixelHelper_3BPP_Image()
    {
        ColorMinMax[] minMax = new ColorMinMax[_images3bpp.Count];
        for (int i = 0; i < _images3bpp.Count; i++)
            minMax[i] = _images3bpp[i].MinMax();

        return minMax;
    }

    [Benchmark]
    public ColorMinMax[] PixelHelper_4BPP_Image()
    {
        ColorMinMax[] minMax = new ColorMinMax[_images4bpp.Count];
        for (int i = 0; i < _images4bpp.Count; i++)
            minMax[i] = _images4bpp[i].MinMax();

        return minMax;
    }

    [Benchmark]
    public ColorMinMax[] Reference_3BPP()
    {
        ColorMinMax[] minMax = new ColorMinMax[_colors3bpp.Count];
        for (int i = 0; i < _colors3bpp.Count; i++)
            minMax[i] = ReferencePixelHelper.MinMax(new ReadOnlySpan<ColorRGB>(_colors3bpp[i]));

        return minMax;
    }

    [Benchmark]
    public ColorMinMax[] Reference_4BPP()
    {
        ColorMinMax[] minMax = new ColorMinMax[_colors4bpp.Count];
        for (int i = 0; i < _colors4bpp.Count; i++)
            minMax[i] = ReferencePixelHelper.MinMax(new ReadOnlySpan<ColorRGBA>(_colors4bpp[i]));

        return minMax;
    }

    #endregion
}