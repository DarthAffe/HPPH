using System.Numerics;

namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static T[] CreateSimpleColorPalette<T>(Image<T> image, int paletteSize)
        where T : unmanaged, IColor
        => CreateSimpleColorPalette<T>(image.ToArray(), paletteSize);

    public static T[] CreateSimpleColorPalette<T>(RefImage<T> image, int paletteSize)
        where T : unmanaged, IColor
        => CreateSimpleColorPalette<T>(image.ToArray(), paletteSize);

    public static T[] CreateSimpleColorPalette<T>(Span<T> colors, int paletteSize)
        where T : unmanaged, IColor =>
        CreateSimpleColorPalette<T>(colors.ToArray().Cast<IColor>().ToArray(), paletteSize).Select(x => T.Create(x.R, x.G, x.B, x.A)).Cast<T>().ToArray();

    private static IColor[] CreateSimpleColorPalette<T>(IColor[] colors, int paletteSize)
        where T : struct, IColor
    {
        if (paletteSize == 0) return [];

        int splits = BitOperations.Log2((uint)paletteSize);

        HashSet<ColorCube> cubes = [new ColorCube(colors)];

        for (int i = 0; i < splits; i++)
        {
            HashSet<ColorCube> currentCubes = [.. cubes];
            foreach (ColorCube currentCube in currentCubes)
            {
                currentCube.Split(out ColorCube a, out ColorCube b);
                cubes.Remove(currentCube);
                cubes.Add(a);
                cubes.Add(b);
            }
        }

        return cubes.Select(c => c.GetAverageColor<T>()).ToArray();
    }

    #endregion
}

internal class ColorCube
{
    #region Properties & Fields

    private readonly List<IColor> _colors;

    #endregion

    #region Constructors

    internal ColorCube(IList<IColor> colors)
    {
        if (colors.Count == 0)
        {
            _colors = [];
            return;
        }

        int redRange = colors.Max(c => c.R) - colors.Min(c => c.R);
        int greenRange = colors.Max(c => c.G) - colors.Min(c => c.G);
        int blueRange = colors.Max(c => c.B) - colors.Min(c => c.B);

        if ((redRange > greenRange) && (redRange > blueRange))
            _colors = [.. colors.OrderBy(a => a.R)];
        else if (greenRange > blueRange)
            _colors = [.. colors.OrderBy(a => a.G)];
        else
            _colors = [.. colors.OrderBy(a => a.B)];
    }

    #endregion

    #region Methods

    internal void Split(out ColorCube a, out ColorCube b)
    {
        int median = _colors.Count / 2;

        a = new ColorCube(_colors.GetRange(0, median));
        b = new ColorCube(_colors.GetRange(median, _colors.Count - median));
    }

    internal IColor GetAverageColor<T>()
        where T : struct, IColor
    {
        if (_colors.Count == 0) return T.Create(0, 0, 0, 0);

        int r = _colors.Sum(x => x.R);
        int g = _colors.Sum(x => x.G);
        int b = _colors.Sum(x => x.B);
        int a = _colors.Sum(x => x.A);

        return T.Create((byte)(r / _colors.Count),
                        (byte)(g / _colors.Count),
                        (byte)(b / _colors.Count),
                        (byte)(a / _colors.Count));
    }

    #endregion
}