using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HPPH.System.Drawing;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
[HtmlExporter]
[MemoryDiagnoser]
public class ConvertBenchmarks
{
    #region Properties & Fields

    private readonly List<ColorRGB[]> _colors = [];

    #endregion

    #region Constructors

    public ConvertBenchmarks()
    {
        if (!Directory.Exists(@"..\..\..\..\sample_data")) return;

        _colors = [];

        IEnumerable<string> files = Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);
        foreach (string file in files)
            _colors.Add(ImageHelper.LoadImage(file).AsRefImage<ColorRGB>().ToArray());
    }

    #endregion

    #region Methods

    [Benchmark]
    public ColorBGR[] RGBToBGR()
    {
        ColorBGR[] result = [];
        foreach (ColorRGB[] color in _colors)
        {
            result = new ReadOnlySpan<ColorRGB>(color).Convert<ColorRGB, ColorBGR>();
        }

        return result;
    }

    [Benchmark]
    public ColorBGRA[] RGBToBGRA()
    {
        ColorBGRA[] result = [];
        foreach (ColorRGB[] color in _colors)
            result = new ReadOnlySpan<ColorRGB>(color).Convert<ColorRGB, ColorBGRA>();

        return result;
    }

    #endregion
}