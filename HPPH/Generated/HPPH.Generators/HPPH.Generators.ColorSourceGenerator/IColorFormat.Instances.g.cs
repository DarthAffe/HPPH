#nullable enable

namespace HPPH;

public partial interface IColorFormat
{
    #region Instances

    public static ColorFormatRGB RGB => ColorFormatRGB.Instance;
    public static ColorFormatBGR BGR => ColorFormatBGR.Instance;
    public static ColorFormatARGB ARGB => ColorFormatARGB.Instance;
    public static ColorFormatABGR ABGR => ColorFormatABGR.Instance;
    public static ColorFormatRGBA RGBA => ColorFormatRGBA.Instance;
    public static ColorFormatBGRA BGRA => ColorFormatBGRA.Instance;

    IColor CreateColor(byte r, byte g, byte b, byte a);

    #endregion
}
