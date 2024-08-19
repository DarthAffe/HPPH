#nullable enable

namespace HPPH;

public sealed partial class ColorFormatRGB : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatRGB Instance { get; } = new();

    public int BytesPerPixel => 3;
    
    public string Name => "RGB";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.R, Color.G, Color.B];

    #endregion

    #region Constructors

    private ColorFormatRGB() {}

    #endregion
    
    #region Methods
    
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorRGB.Create(r, g, b, a);
    
    #endregion
}