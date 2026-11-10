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

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    private List<ColorRGB[]> _colors3bpp = [];
    private List<ColorRGBA[]> _colors4bpp = [];
    private List<IImage<ColorRGB>> _images3bpp = [];
    private List<IImage<ColorRGBA>> _images4bpp = [];

    #endregion

    #region Methods

    [GlobalSetup]
    public void Setup()
    {
        _colors3bpp = BenchmarkHelper.GetSampleData<ColorRGB>(Data);
        _colors4bpp = BenchmarkHelper.GetSampleData<ColorRGBA>(Data);

        _images3bpp = BenchmarkHelper.GetSampleDataImages<ColorRGB>(Data);
        _images4bpp = BenchmarkHelper.GetSampleDataImages<ColorRGBA>(Data);
    }


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