#nullable enable

namespace HPPH;

/// <summary>
/// Represents the BGR color-format.
/// </summary>
public sealed partial class ColorFormatBGR : IColorFormat
{
    #region Properties & Fields

    /// <summary>
    /// Gets the instance of the BGR color-format.
    /// </summary>
    public static ColorFormatBGR Instance { get; } = new();

    /// <inheritdoc />
    public int BytesPerPixel => 3;
    
    /// <inheritdoc />
    public string Name => "BGR";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.B, Color.G, Color.R];

    #endregion

    #region Constructors

    private ColorFormatBGR() {}

    #endregion
    
    #region Methods
    
    /// <inheritdoc />
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorBGR.Create(r, g, b, a);

    /// <inheritdoc cref="IColorFormat.CreateImage" />
    public Image<ColorBGR> CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => Image<ColorBGR>.Create(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => CreateImage(buffer, width, height, stride);

    /// <inheritdoc cref="IColorFormat.WrapImage" />
    public Image<ColorBGR> WrapImage(byte[] buffer, int width, int height, int stride) => Image<ColorBGR>.Wrap(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.WrapImage(byte[] buffer, int width, int height, int stride) => WrapImage(buffer, width, height, stride);

    /// <inheritdoc />
    public override string ToString() => Name;

    #endregion
}