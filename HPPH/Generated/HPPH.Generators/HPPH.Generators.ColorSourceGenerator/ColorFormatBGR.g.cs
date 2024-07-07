namespace HPPH;

public sealed partial class ColorFormatBGR : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatBGR Instance { get; } = new();

    public int BytesPerPixel => 3;
    
    public string Name => "BGR";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [2, 1, 0];

    #endregion

    #region Constructors

    private ColorFormatBGR() {}

    #endregion
}