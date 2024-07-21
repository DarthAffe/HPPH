#nullable enable

namespace HPPH;

public sealed partial class ColorFormatRGBA : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatRGBA Instance { get; } = new();

    public int BytesPerPixel => 4;
    
    public string Name => "RGBA";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.R, Color.G, Color.B, Color.A];

    #endregion

    #region Constructors

    private ColorFormatRGBA() {}

    #endregion
}