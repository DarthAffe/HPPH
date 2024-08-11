using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;

namespace HPPH;

public static partial class PixelHelper
{
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
        ArgumentNullException.ThrowIfNull(image, nameof(image));

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
            return CreateColorPalette(buffer, paletteSize);
        }
        else
        {
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
    }

    public static T[] CreateColorPalette<T>(this ReadOnlySpan<T> colors, int paletteSize)
        where T : unmanaged, IColor
    {
        int sizeInBytes = colors.Length * T.ColorFormat.BytesPerPixel;

        if (sizeInBytes <= 1024)
        {
            Span<T> buffer = MemoryMarshal.Cast<byte, T>(stackalloc byte[sizeInBytes]);

            colors.CopyTo(buffer);
            return CreateColorPalette(buffer, paletteSize);
        }
        else
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
        ArgumentNullException.ThrowIfNull(image, nameof(image));

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
            return CreateSimpleColorPalette(buffer, paletteSize);
        }
        else
        {
            T[] array = ArrayPool<T>.Shared.Rent(dataLength);
            Span<T> buffer = array.AsSpan()[..(dataLength)];
            try
            {
                image.CopyTo(buffer);
                return CreateSimpleColorPalette(buffer, paletteSize);
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
            return CreateSimpleColorPalette(buffer, paletteSize);
        }
        else
        {
            T[] buffer = ArrayPool<T>.Shared.Rent(colors.Length);
            try
            {
                Span<T> colorBuffer = buffer.AsSpan()[..colors.Length];
                colors.CopyTo(colorBuffer);

                return CreateSimpleColorPalette(colorBuffer, paletteSize);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(buffer);
            }
        }
    }

    public static unsafe T[] CreateSimpleColorPalette<T>(this Span<T> colors, int paletteSize)
        where T : unmanaged, IColor
    {
        if (paletteSize < 0) throw new ArgumentException("PaletteSize can't be < 0", nameof(paletteSize));
        if (paletteSize == 0) return [];

        if (!BitOperations.IsPow2(paletteSize)) throw new ArgumentException("PaletteSize has to be a power of 2", nameof(paletteSize));

        int splits = BitOperations.Log2((uint)paletteSize);

        ColorCube<T>[] cubes = new ColorCube<T>[1 << splits];
        cubes[0] = new ColorCube<T>(0, colors.Length, SortTarget.None);

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

        int colorsLength = colors.Length;
        fixed (T* colorsPtr = colors)
        {
            //HACK DarthAffe 21.07.2024: This is dangerous and needs to be treated carefully! It's used in the anonymous method used to run the splits in parallel and absolutely can't be used outside this fixed block!
            T* unsafeColorsPtr = colorsPtr;

            for (int i = 0; i < splits; i++)
            {
                int currentCubeCount = 1 << i;

                Parallel.For(0, currentCubeCount, parallelOptions, CreateCubes);

                void CreateCubes(int index)
                {
                    cubes[index].Split(new Span<T>(unsafeColorsPtr, colorsLength), out ColorCube<T> a, out ColorCube<T> b);
                    cubes[index] = a;
                    cubes[currentCubeCount + index] = b;
                }
            }
        }

        T[] result = new T[cubes.Length];
        for (int i = 0; i < cubes.Length; i++)
            result[i] = Average(cubes[i].Slice(colors));

        return result;
    }

    #endregion
}
