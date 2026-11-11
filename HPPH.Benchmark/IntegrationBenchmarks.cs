#pragma warning disable CA1416

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SkiaSharp;
using Bitmap = System.Drawing.Bitmap;
using Gdi = HPPH.System.Drawing;
using Skia = HPPH.SkiaSharp;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class IntegrationBenchmarks
{
    #region Properties & Fields

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    private List<Image<ColorRGBA>> _rgbaImages = [];
    private List<Image<ColorBGRA>> _bgraImages = [];
    private List<byte[]> _pngs = [];
    private List<Bitmap> _bitmaps = [];

    #endregion

    #region Methods

    [GlobalSetup]
    public void Setup()
    {
        _rgbaImages = BenchmarkHelper.GetSampleDataImages<ColorRGBA>(Data);
        _bgraImages = BenchmarkHelper.GetSampleDataImages<ColorBGRA>(Data);
        _pngs = [.. _bgraImages.Select(Skia.ImageExtension.ToPng)];
        _bitmaps = [.. _bgraImages.Select(Gdi.ImageExtension.ToBitmap)];
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (Bitmap bitmap in _bitmaps)
            bitmap.Dispose();
    }
    
    [Benchmark]
    public IImage? Skia_LoadImage()
    {
        IImage? result = null;
        foreach (byte[] png in _pngs)
        {
            using MemoryStream stream = new(png, false);
            result = Skia.ImageHelper.LoadImage(stream);
        }

        return result;
    }

    [Benchmark]
    public Image<ColorRGB>? Skia_LoadImage_Generic()
    {
        Image<ColorRGB>? result = null;
        foreach (byte[] png in _pngs)
        {
            using MemoryStream stream = new(png, false);
            result = Skia.ImageHelper.LoadImage<ColorRGB>(stream);
        }

        return result;
    }

    [Benchmark]
    public Image<ColorRGB>? Skia_LoadImage_ConvertTo()
    {
        Image<ColorRGB>? result = null;
        foreach (byte[] png in _pngs)
        {
            using MemoryStream stream = new(png, false);
            result = Skia.ImageHelper.LoadImage(stream).ConvertTo<ColorRGB>();
        }

        return result;
    }

    [Benchmark]
    public int Skia_ToSKBitmap_BGRA()
    {
        int result = 0;
        foreach (Image<ColorBGRA> image in _bgraImages)
        {
            using SKBitmap bitmap = Skia.ImageExtension.ToSKBitmap(image);
            result += bitmap.Width;
        }

        return result;
    }

    [Benchmark]
    public int Skia_ToSKBitmap_RGBA()
    {
        int result = 0;
        foreach (Image<ColorRGBA> image in _rgbaImages)
        {
            using SKBitmap bitmap = Skia.ImageExtension.ToSKBitmap(image);
            result += bitmap.Width;
        }

        return result;
    }

    [Benchmark]
    public int Gdi_ToBitmap_BGRA()
    {
        int result = 0;
        foreach (Image<ColorBGRA> image in _bgraImages)
        {
            using Bitmap bitmap = Gdi.ImageExtension.ToBitmap(image);
            result += bitmap.Width;
        }

        return result;
    }

    [Benchmark]
    public int Gdi_ToBitmap_RGBA()
    {
        int result = 0;
        foreach (Image<ColorRGBA> image in _rgbaImages)
        {
            using Bitmap bitmap = Gdi.ImageExtension.ToBitmap(image);
            result += bitmap.Width;
        }

        return result;
    }

    [Benchmark]
    public IImage? Gdi_ToImage()
    {
        IImage? result = null;
        foreach (Bitmap bitmap in _bitmaps)
            result = Gdi.ImageExtension.ToImage(bitmap);

        return result;
    }

    #endregion
}