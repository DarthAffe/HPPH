using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HPPH.Reference;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class RotateBenchmarks
{
    #region Properties & Fields

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    [Params(ImageRotation.Rotate90, ImageRotation.Rotate270)]
    public ImageRotation Rotation { get; set; }

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
            result = image.Rotate(Rotation);

        return result;
    }

    [Benchmark]
    public void PixelHelper_4BPP_Buffer()
    {
        foreach (Image<ColorBGRA> image in _images4bpp)
            image.Rotate(Rotation, _buffer, image.Height * 4);
    }

    [Benchmark]
    public void PixelHelper_3BPP_Buffer()
    {
        foreach (Image<ColorRGB> image in _images3bpp)
            image.Rotate(Rotation, _buffer, image.Height * 3);
    }

    [Benchmark]
    public void PixelHelper_4BPP_Region()
    {
        foreach (Image<ColorBGRA> image in _images4bpp)
            if ((image.Width >= 2) && (image.Height >= 2))
                image.AsRefImage()[image.Width / 4, image.Height / 4, image.Width / 2, image.Height / 2].Rotate(Rotation, _buffer, (image.Height / 2) * 4);
    }

    [Benchmark]
    public Image<ColorBGRA>? Reference_4BPP()
    {
        Image<ColorBGRA>? result = null;
        foreach (Image<ColorBGRA> image in _images4bpp)
            result = ReferencePixelHelper.Rotate(image.AsRefImage(), Rotation);

        return result;
    }

    #endregion
}
