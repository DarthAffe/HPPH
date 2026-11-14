#nullable enable

namespace HPPH;

/// <summary>
/// Represents the ABGR color-format.
/// </summary>
public sealed partial class ColorFormatABGR : IColorFormat
{
    #region Properties & Fields

    /// <summary>
    /// Gets the instance of the ABGR color-format.
    /// </summary>
    public static ColorFormatABGR Instance { get; } = new();

    /// <inheritdoc />
    public int BytesPerPixel => 4;
    
    /// <inheritdoc />
    public string Name => "ABGR";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.A, Color.B, Color.G, Color.R];

    #endregion

    #region Constructors

    private ColorFormatABGR() {}

    #endregion
    
    #region Methods
    
    /// <inheritdoc />
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorABGR.Create(r, g, b, a);

    /// <inheritdoc cref="IColorFormat.CreateImage" />
    public Image<ColorABGR> CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => Image<ColorABGR>.Create(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => CreateImage(buffer, width, height, stride);

    /// <inheritdoc cref="IColorFormat.WrapImage" />
    public Image<ColorABGR> WrapImage(byte[] buffer, int width, int height, int stride) => Image<ColorABGR>.Wrap(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.WrapImage(byte[] buffer, int width, int height, int stride) => WrapImage(buffer, width, height, stride);

    /// <inheritdoc />
    public override string ToString() => Name;

    #endregion
}