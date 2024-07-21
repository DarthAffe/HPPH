using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HPPH.Reference;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
[HtmlExporter]
[MemoryDiagnoser]
public class SumBenchmarks
{
    #region Properties & Fields

    private readonly List<ColorRGB[]> _colors3bpp;
    private readonly List<ColorRGBA[]> _colors4bpp;

    #endregion

    #region Constructors

    public SumBenchmarks()
    {
        _colors3bpp = BenchmarkHelper.GetSampleData<ColorRGB>();
        _colors4bpp = BenchmarkHelper.GetSampleData<ColorRGBA>();
    }

    #endregion

    #region Methods

    [Benchmark]
    public ISum[] PixelHelper_3BPP()
    {
        ISum[] sums = new ISum[_colors3bpp.Count];
        for (int i = 0; i < _colors3bpp.Count; i++)
            sums[i] = new ReadOnlySpan<ColorRGB>(_colors3bpp[i]).Sum();

        return sums;
    }

    [Benchmark]
    public ISum[] PixelHelper_4BPP()
    {
        ISum[] sums = new ISum[_colors4bpp.Count];
        for (int i = 0; i < _colors4bpp.Count; i++)
            sums[i] = new ReadOnlySpan<ColorRGBA>(_colors4bpp[i]).Sum();

        return sums;
    }

    [Benchmark]
    public ISum[] Reference_3BPP()
    {
        ISum[] sums = new ISum[_colors3bpp.Count];
        for (int i = 0; i < _colors3bpp.Count; i++)
            sums[i] = ReferencePixelHelper.Sum(new ReadOnlySpan<ColorRGB>(_colors3bpp[i]));

        return sums;
    }

    [Benchmark]
    public ISum[] Reference_4BPP()
    {
        ISum[] sums = new ISum[_colors4bpp.Count];
        for (int i = 0; i < _colors4bpp.Count; i++)
            sums[i] = ReferencePixelHelper.Sum(new ReadOnlySpan<ColorRGBA>(_colors4bpp[i]));

        return sums;
    }

    #endregion
}