namespace HPPH;

public partial interface IColorFormat
{
    internal ISum Sum(ReadOnlySpan<byte> data);
}