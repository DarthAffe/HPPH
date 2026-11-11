#nullable enable

namespace HPPH;

public sealed partial class ColorFormatARGB : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatARGB Instance { get; } = new();

    public int BytesPerPixel => 4;
    
    public string Name => "ARGB";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.A, Color.R, Color.G, Color.B];

    #endregion

    #region Constructors

    private ColorFormatARGB() {}

    #endregion
    
    #region Methods
    
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorARGB.Create(r, g, b, a);

    /// <inheritdoc cref="IColorFormat.CreateImage" />
    public Image<ColorARGB> CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => Image<ColorARGB>.Create(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.CreateImage(ReadOnlySpan<byte> buffer, int width, int height, int stride) => CreateImage(buffer, width, height, stride);

    /// <inheritdoc cref="IColorFormat.WrapImage" />
    public Image<ColorARGB> WrapImage(byte[] buffer, int width, int height, int stride) => Image<ColorARGB>.Wrap(buffer, width, height, stride);

    /// <inheritdoc />
    IImage IColorFormat.WrapImage(byte[] buffer, int width, int height, int stride) => WrapImage(buffer, width, height, stride);

    #endregion
}