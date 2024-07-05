namespace HPPH.Reference;

internal static class HPPHExtensions
{
    public static byte GetByteValueFromPercentage(this float percentage)
    {
        if (float.IsNaN(percentage) || (percentage < 0)) return 0;

        return (byte)(percentage >= 1.0f ? 255 : percentage * 256.0f);
    }

    public static float GetPercentageFromByteValue(this byte value)
        => value == 255 ? 1.0f : (value / 256.0f);
}