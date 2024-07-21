﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HPPH.Reference;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
[HtmlExporter]
[MemoryDiagnoser]
public class AverageBenchmarks
{
    #region Properties & Fields

    private readonly List<ColorRGB[]> _colors3bpp;
    private readonly List<ColorRGBA[]> _colors4bpp;

    #endregion

    #region Constructors

    public AverageBenchmarks()
    {
        _colors3bpp = BenchmarkHelper.GetSampleData<ColorRGB>();
        _colors4bpp = BenchmarkHelper.GetSampleData<ColorRGBA>();
    }

    #endregion

    #region Methods

    [Benchmark]
    public ColorRGB[] PixelHelper_3BPP()
    {
        ColorRGB[] averages = new ColorRGB[_colors3bpp.Count];
        for (int i = 0; i < _colors3bpp.Count; i++)
            averages[i] = new ReadOnlySpan<ColorRGB>(_colors3bpp[i]).Average();

        return averages;
    }

    [Benchmark]
    public ColorRGBA[] PixelHelper_4BPP()
    {
        ColorRGBA[] averages = new ColorRGBA[_colors4bpp.Count];
        for (int i = 0; i < _colors4bpp.Count; i++)
            averages[i] = new ReadOnlySpan<ColorRGBA>(_colors4bpp[i]).Average();

        return averages;
    }

    [Benchmark]
    public ColorRGB[] Reference_3BPP()
    {
        ColorRGB[] averages = new ColorRGB[_colors3bpp.Count];
        for (int i = 0; i < _colors3bpp.Count; i++)
            averages[i] = ReferencePixelHelper.Average(new ReadOnlySpan<ColorRGB>(_colors3bpp[i]));

        return averages;
    }

    [Benchmark]
    public ColorRGBA[] Reference_4BPP()
    {
        ColorRGBA[] averages = new ColorRGBA[_colors4bpp.Count];
        for (int i = 0; i < _colors4bpp.Count; i++)
            averages[i] = ReferencePixelHelper.Average(new ReadOnlySpan<ColorRGBA>(_colors4bpp[i]));

        return averages;
    }

    #endregion
}