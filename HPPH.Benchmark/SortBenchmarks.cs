using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HPPH.Reference;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class SortBenchmarks
{
    #region Properties & Fields

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    private List<ColorRGB[]> _source3bpp = [];
    private List<ColorRGBA[]> _source4bpp = [];
    private List<ColorRGB[]> _colors3bpp = [];
    private List<ColorRGBA[]> _colors4bpp = [];

    #endregion

    #region Methods

    [GlobalSetup]
    public void Setup()
    {
        _source3bpp = BenchmarkHelper.GetSampleData<ColorRGB>(Data);
        _source4bpp = BenchmarkHelper.GetSampleData<ColorRGBA>(Data);

        _colors3bpp = [.. _source3bpp.Select(colors => (ColorRGB[])colors.Clone())];
        _colors4bpp = [.. _source4bpp.Select(colors => (ColorRGBA[])colors.Clone())];
    }

    // Sorting works in place, so every iteration has to start from the unsorted data again.
    [IterationSetup]
    public void RestoreData()
    {
        for (int i = 0; i < _source3bpp.Count; i++)
            _source3bpp[i].CopyTo(_colors3bpp[i], 0);

        for (int i = 0; i < _source4bpp.Count; i++)
            _source4bpp[i].CopyTo(_colors4bpp[i], 0);
    }

    [Benchmark]
    public void PixelHelper_3BPP()
    {
        foreach (ColorRGB[] colors in _colors3bpp)
            new Span<ColorRGB>(colors).SortByRed();
    }

    [Benchmark]
    public void PixelHelper_4BPP()
    {
        foreach (ColorRGBA[] colors in _colors4bpp)
            new Span<ColorRGBA>(colors).SortByRed();
    }

    [Benchmark]
    public void Reference_3BPP()
    {
        foreach (ColorRGB[] colors in _colors3bpp)
            ReferencePixelHelper.SortByRed(new Span<ColorRGB>(colors));
    }

    [Benchmark]
    public void Reference_4BPP()
    {
        foreach (ColorRGBA[] colors in _colors4bpp)
            ReferencePixelHelper.SortByRed(new Span<ColorRGBA>(colors));
    }

    #endregion
}