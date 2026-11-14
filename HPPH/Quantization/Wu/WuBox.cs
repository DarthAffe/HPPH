using System.Runtime.CompilerServices;

namespace HPPH;

[SkipLocalsInit]
internal struct WuBox
{
    #region Properties & Fields

    public int R0;
    public int R1;

    public int G0;
    public int G1;

    public int B0;
    public int B1;

    public readonly int Volume => (R1 - R0) * (G1 - G0) * (B1 - B0);

    #endregion

    #region Methods

    public readonly int GetLower(int axis) => axis switch { 0 => R0, 1 => G0, _ => B0 };
    public readonly int GetUpper(int axis) => axis switch { 0 => R1, 1 => G1, _ => B1 };

    public readonly WuBox WithLower(int axis, int value)
    {
        WuBox box = this;
        switch (axis)
        {
            case 0: box.R0 = value; break;
            case 1: box.G0 = value; break;
            default: box.B0 = value; break;
        }

        return box;
    }

    public readonly WuBox WithUpper(int axis, int value)
    {
        WuBox box = this;
        switch (axis)
        {
            case 0: box.R1 = value; break;
            case 1: box.G1 = value; break;
            default: box.B1 = value; break;
        }

        return box;
    }

    #endregion
}