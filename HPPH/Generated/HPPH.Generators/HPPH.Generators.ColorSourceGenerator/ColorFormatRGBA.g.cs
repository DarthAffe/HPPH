#nullable enable

namespace HPPH;

/// <summary>
/// Represents the RGBA color-format.
/// </summary>
public sealed partial class ColorFormatRGBA : IColorFormat
{
    #region Properties & Fields

    /// <summary>
    /// Gets the instance of the RGBA color-format.
    /// </summary>
    public static ColorFormatRGBA Instance { get; } = new();

    /// <inheritdoc />
    public int BytesPerPixel => 4;
    
    /// <inheritdoc />
    public string Name => "RGBA";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.R, Color.G, Color.B, Color.A];

    #endregion

    #region Constructors

    private ColorFormatRGBA() {}

    #endregion
    
    #region Methods
    
    /// <inheritdoc />
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorRGBA.Create(r, g, b, a);

    /// <inheritdoc cref="IColorFormat.CreateImage" />
    public Image<ColorRGBA> CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => Image<ColorRGBA>.Create(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => CreateImage(buffer, width, height, stride);

    /// <inheritdoc cref="IColorFormat.WrapImage" />
    public Image<ColorRGBA> WrapImage(byte[] buffer, int width, int height, int stride) => Image<ColorRGBA>.Wrap(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.WrapImage(byte[] buffer, int width, int height, int stride) => WrapImage(buffer, width, height, stride);

    /// <inheritdoc />
    public override string ToString() => Name;

    #endregion
}