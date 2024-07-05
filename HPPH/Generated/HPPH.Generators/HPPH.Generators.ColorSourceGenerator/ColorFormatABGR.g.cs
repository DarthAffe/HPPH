namespace HPPH;

public sealed partial class ColorFormatABGR : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatABGR Instance { get; } = new();

    public int BytesPerPixel => 4;
    
    public string Name => "ABGR";

    #endregion

    #region Constructors

    private ColorFormatABGR() {}

    #endregion
}