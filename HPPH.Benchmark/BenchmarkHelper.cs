#pragma warning disable CA1416

using System.Drawing;
using HPPH.System.Drawing;

namespace HPPH.Benchmark;

public enum DataSet
{
    /// <summary>The images in sample_data (1x1 up to 1024x1024, about 3.1 megapixels in total).</summary>
    Samples,

    /// <summary>One 1920x1080 image.</summary>
    FullHD,

    /// <summary>One 3840x2160 image.</summary>
    UHD
}

internal static class BenchmarkHelper
{
    private const string SAMPLE_DATA_DIR = @"..\..\..\..\..\..\..\..\sample_data";

    public static List<T[]> GetSampleData<T>(DataSet dataSet)
        where T : unmanaged, IColor
        => [.. GetSampleDataImages<T>(dataSet).Select(image => image.ToArray())];

    public static List<IImage<T>> GetSampleDataImages<T>(DataSet dataSet)
        where T : unmanaged, IColor
        => dataSet switch
        {
            DataSet.Samples => [.. GetSampleFiles().Select(ImageHelper.LoadImage<T>)],
            DataSet.FullHD => [CreateScaledImage<T>(1920, 1080)],
            DataSet.UHD => [CreateScaledImage<T>(3840, 2160)],
            _ => throw new ArgumentOutOfRangeException(nameof(dataSet))
        };

    private static IImage<T> CreateScaledImage<T>(int width, int height)
        where T : unmanaged, IColor
    {
        using Bitmap source = new(GetSampleFiles().First());
        using Bitmap scaled = new(source, width, height);
        return scaled.ToImage<T>();
    }

    private static IEnumerable<string> GetSampleFiles()
    {
        if (!Directory.Exists(SAMPLE_DATA_DIR)) throw new Exception("sample data not found!");

        return Directory.EnumerateFiles(SAMPLE_DATA_DIR, "*.png", SearchOption.AllDirectories).Order();
    }
}