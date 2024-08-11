#pragma warning disable CA1416

using HPPH.System.Drawing;

namespace HPPH.Benchmark;

internal static class BenchmarkHelper
{
    private const string SAMPLE_DATA_DIR = @"..\..\..\..\..\..\..\..\sample_data";

    public static List<T[]> GetSampleData<T>()
        where T : struct, IColor
    {
        if (!Directory.Exists(SAMPLE_DATA_DIR)) throw new Exception("sample data not found!");

        List<T[]> colors = [];

        IEnumerable<string> files = Directory.EnumerateFiles(SAMPLE_DATA_DIR, "*.png", SearchOption.AllDirectories);
        foreach (string file in files)
            colors.Add(ImageHelper.LoadImage(file).ConvertTo<T>().ToArray());

        return colors;
    }
}