namespace HPPH;

public partial interface IColorFormat
{
    internal IColor Average(ReadOnlySpan<byte> data);
}