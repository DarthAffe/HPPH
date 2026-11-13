using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace HPPH.Benchmark;

[SimpleJob(RuntimeMoniker.Net10_0)]
[HtmlExporter]
[MemoryDiagnoser]
public class IterationBenchmarks
{
    #region Properties & Fields

    [Params(DataSet.Samples, DataSet.FullHD, DataSet.UHD)]
    public DataSet Data { get; set; }

    private List<Image<ColorRGBA>> _images = [];

    #endregion

    #region Methods

    [GlobalSetup]
    public void Setup()
    {
        _images = BenchmarkHelper.GetSampleDataImages<ColorRGBA>(Data);
    }

    [Benchmark]
    public long Foreach_Image()
    {
        long sum = 0;
        foreach (Image<ColorRGBA> image in _images)
            foreach (ColorRGBA color in image)
                sum += color.R;

        return sum;
    }

    [Benchmark]
    public long Foreach_Enumerable()
    {
        long sum = 0;
        foreach (Image<ColorRGBA> image in _images)
            foreach (ColorRGBA color in (IEnumerable<ColorRGBA>)image)
                sum += color.R;

        return sum;
    }

    [Benchmark]
    public long Foreach_RefImage()
    {
        long sum = 0;
        foreach (Image<ColorRGBA> image in _images)
            foreach (ColorRGBA color in image.AsRefImage())
                sum += color.R;

        return sum;
    }

    [Benchmark]
    public long Indexer()
    {
        long sum = 0;
        foreach (Image<ColorRGBA> image in _images)
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    sum += image[x, y].R;

        return sum;
    }

    [Benchmark]
    public long Foreach_Rows()
    {
        long sum = 0;
        foreach (Image<ColorRGBA> image in _images)
            foreach (ImageRow<ColorRGBA> row in image.Rows)
                foreach (ColorRGBA color in row)
                    sum += color.R;

        return sum;
    }

    [Benchmark]
    public long Foreach_RowSpans()
    {
        long sum = 0;
        foreach (Image<ColorRGBA> image in _images)
            foreach (ImageRow<ColorRGBA> row in image.Rows)
                foreach (ColorRGBA color in row.AsSpan())
                    sum += color.R;

        return sum;
    }

    #endregion
}