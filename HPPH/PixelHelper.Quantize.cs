using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;

namespace HPPH;

public static partial class PixelHelper
{
    #region Constants

    private const int MIN_PARALLEL_CUBE_SIZE = 64 * 1024;

    #endregion

    #region Methods

    public static IColor[] CreateColorPalette(this IImage image, int paletteSize)
    {
        ArgumentNullException.ThrowIfNull(image);

        int dataLength = image.SizeInBytes;

        if (dataLength <= 1024)
        {
            Span<byte> buffer = stackalloc byte[dataLength];

            image.CopyTo(buffer);
            return image.ColorFormat.CreateColorPalette(buffer, paletteSize);
        }
        else
        {
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
    }

    public static T[] CreateColorPalette<T>(this IImage<T> image, int paletteSize)
        where T : unmanaged, IColor
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().CreateColorPalette(paletteSize);
    }

    public static T[] CreateColorPalette<T>(this RefImage<T> image, int paletteSize)
        where T : unmanaged, IColor
    {
        int dataLength = image.Width * image.Height;
        int sizeInBytes = dataLength * T.ColorFormat.BytesPerPixel;

        if (sizeInBytes <= 1024)
        {
            Span<T> buffer = MemoryMarshal.Cast<byte, T>(stackalloc byte[sizeInBytes]);

            image.CopyTo(buffer);
            return buffer.CreateColorPalette(paletteSize);
        }
        else
        {
            T[] array = ArrayPool<T>.Shared.Rent(dataLength);
            Span<T> buffer = array.AsSpan()[..(dataLength)];
            try
            {
                image.CopyTo(buffer);
                return buffer.CreateColorPalette(paletteSize);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(array);
            }
        }
    }

    public static T[] CreateColorPalette<T>(this ReadOnlySpan<T> colors, int paletteSize)
        where T : unmanaged, IColor
    {
        int sizeInBytes = colors.Length * T.ColorFormat.BytesPerPixel;

        if (sizeInBytes <= 1024)
        {
            Span<T> buffer = MemoryMarshal.Cast<byte, T>(stackalloc byte[sizeInBytes]);

            colors.CopyTo(buffer);
            return buffer.CreateColorPalette(paletteSize);
        }
        else
        {
            T[] buffer = ArrayPool<T>.Shared.Rent(colors.Length);
            try
            {
                Span<T> colorBuffer = buffer.AsSpan()[..colors.Length];
                colors.CopyTo(colorBuffer);

                return colorBuffer.CreateColorPalette(paletteSize);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(buffer);
            }
        }
    }

    public static T[] CreateColorPalette<T>(this Span<T> colors, int paletteSize)
        where T : unmanaged, IColor
    {
        throw new NotImplementedException("This requires some more research and will be implemented later");

        //TODO DarthAffe 21.07.2024: Run some tests how the result to performance ratio is with the options described at http://blog.pkh.me/p/39-improving-color-quantization-heuristics.html

        //if (paletteSize < 0) throw new ArgumentException("PaletteSize can't be < 0", nameof(paletteSize));
        //if (paletteSize == 0) return [];

        //Span<ColorCube<T>> cubes = new ColorCube<T>[paletteSize];
        //cubes[0] = new ColorCube<T>(0, colors.Length, SortTarget.None);

        //for (int i = 0; i < paletteSize; i++)
        //{
        //    int splitCubeIndex = 0;

        //    for (int j = 0; j < paletteSize; j++)
        //    {

        //    }

        //    cubes[splitCubeIndex].Split(colors, out ColorCube<T> a, out ColorCube<T> b);
        //    cubes[splitCubeIndex] = a;
        //    cubes[i + 1] = b;
        //}

        //T[] result = new T[cubes.Length];
        //for (int i = 0; i < cubes.Length; i++)
        //    result[i] = Average(cubes[i].Slice(colors));

        //return result;
    }

