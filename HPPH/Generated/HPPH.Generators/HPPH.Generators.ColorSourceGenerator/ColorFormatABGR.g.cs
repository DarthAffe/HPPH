#nullable enable

namespace HPPH;

public sealed partial class ColorFormatABGR : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatABGR Instance { get; } = new();

    public int BytesPerPixel => 4;
    
    public string Name => "ABGR";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.A, Color.B, Color.G, Color.R];

    #endregion

    #region Constructors

    private ColorFormatABGR() {}

    #endregion
    
    #region Methods
    
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorABGR.Create(r, g, b, a);
    
    #endregion
}