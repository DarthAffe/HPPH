namespace HPPH;

public sealed partial class ColorFormatRGB : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatRGB Instance { get; } = new();

    public int BytesPerPixel => 3;
    
    public string Name => "RGB";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [0, 1, 2];

    #endregion

    #region Constructors

    private ColorFormatRGB() {}

    #endregion
}