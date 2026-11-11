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

    /// <inheritdoc cref="IColorFormat.CreateImage" />
    public Image<ColorBGRA> CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => Image<ColorBGRA>.Create(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => CreateImage(buffer, width, height, stride);

    /// <inheritdoc cref="IColorFormat.WrapImage" />
    public Image<ColorBGRA> WrapImage(byte[] buffer, int width, int height, int stride) => Image<ColorBGRA>.Wrap(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.WrapImage(byte[] buffer, int width, int height, int stride) => WrapImage(buffer, width, height, stride);

    #endregion
}