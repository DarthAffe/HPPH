namespace HPPH;

/// <summary>
/// Represents a generic color made of 4 bytes (alpha, red, green and blue)
/// </summary>
public interface IColor : IEquatable<IColor>
{
    /// <summary>
    /// Gets the red-component of this color.
    /// </summary>
    byte R { get; }

    /// <summary>
    /// Gets the green-component of this color.
    /// </summary>
    byte G { get; }

    /// <summary>
    /// Gets the blue-component of this color.
    /// </summary>
    byte B { get; }

    /// <summary>
    /// Gets the alpha-component of this color.
    /// </summary>
    byte A { get; }

    /// <summary>
    /// Gets the color-format of this color.
    /// </summary>
    public static virtual IColorFormat ColorFormat => throw new NotSupportedException();

    public static virtual IColor Create(byte r, byte g, byte b, byte a) => throw new NotSupportedException();
}