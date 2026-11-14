using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

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

    // Joins the lower and upper 64 bits of two vectors: (a0, b0) and (a1, b1). Vector64 is only accelerated on ARM, so x86 needs its own instruction.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ulong> JoinLower(Vector128<ulong> a, Vector128<ulong> b)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackLow(a, b);

        if (AdvSimd.Arm64.IsSupported)
            return AdvSimd.Arm64.ZipLow(a, b);

        return Vector128.Create(a.GetLower(), b.GetLower());
    }

    /// <inheritdoc cref="JoinLower"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ulong> JoinUpper(Vector128<ulong> a, Vector128<ulong> b)
    {
        if (Sse2.IsSupported)
            return Sse2.UnpackHigh(a, b);

        if (AdvSimd.Arm64.IsSupported)
            return AdvSimd.Arm64.ZipHigh(a, b);

        return Vector128.Create(a.GetUpper(), b.GetUpper());
    }
}
