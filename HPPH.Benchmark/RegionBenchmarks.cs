using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class RegionBenchmarks
{
    #region Constants

    private const int REGIONS_PER_EDGE = 16;
    private const int DEPTH = 20;
    private const int STRIP_WIDTH = 16;

    // Smaller sample images can't hold the regions.
    private const int MIN_IMAGE_SIZE = 64;

    #endregion

    #region Properties & Fields

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    private List<Image<ColorBGRA>> _images = [];

    #endregion

    #region Methods

    [GlobalSetup]
    public void Setup()
    {
        _images = [.. BenchmarkHelper.GetSampleDataImages<ColorBGRA>(Data).Where(image => (image.Width >= MIN_IMAGE_SIZE) && (image.Height >= MIN_IMAGE_SIZE))];
    }
    
    [Benchmark]
    public ColorBGRA[] Average_EdgeRegions()
    {
        ColorBGRA[] averages = new ColorBGRA[_images.Count * REGIONS_PER_EDGE * 4];

        int index = 0;
        foreach (Image<ColorBGRA> image in _images)
        {
            RefImage<ColorBGRA> refImage = image.AsRefImage();
            int horizontalSize = image.Width / REGIONS_PER_EDGE;
            int verticalSize = image.Height / REGIONS_PER_EDGE;

            for (int i = 0; i < REGIONS_PER_EDGE; i++)
            {
                averages[index++] = refImage[i * horizontalSize, 0, horizontalSize, DEPTH].Average();
                averages[index++] = refImage[i * horizontalSize, image.Height - DEPTH, horizontalSize, DEPTH].Average();
                averages[index++] = refImage[0, i * verticalSize, DEPTH, verticalSize].Average();
                averages[index++] = refImage[image.Width - DEPTH, i * verticalSize, DEPTH, verticalSize].Average();
            }
        }

        return averages;
    }

    [Benchmark]
    public ColorSum[] Sum_Strip()
    {
        ColorSum[] sums = new ColorSum[_images.Count];
        for (int i = 0; i < _images.Count; i++)
            sums[i] = _images[i].AsRefImage()[0, 0, STRIP_WIDTH, _images[i].Height].Sum();

        return sums;
    }

    [Benchmark]
    public ColorMinMax[] MinMax_Strip()
    {
        ColorMinMax[] minMax = new ColorMinMax[_images.Count];
        for (int i = 0; i < _images.Count; i++)
            minMax[i] = _images[i].AsRefImage()[0, 0, STRIP_WIDTH, _images[i].Height].MinMax();

        return minMax;
    }

    [Benchmark]
    public ColorBGRA[] Average_Strip()
    {
        ColorBGRA[] averages = new ColorBGRA[_images.Count];
        for (int i = 0; i < _images.Count; i++)
            averages[i] = _images[i].AsRefImage()[0, 0, STRIP_WIDTH, _images[i].Height].Average();

        return averages;
    }

    #endregion
}