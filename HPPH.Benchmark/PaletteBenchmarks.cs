using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class PaletteBenchmarks
{
    #region Properties & Fields

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    [Params(16, 256)]
    public int PaletteSize { get; set; }

    private List<ColorRGB[]> _colors = [];

    #endregion

    #region Methods

    [GlobalSetup]
    public void Setup()
    {
        _colors = BenchmarkHelper.GetSampleData<ColorRGB>(Data);
    }

    [Benchmark]
    public ColorRGB[] CreateMedianCutColorPalette()
    {
        ColorRGB[] result = [];
        foreach (ColorRGB[] colors in _colors)
            result = new ReadOnlySpan<ColorRGB>(colors).CreateMedianCutColorPalette(PaletteSize);

        return result;
    }

    [Benchmark]
    public ColorRGB[] CreateWuColorPalette()
    {
        ColorRGB[] result = [];
        foreach (ColorRGB[] colors in _colors)
            result = new ReadOnlySpan<ColorRGB>(colors).CreateWuColorPalette(PaletteSize);

        return result;
    }

    #endregion
}