    public static IColor[] CreateSimpleColorPalette(this IImage image, int paletteSize)
    {
        ArgumentNullException.ThrowIfNull(image);

        int dataLength = image.SizeInBytes;

        if (dataLength <= 1024)
        {
            Span<byte> buffer = stackalloc byte[dataLength];

            image.CopyTo(buffer);
            return image.ColorFormat.CreateSimpleColorPalette(buffer, paletteSize);
        }
        else
        {
            byte[] array = ArrayPool<byte>.Shared.Rent(dataLength);
            Span<byte> buffer = array.AsSpan()[..dataLength];
            try
            {
                image.CopyTo(buffer);
                return image.ColorFormat.CreateSimpleColorPalette(buffer, paletteSize);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }

    public static T[] CreateSimpleColorPalette<T>(this IImage<T> image, int paletteSize)
        where T : unmanaged, IColor
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().CreateSimpleColorPalette(paletteSize);
    }

    public static T[] CreateSimpleColorPalette<T>(this RefImage<T> image, int paletteSize)
        where T : unmanaged, IColor
    {
        int dataLength = image.Width * image.Height;
        int sizeInBytes = dataLength * T.ColorFormat.BytesPerPixel;

        if (sizeInBytes <= 1024)
        {
            Span<T> buffer = MemoryMarshal.Cast<byte, T>(stackalloc byte[sizeInBytes]);

            image.CopyTo(buffer);
            return buffer.CreateSimpleColorPalette(paletteSize);
        }
        else
        {
            T[] array = ArrayPool<T>.Shared.Rent(dataLength);
            Span<T> buffer = array.AsSpan()[..(dataLength)];
            try
            {
                image.CopyTo(buffer);
                return buffer.CreateSimpleColorPalette(paletteSize);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(array);
            }
        }
    }

    public static T[] CreateSimpleColorPalette<T>(this ReadOnlySpan<T> colors, int paletteSize)
        where T : unmanaged, IColor
    {
        int sizeInBytes = colors.Length * T.ColorFormat.BytesPerPixel;

        if (sizeInBytes <= 1024)
        {
            Span<T> buffer = MemoryMarshal.Cast<byte, T>(stackalloc byte[sizeInBytes]);

            colors.CopyTo(buffer);
            return buffer.CreateSimpleColorPalette(paletteSize);
        }
        else
        {
            T[] buffer = ArrayPool<T>.Shared.Rent(colors.Length);
            try
            {
                Span<T> colorBuffer = buffer.AsSpan()[..colors.Length];
                colors.CopyTo(colorBuffer);

                return colorBuffer.CreateSimpleColorPalette(paletteSize);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(buffer);
            }
        }
    }

    /// <summary>
    /// Creates a palette of up to <paramref name="paletteSize"/> colors by median cut.
    /// </summary>
    /// <remarks>
    /// This overload works in place: it reorders <paramref name="colors"/>. Use the <see cref="ReadOnlySpan{T}"/>
    /// overload to keep the input unchanged. Fewer colors are returned if the input doesn't have enough distinct colors to fill the palette.
    /// </remarks>
    public static T[] CreateSimpleColorPalette<T>(this Span<T> colors, int paletteSize)
        where T : unmanaged, IColor
    {
        if (paletteSize < 0) throw new ArgumentException("PaletteSize can't be < 0", nameof(paletteSize));
        if (paletteSize == 0) return [];

        if (!BitOperations.IsPow2(paletteSize)) throw new ArgumentException("PaletteSize has to be a power of 2", nameof(paletteSize));

        int splits = BitOperations.Log2((uint)paletteSize);

        ColorCube<T>[] cubes = new ColorCube<T>[1 << splits];
        cubes[0] = new ColorCube<T>(0, colors.Length, SortTarget.None);

        for (int i = 0; i < splits; i++)
        {
            int currentCubeCount = 1 << i;

            if ((currentCubeCount > 1) && ((colors.Length / currentCubeCount) >= MIN_PARALLEL_CUBE_SIZE))
                SplitParallel(colors, cubes, currentCubeCount);
            else
            {
                for (int index = 0; index < currentCubeCount; index++)
                {
                    cubes[index].Split(colors, out ColorCube<T> a, out ColorCube<T> b);
                    cubes[index] = a;
                    cubes[currentCubeCount + index] = b;
                }
            }
        }

        int resultCount = cubes.Count(cube => cube.Length > 0);

        T[] result = new T[resultCount];
        int resultIndex = 0;
        foreach (ColorCube<T> cube in cubes)
            if (cube.Length > 0)
                result[resultIndex++] = cube.Slice(colors).Average();

        return result;
    }

    private static unsafe void SplitParallel<T>(Span<T> colors, ColorCube<T>[] cubes, int cubeCount)
        where T : unmanaged, IColor
    {
        fixed (T* colorsPtr = colors)
        {
            nint colorsAddress = (nint)colorsPtr;
            int colorsLength = colors.Length;

            Parallel.For(0, cubeCount, index =>
            {
                cubes[index].Split(new Span<T>((T*)colorsAddress, colorsLength), out ColorCube<T> a, out ColorCube<T> b);
                cubes[index] = a;
                cubes[cubeCount + index] = b;
            });
        }
    }

    #endregion
}
