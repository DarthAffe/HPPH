using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
[HtmlExporter]
[MemoryDiagnoser]
public class ConvertBenchmarks
{
    #region Properties & Fields

    private readonly List<ColorRGB[]> _colors3bpp;
    private readonly List<ColorRGBA[]> _colors4bpp;

    #endregion

    #region Constructors

    public ConvertBenchmarks()
    {
        _colors3bpp = BenchmarkHelper.GetSampleData<ColorRGB>();
        _colors4bpp = BenchmarkHelper.GetSampleData<ColorRGBA>();
    }

    #endregion

    #region Methods

    [Benchmark]
    public ColorBGR[] RGBToBGR()
    {
        ColorBGR[] result = [];
        foreach (ColorRGB[] color in _colors3bpp)
            result = new ReadOnlySpan<ColorRGB>(color).Convert<ColorRGB, ColorBGR>();

        return result;
    }

    [Benchmark]
    public ColorBGRA[] RGBToBGRA()
    {
        ColorBGRA[] result = [];
        foreach (ColorRGB[] color in _colors3bpp)
            result = new ReadOnlySpan<ColorRGB>(color).Convert<ColorRGB, ColorBGRA>();

        return result;
    }

    [Benchmark]
    public ColorABGR[] RGBAToABGR()
    {
        ColorABGR[] result = [];
        foreach (ColorRGBA[] color in _colors4bpp)
            result = new ReadOnlySpan<ColorRGBA>(color).Convert<ColorRGBA, ColorABGR>();

        return result;
    }

    [Benchmark]
    public ColorBGR[] ARGBToBGR()
    {
        ColorBGR[] result = [];
        foreach (ColorRGBA[] color in _colors4bpp)
            result = new ReadOnlySpan<ColorRGBA>(color).Convert<ColorRGBA, ColorBGR>();

        return result;
    }

    [Benchmark]
    public void RGBToBGR_InPlace()
    {
        foreach (ColorRGB[] color in _colors3bpp)
            new Span<ColorRGB>(color).ConvertInPlace<ColorRGB, ColorBGR>();
    }

    [Benchmark]
    public void RGBAToABGR_InPlace()
    {
        foreach (ColorRGBA[] color in _colors4bpp)
            new Span<ColorRGBA>(color).ConvertInPlace<ColorRGBA, ColorABGR>();
    }

    #endregion
}