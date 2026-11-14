using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HPPH.Reference;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class DownscaleBenchmarks
{
    #region Properties & Fields

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    [Params(2, 3, 4, 8, 16)]
    public int Factor { get; set; }

    private List<Image<ColorRGB>> _images3bpp = [];
    private List<Image<ColorBGRA>> _images4bpp = [];

    private byte[] _buffer = [];

    #endregion

    #region Methods

    [GlobalSetup]
    public void Setup()
    {
        _images3bpp = BenchmarkHelper.GetSampleDataImages<ColorRGB>(Data);
        _images4bpp = BenchmarkHelper.GetSampleDataImages<ColorBGRA>(Data);

        _buffer = new byte[_images4bpp.Max(image => image.Width * image.Height) * 4];
    }

    [Benchmark]
    public Image<ColorBGRA>? PixelHelper_4BPP()
    {
        Image<ColorBGRA>? result = null;
        foreach (Image<ColorBGRA> image in _images4bpp)
            result = image.Downscale(Factor);

        return result;
    }

    [Benchmark]
    public void PixelHelper_4BPP_Buffer()
    {
        foreach (Image<ColorBGRA> image in _images4bpp)
            image.Downscale(Factor, _buffer, PixelHelper.GetDownscaledSize(image.Width, image.Height, Factor).Width * 4);
    }

    [Benchmark]
    public void PixelHelper_3BPP_Buffer()
    {
        foreach (Image<ColorRGB> image in _images3bpp)
            image.Downscale(Factor, _buffer, PixelHelper.GetDownscaledSize(image.Width, image.Height, Factor).Width * 3);
    }

    [Benchmark]
    public void AverageOfBlocks_4BPP()
    {
        Span<ColorBGRA> buffer = MemoryMarshal.Cast<byte, ColorBGRA>(_buffer.AsSpan());
        foreach (Image<ColorBGRA> image in _images4bpp)
        {
            (int width, int height) = PixelHelper.GetDownscaledSize(image.Width, image.Height, Factor);
            int blockWidth = Math.Min(Factor, image.Width);
            int blockHeight = Math.Min(Factor, image.Height);

            RefImage<ColorBGRA> source = image.AsRefImage();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    buffer[(y * width) + x] = source[x * blockWidth, y * blockHeight, blockWidth, blockHeight].Average();
        }
    }

    [Benchmark]
    public Image<ColorBGRA>? Reference_4BPP()
    {
        Image<ColorBGRA>? result = null;
        foreach (Image<ColorBGRA> image in _images4bpp)
            result = ReferencePixelHelper.Downscale(image.AsRefImage(), Factor, Factor);

        return result;
    }

    #endregion
}
