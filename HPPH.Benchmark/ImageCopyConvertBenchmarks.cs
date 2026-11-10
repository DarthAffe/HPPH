using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class ImageCopyConvertBenchmarks
{
    #region Properties & Fields

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    private List<IImage<ColorRGBA>> _images = [];

    private ColorBGRA[] _bgraBuffer = [];
    private ColorRGB[] _rgbBuffer = [];
    private byte[] _rawBuffer = [];

    #endregion

    #region Methods

    [GlobalSetup]
    public void Setup()
    {
        _images = BenchmarkHelper.GetSampleDataImages<ColorRGBA>(Data);

        int maxPixels = _images.Max(image => image.Width * image.Height);
        _bgraBuffer = new ColorBGRA[maxPixels];
        _rgbBuffer = new ColorRGB[maxPixels];
        _rawBuffer = new byte[maxPixels * 4];
    }
    
    [Benchmark]
    public IImage<ColorBGRA>? ConvertTo_4To4()
    {
        IImage<ColorBGRA>? result = null;
        foreach (IImage<ColorRGBA> image in _images)
            result = image.ConvertTo<ColorBGRA>();

        return result;
    }

    [Benchmark]
    public IImage<ColorRGB>? ConvertTo_4To3()
    {
        IImage<ColorRGB>? result = null;
        foreach (IImage<ColorRGBA> image in _images)
            result = image.ConvertTo<ColorRGB>();

        return result;
    }

    [Benchmark]
    public void ConvertToBuffer_4To4()
    {
        foreach (IImage<ColorRGBA> image in _images)
            image.ConvertTo(_bgraBuffer.AsSpan());
    }

    [Benchmark]
    public void ConvertToBuffer_4To3()
    {
        foreach (IImage<ColorRGBA> image in _images)
            image.ConvertTo(_rgbBuffer.AsSpan());
    }

    [Benchmark]
    public void CopyTo_Image()
    {
        foreach (IImage<ColorRGBA> image in _images)
            image.CopyTo(_rawBuffer);
    }

    [Benchmark]
    public void CopyTo_Region()
    {
        // The center quarter: rows that don't follow each other in memory.
        foreach (IImage<ColorRGBA> image in _images)
            if ((image.Width >= 2) && (image.Height >= 2))
                image.AsRefImage()[image.Width / 4, image.Height / 4, image.Width / 2, image.Height / 2].CopyTo(_rawBuffer.AsSpan());
    }

    #endregion
}