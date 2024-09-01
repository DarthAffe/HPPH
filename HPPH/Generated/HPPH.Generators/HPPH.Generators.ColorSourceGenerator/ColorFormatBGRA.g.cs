#nullable enable

namespace HPPH;

public sealed partial class ColorFormatBGRA : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatBGRA Instance { get; } = new();

    public int BytesPerPixel => 4;
    
    public string Name => "BGRA";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.B, Color.G, Color.R, Color.A];

    #endregion

    #region Constructors

    private ColorFormatBGRA() {}

    #endregion
    
    #region Methods
    
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorBGRA.Create(r, g, b, a);
    
    #endregion
}