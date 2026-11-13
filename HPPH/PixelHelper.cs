using System.Runtime.CompilerServices;

namespace HPPH;

/// <summary>
/// Contains extension methods to process images and colors.
/// </summary>
[SkipLocalsInit]
public static partial class PixelHelper
{
    private static IColor[] ToIColors<T>(ReadOnlySpan<T> colors)
        where T : unmanaged, IColor<T>
    {
        IColor[] result = new IColor[colors.Length];
        for (int i = 0; i < colors.Length; i++)
            result[i] = colors[i];

        return result;
    }
}
