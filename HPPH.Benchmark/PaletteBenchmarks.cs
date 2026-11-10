using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class PaletteBenchmarks
{
    #region Constants

    private const int PALETTE_SIZE = 16;

    #endregion

    #region Properties & Fields

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    private List<ColorRGB[]> _colors = [];

    #endregion

    #region Methods

    [GlobalSetup]
    public void Setup()
    {
        _colors = BenchmarkHelper.GetSampleData<ColorRGB>(Data);
    }

    [Benchmark]
    public ColorRGB[] CreateSimpleColorPalette()
    {
        ColorRGB[] result = [];
        foreach (ColorRGB[] colors in _colors)
            result = new ReadOnlySpan<ColorRGB>(colors).CreateSimpleColorPalette(PALETTE_SIZE);

        return result;
    }

    #endregion
}