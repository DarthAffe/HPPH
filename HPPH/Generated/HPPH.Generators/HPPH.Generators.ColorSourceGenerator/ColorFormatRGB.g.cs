#nullable enable

namespace HPPH;

/// <summary>
/// Represents the RGB color-format.
/// </summary>
public sealed partial class ColorFormatRGB : IColorFormat
{
    #region Properties & Fields

    /// <summary>
    /// Gets the instance of the RGB color-format.
    /// </summary>
    public static ColorFormatRGB Instance { get; } = new();

    /// <inheritdoc />
    public int BytesPerPixel => 3;
    
    /// <inheritdoc />
    public string Name => "RGB";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.R, Color.G, Color.B];

    #endregion

    #region Constructors

    private ColorFormatRGB() {}

    #endregion
    
    #region Methods
    
    /// <inheritdoc />
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorRGB.Create(r, g, b, a);

    /// <inheritdoc cref="IColorFormat.CreateImage" />
    public Image<ColorRGB> CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => Image<ColorRGB>.Create(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => CreateImage(buffer, width, height, stride);

    /// <inheritdoc cref="IColorFormat.WrapImage" />
    public Image<ColorRGB> WrapImage(byte[] buffer, int width, int height, int stride) => Image<ColorRGB>.Wrap(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.WrapImage(byte[] buffer, int width, int height, int stride) => WrapImage(buffer, width, height, stride);

    /// <inheritdoc />
    public override string ToString() => Name;

    #endregion
}