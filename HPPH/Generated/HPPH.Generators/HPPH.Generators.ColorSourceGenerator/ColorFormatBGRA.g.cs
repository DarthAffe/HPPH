namespace HPPH;

public sealed partial class ColorFormatBGRA : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatBGRA Instance { get; } = new();

    public int BytesPerPixel => 4;
    
    public string Name => "BGRA";

    #endregion

    #region Constructors

    private ColorFormatBGRA() {}

    #endregion
}