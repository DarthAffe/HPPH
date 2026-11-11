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
    
    #region Methods
    
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorRGBA.Create(r, g, b, a);

    /// <inheritdoc cref="IColorFormat.CreateImage" />
    public Image<ColorRGBA> CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => Image<ColorRGBA>.Create(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => CreateImage(buffer, width, height, stride);

    /// <inheritdoc cref="IColorFormat.WrapImage" />
    public Image<ColorRGBA> WrapImage(byte[] buffer, int width, int height, int stride) => Image<ColorRGBA>.Wrap(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.WrapImage(byte[] buffer, int width, int height, int stride) => WrapImage(buffer, width, height, stride);

    #endregion
}