#nullable enable

namespace HPPH;

public sealed partial class ColorFormatBGR : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatBGR Instance { get; } = new();

    public int BytesPerPixel => 3;
    
    public string Name => "BGR";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.B, Color.G, Color.R];

    #endregion

    #region Constructors

    private ColorFormatBGR() {}

    #endregion
    
    #region Methods
    
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorBGR.Create(r, g, b, a);

    /// <inheritdoc cref="IColorFormat.CreateImage" />
    public Image<ColorBGR> CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => Image<ColorBGR>.Create(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => CreateImage(buffer, width, height, stride);

    /// <inheritdoc cref="IColorFormat.WrapImage" />
    public Image<ColorBGR> WrapImage(byte[] buffer, int width, int height, int stride) => Image<ColorBGR>.Wrap(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.WrapImage(byte[] buffer, int width, int height, int stride) => WrapImage(buffer, width, height, stride);

    #endregion
}