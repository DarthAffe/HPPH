using System.Runtime.CompilerServices;

namespace HPPH;

[SkipLocalsInit]
public static partial class PixelHelper
{
    private static readonly ParallelOptions PARALLEL_OPTIONS = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
}
