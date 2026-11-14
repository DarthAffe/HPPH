using System.Collections.Concurrent;
using System.Drawing;

namespace HPPH.Test;

internal static class ImageHelper
{
    #region Properties & Fields

    private static readonly ConcurrentDictionary<string, Lazy<CachedImage<ColorRGBA>>> _decoded = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Methods

    public static T[] GetColorsFromImage<T>(string file)
        where T : unmanaged, IColor<T>
        => GetCached<T>(file).Colors.AsSpan().ToArray();

    public static Image<T> GetImage<T>(string file)
        where T : unmanaged, IColor<T>
    {
        CachedImage<T> cached = GetCached<T>(file);
        return Image<T>.Create(cached.Colors, cached.Width, cached.Height);
    }

    private static CachedImage<T> GetCached<T>(string file)
        where T : unmanaged, IColor<T>
    {
        string path = Path.GetFullPath(file);

        if (typeof(T) == typeof(ColorRGBA)) return (CachedImage<T>)(object)GetDecoded(path);

        return Cache<T>.Images.GetOrAdd(path, p => new Lazy<CachedImage<T>>(() => ToFormat<T>(GetDecoded(p)))).Value;
    }

    private static CachedImage<ColorRGBA> GetDecoded(string path)
        => _decoded.GetOrAdd(path, p => new Lazy<CachedImage<ColorRGBA>>(() => Decode(p))).Value;

    private static CachedImage<ColorRGBA> Decode(string path)
    {
        using FileStream stream = File.OpenRead(path);
        using Bitmap bmp = new(stream);

        ColorRGBA[] colors = new ColorRGBA[bmp.Width * bmp.Height];
        int i = 0;
        for (int y = 0; y < bmp.Height; y++)
            for (int x = 0; x < bmp.Width; x++)
            {
                Color color = bmp.GetPixel(x, y);
                colors[i++] = new ColorRGBA(color.R, color.G, color.B, color.A);
            }

        return new CachedImage<ColorRGBA>(bmp.Width, bmp.Height, colors);
    }

    private static CachedImage<T> ToFormat<T>(CachedImage<ColorRGBA> decoded)
        where T : unmanaged, IColor<T>
    {
        T[] colors = new T[decoded.Colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            ColorRGBA color = decoded.Colors[i];
            colors[i] = T.Create(color.R, color.G, color.B, color.A);
        }

        return new CachedImage<T>(decoded.Width, decoded.Height, colors);
    }

    #endregion

    private sealed record CachedImage<T>(int Width, int Height, T[] Colors);

    private static class Cache<T>
        where T : unmanaged, IColor<T>
    {
        public static readonly ConcurrentDictionary<string, Lazy<CachedImage<T>>> Images = new(StringComparer.OrdinalIgnoreCase);
    }
}
