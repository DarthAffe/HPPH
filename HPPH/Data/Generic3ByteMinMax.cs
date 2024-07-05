namespace HPPH;

internal readonly struct Generic3ByteMinMax(byte b1Min, byte b1Max, byte b2Min, byte b2Max, byte b3Min, byte b3Max)
{
    public readonly byte B1Min = b1Min;
    public readonly byte B1Max = b1Max;

    public readonly byte B2Min = b2Min;
    public readonly byte B2Max = b2Max;

    public readonly byte B3Min = b3Min;
    public readonly byte B3Max = b3Max;

    public byte B1Range => (byte)(B1Max - B1Min);
    public byte B2Range => (byte)(B2Max - B2Min);
    public byte B3Range => (byte)(B3Max - B3Min);
}
