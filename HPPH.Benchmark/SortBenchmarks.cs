using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HPPH.Reference;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
[HtmlExporter]
[MemoryDiagnoser]
public class SortBenchmarks
{
    #region Properties & Fields

    private readonly List<ColorRGB[]> _colors3bpp;
    private readonly List<ColorRGBA[]> _colors4bpp;

    #endregion

    #region Constructors

    public SortBenchmarks()
    {
        _colors3bpp = BenchmarkHelper.GetSampleData<ColorRGB>();
        _colors4bpp = BenchmarkHelper.GetSampleData<ColorRGBA>();
    }

    #endregion

    #region Methods

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