using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HPPH.Reference;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class SumBenchmarks
{
    #region Properties & Fields

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    private List<ColorRGB[]> _colors3bpp = [];
    private List<ColorRGBA[]> _colors4bpp = [];
    private List<Image<ColorRGB>> _images3bpp = [];
    private List<Image<ColorRGBA>> _images4bpp = [];

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
    public ColorSum[] PixelHelper_3BPP()
    {
        ColorSum[] sums = new ColorSum[_colors3bpp.Count];
        for (int i = 0; i < _colors3bpp.Count; i++)
            sums[i] = new ReadOnlySpan<ColorRGB>(_colors3bpp[i]).Sum();

        return sums;
    }

    [Benchmark]
    public ColorSum[] PixelHelper_4BPP()
    {
        ColorSum[] sums = new ColorSum[_colors4bpp.Count];
        for (int i = 0; i < _colors4bpp.Count; i++)
            sums[i] = new ReadOnlySpan<ColorRGBA>(_colors4bpp[i]).Sum();

        return sums;
    }

    [Benchmark]
    public ColorSum[] PixelHelper_3BPP_Image()
    {
        ColorSum[] sums = new ColorSum[_colors3bpp.Count];
        for (int i = 0; i < _images3bpp.Count; i++)
            sums[i] = _images3bpp[i].Sum();

        return sums;
    }

    [Benchmark]
    public ColorSum[] PixelHelper_4BPP_Image()
    {
        ColorSum[] sums = new ColorSum[_images4bpp.Count];
        for (int i = 0; i < _images4bpp.Count; i++)
            sums[i] = _images4bpp[i].Sum();

        return sums;
    }

    [Benchmark]
    public ColorSum[] Reference_3BPP()
    {
        ColorSum[] sums = new ColorSum[_colors3bpp.Count];
        for (int i = 0; i < _colors3bpp.Count; i++)
            sums[i] = ReferencePixelHelper.Sum(new ReadOnlySpan<ColorRGB>(_colors3bpp[i]));

        return sums;
    }

    [Benchmark]
    public ColorSum[] Reference_4BPP()
    {
        ColorSum[] sums = new ColorSum[_colors4bpp.Count];
        for (int i = 0; i < _colors4bpp.Count; i++)
            sums[i] = ReferencePixelHelper.Sum(new ReadOnlySpan<ColorRGBA>(_colors4bpp[i]));

        return sums;
    }

    #endregion
}