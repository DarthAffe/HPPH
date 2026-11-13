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

    /// <summary>
    /// Creates a palette of up to <paramref name="paletteSize"/> colors by median cut.
    /// </summary>
    /// <remarks>
    /// <paramref name="paletteSize"/> has to be a power of 2.
    /// The palette is ordered by the number of pixels each color represents, most common first.
    /// Fewer colors are returned if the image doesn't have enough distinct colors to fill the palette.
    /// </remarks>
    /// <param name="image">The image to create the palette for.</param>
    /// <param name="paletteSize">The maximum number of colors in the palette. Has to be a power of 2.</param>
    /// <param name="pixelCounts">
    /// Optional. Receives the number of pixels each palette color represents, at the same index as the color.
    /// Needs at least <paramref name="paletteSize"/> elements, only the first <c>palette.Length</c> are written.
    /// </param>
    /// <returns>The palette.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="paletteSize"/> is negative.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="paletteSize"/> is not a power of 2, or <paramref name="pixelCounts"/> is not empty but shorter than <paramref name="paletteSize"/>.
    /// </exception>
    public static IColor[] CreateMedianCutColorPalette(this IImage image, int paletteSize, Span<long> pixelCounts = default)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<MedianCutPaletteOperation, IColor[]>(new MedianCutPaletteOperation(paletteSize, pixelCounts));
    }

    /// <inheritdoc cref="CreateMedianCutColorPalette(IImage, int, Span{long})"/>
    public static T[] CreateMedianCutColorPalette<T>(this Image<T> image, int paletteSize, Span<long> pixelCounts = default)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().CreateMedianCutColorPalette(paletteSize, pixelCounts);
    }

    /// <inheritdoc cref="CreateMedianCutColorPalette(IImage, int, Span{long})"/>
    public static T[] CreateMedianCutColorPalette<T>(this ReadOnlySpan<T> colors, int paletteSize, Span<long> pixelCounts = default)
        where T : unmanaged, IColor<T>
        => RefImage<T>.Wrap(colors, colors.Length, 1).CreateMedianCutColorPalette(paletteSize, pixelCounts);

    /// <inheritdoc cref="CreateMedianCutColorPalette(IImage, int, Span{long})"/>
    public static T[] CreateMedianCutColorPalette<T>(this RefImage<T> image, int paletteSize, Span<long> pixelCounts = default)
        where T : unmanaged, IColor<T>
    {
        ValidatePaletteArguments(paletteSize, pixelCounts);
        if (paletteSize == 0) return [];
        if (!BitOperations.IsPow2(paletteSize)) throw new ArgumentException("PaletteSize has to be a power of 2", nameof(paletteSize));

        if ((image.Width * image.Height) <= paletteSize)
            return GetDistinctColors(image.ToArray(), pixelCounts);

        int dataLength = image.Width * image.Height;
        if (dataLength == 0) return [];

        // Median cut sorts the colors, so it works on a copy.
        int sizeInBytes = dataLength * T.ColorFormat.BytesPerPixel;
        if (sizeInBytes <= 1024)
        {
            Span<T> buffer = MemoryMarshal.Cast<byte, T>(stackalloc byte[sizeInBytes]);

            image.CopyTo(buffer);
            return MedianCut(buffer, paletteSize, pixelCounts);
        }
        else
        {
            T[] array = ArrayPool<T>.Shared.Rent(dataLength);
            try
            {
                Span<T> buffer = array.AsSpan(0, dataLength);

                image.CopyTo(buffer);
                return MedianCut(buffer, paletteSize, pixelCounts);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(array);
            }
        }
    }

    private static T[] MedianCut<T>(Span<T> colors, int paletteSize, Span<long> pixelCounts)
        where T : unmanaged, IColor<T>
    {
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

        T[] averages = new T[resultCount];
        long[] populations = new long[resultCount];
        int resultIndex = 0;
        foreach (ColorCube<T> cube in cubes)
            if (cube.Length > 0)
            {
                averages[resultIndex] = cube.Slice(colors).Average();
                populations[resultIndex] = cube.Length;
                resultIndex++;
            }

        return ToPalette(averages, populations, pixelCounts);
    }

    private static unsafe void SplitParallel<T>(Span<T> colors, ColorCube<T>[] cubes, int cubeCount)
        where T : unmanaged, IColor<T>
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
    
    private readonly ref struct MedianCutPaletteOperation : IImageOperation<IColor[]>
    {
        private readonly int _paletteSize;
        private readonly Span<long> _pixelCounts;

        public MedianCutPaletteOperation(int paletteSize, Span<long> pixelCounts = default)
        {
            this._paletteSize = paletteSize;
            this._pixelCounts = pixelCounts;
        }

        public IColor[] Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => ToIColors(image.CreateMedianCutColorPalette(_paletteSize, _pixelCounts));
    }
}
