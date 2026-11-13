using System.Numerics;

namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    #region MedianCut

    public static T[] CreateMedianCutColorPalette<T>(Image<T> image, int paletteSize)
        where T : unmanaged, IColor<T>
        => CreateMedianCutColorPalette<T>(image.ToArray(), paletteSize);

    public static T[] CreateMedianCutColorPalette<T>(RefImage<T> image, int paletteSize)
        where T : unmanaged, IColor<T>
        => CreateMedianCutColorPalette<T>(image.ToArray(), paletteSize);

    public static T[] CreateMedianCutColorPalette<T>(Span<T> colors, int paletteSize)
        where T : unmanaged, IColor<T> =>
        CreateMedianCutColorPalette<T>(colors.ToArray().Cast<IColor>().ToArray(), paletteSize).Select(x => T.Create(x.R, x.G, x.B, x.A)).ToArray();

    private static IColor[] CreateMedianCutColorPalette<T>(IColor[] colors, int paletteSize)
        where T : unmanaged, IColor<T>
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

        return ToPalette(cubes.Where(c => c.Count > 0).Select(c => (c.GetAverageColor<T>(), (long)c.Count)));
    }

    private static TColor[] ToPalette<TColor>(IEnumerable<(TColor Color, long Population)> colors)
        => [.. colors.GroupBy(x => x.Color)
                     .Select(group => (Color: group.Key, Population: group.Sum(x => x.Population)))
                     .OrderByDescending(x => x.Population)
                     .Select(x => x.Color)];


    internal class ColorCube
    {
        #region Properties & Fields

        private readonly List<IColor> _colors;

        internal int Count => _colors.Count;

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
            where T : unmanaged, IColor<T>
        {
            if (_colors.Count == 0) return T.Create(0, 0, 0, 0);

            int r = _colors.Sum(x => x.R);
            int g = _colors.Sum(x => x.G);
            int b = _colors.Sum(x => x.B);
            int a = _colors.Sum(x => x.A);

            return T.Create((byte)MathF.Round(r / (float)_colors.Count),
                (byte)MathF.Round(g / (float)_colors.Count),
                (byte)MathF.Round(b / (float)_colors.Count),
                (byte)MathF.Round(a / (float)_colors.Count));
        }

        #endregion
    }

    #endregion

    #region Wu

    public static T[] CreateWuColorPalette<T>(Image<T> image, int paletteSize)
        where T : unmanaged, IColor<T>
        => CreateWuColorPalette<T>(image.ToArray(), paletteSize);

    public static T[] CreateWuColorPalette<T>(RefImage<T> image, int paletteSize)
        where T : unmanaged, IColor<T>
        => CreateWuColorPalette<T>(image.ToArray(), paletteSize);

    public static T[] CreateWuColorPalette<T>(Span<T> colors, int paletteSize)
        where T : unmanaged, IColor<T>
    {
        if (paletteSize < 0) throw new ArgumentException("PaletteSize can't be < 0", nameof(paletteSize));
        if ((paletteSize == 0) || colors.IsEmpty) return [];

        if (colors.Length <= paletteSize) return ToPalette(colors.ToArray().Select(color => (color, 1L)));

        List<WuCell> cells = [.. colors.ToArray()
                                       .GroupBy(color => (color.R >> 3, color.G >> 3, color.B >> 3))
                                       .Select(group => new WuCell([group.Key.Item1, group.Key.Item2, group.Key.Item3],
                                                                   WuStats.Sum(group.Select(color => WuStats.Of(color)))))];

        List<WuBox> boxes = [new(cells)];

        while (boxes.Count < paletteSize)
        {
            WuBox? box = boxes.Where(b => b.Cut != null).OrderByDescending(b => b.Variance).FirstOrDefault();
            if (box == null) break;

            (int axis, int position) = box.Cut!.Value;
            boxes[boxes.IndexOf(box)] = new WuBox([.. box.Cells.Where(cell => cell.Index[axis] < position)]);
            boxes.Add(new WuBox([.. box.Cells.Where(cell => cell.Index[axis] >= position)]));
        }

        return ToPalette(boxes.Select(box => (T.Create((byte)MathF.Round(box.Stats.R / (float)box.Stats.Count),
                                                       (byte)MathF.Round(box.Stats.G / (float)box.Stats.Count),
                                                       (byte)MathF.Round(box.Stats.B / (float)box.Stats.Count),
                                                       (byte)MathF.Round(box.Stats.A / (float)box.Stats.Count)),
                                              box.Stats.Count)));
    }

    internal readonly record struct WuStats(long Count, long R, long G, long B, long A, long Squares)
    {
        public static WuStats Of(IColor color)
            => new(1, color.R, color.G, color.B, color.A, (color.R * color.R) + (color.G * color.G) + (color.B * color.B));

        public static WuStats Sum(IEnumerable<WuStats> stats)
            => stats.Aggregate(new WuStats(), (a, b) => new WuStats(a.Count + b.Count, a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A, a.Squares + b.Squares));
    }

    internal sealed record WuCell(int[] Index, WuStats Stats);

    internal sealed class WuBox
    {
        public List<WuCell> Cells { get; }

        public WuStats Stats { get; }

        public double Variance { get; }

        public (int Axis, int Position)? Cut { get; }

        public WuBox(List<WuCell> cells)
        {
            this.Cells = cells;
            
            Stats = WuStats.Sum(cells.Select(cell => cell.Stats));

            Int128 numerator = ((Int128)Stats.Squares * Stats.Count) - ((Int128)Stats.R * Stats.R) - ((Int128)Stats.G * Stats.G) - ((Int128)Stats.B * Stats.B);
            Variance = (double)numerator / Stats.Count;

            Cut = FindCut(cells);
        }

        private static (int Axis, int Position)? FindCut(List<WuCell> cells)
        {
            (int Axis, int Position)? cut = null;
            double maxScore = 0;

            for (int axis = 0; axis < 3; axis++)
                for (int position = 1; position < 32; position++)
                {
                    WuStats lower = WuStats.Sum(cells.Where(cell => cell.Index[axis] < position).Select(cell => cell.Stats));
                    WuStats upper = WuStats.Sum(cells.Where(cell => cell.Index[axis] >= position).Select(cell => cell.Stats));
                    if ((lower.Count == 0) || (upper.Count == 0)) continue;

                    double score = Score(lower) + Score(upper);
                    if ((cut == null) || (score > maxScore))
                    {
                        cut = (axis, position);
                        maxScore = score;
                    }
                }

            return cut;
        }

        private static double Score(WuStats stats)
            => (((double)stats.R * stats.R) + ((double)stats.G * stats.G) + ((double)stats.B * stats.B)) / stats.Count;
    }

    #endregion

    #endregion
}