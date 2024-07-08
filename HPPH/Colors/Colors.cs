// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming

namespace HPPH;

internal static class Color
{
    internal const int R = 0;
    internal const int G = 1;
    internal const int B = 2;
    internal const int A = 3;
}

[ColorGenerator]
public readonly partial struct ColorRGB;

[ColorGenerator]
public readonly partial struct ColorBGR;

[ColorGenerator]
public readonly partial struct ColorARGB;

[ColorGenerator]
public readonly partial struct ColorABGR;

[ColorGenerator]
public readonly partial struct ColorRGBA;

[ColorGenerator]
public readonly partial struct ColorBGRA;
