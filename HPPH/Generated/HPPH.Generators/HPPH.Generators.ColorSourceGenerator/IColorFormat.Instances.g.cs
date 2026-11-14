#nullable enable

namespace HPPH;

public partial interface IColorFormat
{
    #region Instances

    /// <summary>
    /// Gets the RGB color-format.
    /// </summary>
    public static ColorFormatRGB RGB => ColorFormatRGB.Instance;

    /// <summary>
    /// Gets the BGR color-format.
    /// </summary>
    public static ColorFormatBGR BGR => ColorFormatBGR.Instance;

    /// <summary>
    /// Gets the ARGB color-format.
    /// </summary>
    public static ColorFormatARGB ARGB => ColorFormatARGB.Instance;

    /// <summary>
    /// Gets the ABGR color-format.
    /// </summary>
    public static ColorFormatABGR ABGR => ColorFormatABGR.Instance;

    /// <summary>
    /// Gets the RGBA color-format.
    /// </summary>
    public static ColorFormatRGBA RGBA => ColorFormatRGBA.Instance;

    /// <summary>
    /// Gets the BGRA color-format.
    /// </summary>
    public static ColorFormatBGRA BGRA => ColorFormatBGRA.Instance;

    /// <summary>
    /// Creates a color of this color-format from the given components.
    /// Components this color-format doesn't contain are ignored.
    /// </summary>
    /// <param name="r">The red-component of the color.</param>
    /// <param name="g">The green-component of the color.</param>
    /// <param name="b">The blue-component of the color.</param>
    /// <param name="a">The alpha-component of the color.</param>
    /// <returns>The created color.</returns>
    IColor CreateColor(byte r, byte g, byte b, byte a);

    #endregion
}
