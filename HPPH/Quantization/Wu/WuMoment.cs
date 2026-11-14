using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HPPH;

[SkipLocalsInit]
[StructLayout(LayoutKind.Sequential)]
internal struct WuMoment
{
    #region Properties & Fields

    public long Weight;
    public long R;
    public long G;
    public long B;
    public long A;
    public long Squares;

    #endregion

    #region Operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static WuMoment operator +(in WuMoment left, in WuMoment right)
        => new()
        {
            Weight = left.Weight + right.Weight,
            R = left.R + right.R,
            G = left.G + right.G,
            B = left.B + right.B,
            A = left.A + right.A,
            Squares = left.Squares + right.Squares
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static WuMoment operator -(in WuMoment left, in WuMoment right)
        => new()
        {
            Weight = left.Weight - right.Weight,
            R = left.R - right.R,
            G = left.G - right.G,
            B = left.B - right.B,
            A = left.A - right.A,
            Squares = left.Squares - right.Squares
        };

    #endregion
}