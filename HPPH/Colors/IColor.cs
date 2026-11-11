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
}

/// <summary>
/// Represents a color of the concrete type <typeparamref name="TSelf"/>.
/// </summary>
/// <typeparam name="TSelf">The implementing color type itself.</typeparam>
public interface IColor<out TSelf> : IColor
    where TSelf : unmanaged, IColor<TSelf>
{
    /// <summary>
    /// Gets the color-format of this color.
    /// </summary>
    static abstract IColorFormat ColorFormat { get; }

    /// <summary>
    /// Creates a color of this type from the given components.
    /// Components this type doesn't contain are ignored.
    /// </summary>
    /// <param name="r">The red-component of the color.</param>
    /// <param name="g">The green-component of the color.</param>
    /// <param name="b">The blue-component of the color.</param>
    /// <param name="a">The alpha-component of the color.</param>
    /// <returns>The created color.</returns>
    static abstract TSelf Create(byte r, byte g, byte b, byte a);
}