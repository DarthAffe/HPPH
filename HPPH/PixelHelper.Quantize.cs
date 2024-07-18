using System.Buffers;
using System.Numerics;

namespace HPPH;

public static partial class PixelHelper
{
    #region Methods

    public static IColor[] CreateColorPalette(this IImage image, int paletteSize)
    {
        ArgumentNullException.ThrowIfNull(image);

        int dataLength = image.SizeInBytes;
        byte[] array = ArrayPool<byte>.Shared.Rent(dataLength);
        Span<byte> buffer = array.AsSpan()[..dataLength];
        try
        {
            image.CopyTo(buffer);
            return image.ColorFormat.CreateColorPalette(buffer, paletteSize);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public static T[] CreateColorPalette<T>(this IImage<T> image, int paletteSize)
        where T : unmanaged, IColor
        => image.AsRefImage().CreateColorPalette(paletteSize);

    public static T[] CreateColorPalette<T>(this RefImage<T> image, int paletteSize)
        where T : unmanaged, IColor
    {
        int dataLength = image.Width * image.Height;
        T[] array = ArrayPool<T>.Shared.Rent(dataLength);
        Span<T> buffer = array.AsSpan()[..(dataLength)];
        try
        {
            image.CopyTo(buffer);
            return CreateColorPalette(buffer, paletteSize);
        }
        finally
        {
            ArrayPool<T>.Shared.Return(array);
        }
    }

    public static T[] CreateColorPalette<T>(this ReadOnlySpan<T> colors, int paletteSize)
        where T : unmanaged, IColor
    {
        T[] buffer = ArrayPool<T>.Shared.Rent(colors.Length);
        try
        {
            Span<T> colorBuffer = buffer.AsSpan()[..colors.Length];
            colors.CopyTo(colorBuffer);

            return CreateColorPalette(colorBuffer, paletteSize);
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer);
        }
    }

    public static T[] CreateColorPalette<T>(this Span<T> colors, int paletteSize)
        where T : unmanaged, IColor
    {
        int splits = BitOperations.Log2((uint)paletteSize);

        Span<ColorCube<T>> cubes = new ColorCube<T>[1 << splits];
        cubes[0] = new ColorCube<T>(0, colors.Length, SortTarget.None);

        int currentIndex = 0;
        for (int i = 0; i < splits; i++)
        {
            int currentCubeCount = 1 << i;
            Span<ColorCube<T>> currentCubes = cubes[..currentCubeCount];
            for (int j = 0; j < currentCubes.Length; j++)
            {
                currentCubes[j].Split(colors, out ColorCube<T> a, out ColorCube<T> b);
                currentCubes[j] = a;
                cubes[++currentIndex] = b;
            }
        }

        T[] result = new T[cubes.Length];
        for (int i = 0; i < cubes.Length; i++)
            result[i] = Average(cubes[i].Slice(colors));

        return result;
    }

    #endregion
}
