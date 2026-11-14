using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace HPPH;

public static partial class PixelHelper
{
    #region Constants

    private const int DOWNSCALE_BAND_HEIGHT = 8;

    // pmaddubsw adds two bytes into a short, so a short can take the sums of 128 vectors.
    private const int DOWNSCALE_MAX_SHORT_SUMS = 128;
    private const int DOWNSCALE_MAX_WIDE_BLOCK_WIDTH = 1023;

    // The sums of wide blocks have to fit into an int.
    private const int DOWNSCALE_MAX_WIDE_BLOCK_COUNT = 1 << 23;
    private const int DOWNSCALE_WIDE_SEGMENT = 256;

    // 257 * 255 is the largest sum that fits into an ushort.
    private const int DOWNSCALE_MAX_USHORT_COUNT = 257;
    private const int DOWNSCALE_SEGMENT_BYTES = 4096;

    #endregion

    #region Methods

    /// <summary>
    /// Copies <paramref name="image"/> into a new image that is <paramref name="factor"/> times smaller in both directions.
    /// Each pixel of the new image is the average of a block of <paramref name="factor"/> x <paramref name="factor"/> pixels.
    /// </summary>
    /// <remarks>
    /// The new image has the size <see cref="GetDownscaledSize(int, int, int)"/> returns, the same color format as <paramref name="image"/> and no row padding.
    /// Columns on the right and rows at the bottom that don't fill a whole block are ignored. If <paramref name="image"/> is smaller than the factor, a block is its whole width or height.
    /// Each channel, alpha included, is rounded to the nearest value, ties to even.
    /// </remarks>
    /// <param name="image">The image to downscale.</param>
    /// <param name="factor">The width and height of the blocks that are averaged into one pixel.</param>
    /// <returns>The downscaled image.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than 1.</exception>
    public static IImage Downscale(this IImage image, int factor) => image.Downscale(factor, factor);

    /// <summary>
    /// Copies <paramref name="image"/> into a new image that is <paramref name="factorX"/> times narrower and <paramref name="factorY"/> times lower.
    /// Each pixel of the new image is the average of a block of <paramref name="factorX"/> x <paramref name="factorY"/> pixels.
    /// </summary>
    /// <remarks>
    /// The new image has the size <see cref="GetDownscaledSize(int, int, int, int)"/> returns, the same color format as <paramref name="image"/> and no row padding.
    /// Columns on the right and rows at the bottom that don't fill a whole block are ignored. If <paramref name="image"/> is smaller than a factor, a block is its whole width or height.
    /// Each channel, alpha included, is rounded to the nearest value, ties to even.
    /// </remarks>
    /// <param name="image">The image to downscale.</param>
    /// <param name="factorX">The width of the blocks that are averaged into one pixel.</param>
    /// <param name="factorY">The height of the blocks that are averaged into one pixel.</param>
    /// <returns>The downscaled image.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factorX"/> or <paramref name="factorY"/> is less than 1.</exception>
    public static IImage Downscale(this IImage image, int factorX, int factorY)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<DownscaleOperation, IImage>(new DownscaleOperation(factorX, factorY));
    }

    /// <summary>
    /// Writes <paramref name="image"/>, downscaled by <paramref name="factor"/> in both directions, into a buffer whose rows are <paramref name="stride"/> bytes apart.
    /// </summary>
    /// <remarks>
    /// The result has the size <see cref="GetDownscaledSize(int, int, int)"/> returns.
    /// </remarks>
    /// <param name="image">The image to downscale.</param>
    /// <param name="factor">The width and height of the blocks that are averaged into one pixel.</param>
    /// <param name="destination">The buffer to write to. It can't overlap the memory of <paramref name="image"/>. The bytes between rows are left as they are.</param>
    /// <param name="stride">The distance between the starts of two rows in <paramref name="destination"/>, in bytes. The last row needs no padding.</param>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than 1.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="stride"/> is smaller than a row of the result, <paramref name="destination"/> is too small, or <paramref name="destination"/> overlaps the memory of <paramref name="image"/>.
    /// </exception>
    public static void Downscale(this IImage image, int factor, Span<byte> destination, int stride) => image.Downscale(factor, factor, destination, stride);

    /// <summary>
    /// Writes <paramref name="image"/>, downscaled by <paramref name="factorX"/> and <paramref name="factorY"/>, into a buffer whose rows are <paramref name="stride"/> bytes apart.
    /// </summary>
    /// <remarks>
    /// The result has the size <see cref="GetDownscaledSize(int, int, int, int)"/> returns.
    /// </remarks>
    /// <param name="image">The image to downscale.</param>
    /// <param name="factorX">The width of the blocks that are averaged into one pixel.</param>
    /// <param name="factorY">The height of the blocks that are averaged into one pixel.</param>
    /// <param name="destination">The buffer to write to. It can't overlap the memory of <paramref name="image"/>. The bytes between rows are left as they are.</param>
    /// <param name="stride">The distance between the starts of two rows in <paramref name="destination"/>, in bytes. The last row needs no padding.</param>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factorX"/> or <paramref name="factorY"/> is less than 1.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="stride"/> is smaller than a row of the result, <paramref name="destination"/> is too small, or <paramref name="destination"/> overlaps the memory of <paramref name="image"/>.
    /// </exception>
    public static void Downscale(this IImage image, int factorX, int factorY, Span<byte> destination, int stride)
    {
        ArgumentNullException.ThrowIfNull(image);

        image.Execute<DownscaleIntoOperation, bool>(new DownscaleIntoOperation(factorX, factorY, destination, stride));
    }

    /// <inheritdoc cref="Downscale(IImage, int)"/>
    public static Image<T> Downscale<T>(this Image<T> image, int factor)
        where T : unmanaged, IColor<T>
        => image.Downscale(factor, factor);

    /// <inheritdoc cref="Downscale(IImage, int, int)"/>
    public static Image<T> Downscale<T>(this Image<T> image, int factorX, int factorY)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().Downscale(factorX, factorY);
    }

    /// <summary>
    /// Writes <paramref name="image"/>, downscaled by <paramref name="factor"/> in both directions, into <paramref name="destination"/>, row after row without padding.
    /// </summary>
    /// <remarks>
    /// The result has the size <see cref="GetDownscaledSize(int, int, int)"/> returns.
    /// </remarks>
    /// <param name="image">The image to downscale.</param>
    /// <param name="factor">The width and height of the blocks that are averaged into one pixel.</param>
    /// <param name="destination">The buffer to write to. Needs at least as many elements as the result has pixels and can't overlap the memory of <paramref name="image"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factor"/> is less than 1.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is too small or overlaps the memory of <paramref name="image"/>.</exception>
    public static void Downscale<T>(this Image<T> image, int factor, Span<T> destination)
        where T : unmanaged, IColor<T>
        => image.Downscale(factor, factor, destination);

    /// <summary>
    /// Writes <paramref name="image"/>, downscaled by <paramref name="factorX"/> and <paramref name="factorY"/>, into <paramref name="destination"/>, row after row without padding.
    /// </summary>
    /// <remarks>
    /// The result has the size <see cref="GetDownscaledSize(int, int, int, int)"/> returns.
    /// </remarks>
    /// <param name="image">The image to downscale.</param>
    /// <param name="factorX">The width of the blocks that are averaged into one pixel.</param>
    /// <param name="factorY">The height of the blocks that are averaged into one pixel.</param>
    /// <param name="destination">The buffer to write to. Needs at least as many elements as the result has pixels and can't overlap the memory of <paramref name="image"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="factorX"/> or <paramref name="factorY"/> is less than 1.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is too small or overlaps the memory of <paramref name="image"/>.</exception>
    public static void Downscale<T>(this Image<T> image, int factorX, int factorY, Span<T> destination)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        image.AsRefImage().Downscale(factorX, factorY, destination);
    }

    /// <inheritdoc cref="Downscale(IImage, int, Span{byte}, int)"/>
    public static void Downscale<T>(this Image<T> image, int factor, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
        => image.Downscale(factor, factor, destination, stride);

    /// <inheritdoc cref="Downscale(IImage, int, int, Span{byte}, int)"/>
    public static void Downscale<T>(this Image<T> image, int factorX, int factorY, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        image.AsRefImage().Downscale(factorX, factorY, destination, stride);
    }

    /// <inheritdoc cref="Downscale(IImage, int)"/>
    public static Image<T> Downscale<T>(this RefImage<T> image, int factor)
        where T : unmanaged, IColor<T>
        => image.Downscale(factor, factor);

    /// <inheritdoc cref="Downscale(IImage, int, int)"/>
    public static Image<T> Downscale<T>(this RefImage<T> image, int factorX, int factorY)
        where T : unmanaged, IColor<T>
    {
        (int width, int height) = GetDownscaledSize(image.Width, image.Height, factorX, factorY);

        int rowSize = width * T.ColorFormat.BytesPerPixel;
        byte[] buffer = GC.AllocateUninitializedArray<byte>(rowSize * height);
        Downscale(image, factorX, factorY, width, height, buffer, rowSize);

        return Image<T>.Wrap(buffer, width, height, rowSize);
    }

    /// <inheritdoc cref="Downscale{T}(Image{T}, int, Span{T})"/>
    public static void Downscale<T>(this RefImage<T> image, int factor, Span<T> destination)
        where T : unmanaged, IColor<T>
        => image.Downscale(factor, factor, destination);

    /// <inheritdoc cref="Downscale{T}(Image{T}, int, int, Span{T})"/>
    public static void Downscale<T>(this RefImage<T> image, int factorX, int factorY, Span<T> destination)
        where T : unmanaged, IColor<T>
    {
        (int width, int height) = GetDownscaledSize(image.Width, image.Height, factorX, factorY);
        Span<byte> target = ImageValidation.ValidateDestination(image, destination, width, height);

        Downscale(image, factorX, factorY, width, height, target, width * T.ColorFormat.BytesPerPixel);
    }

    /// <inheritdoc cref="Downscale(IImage, int, Span{byte}, int)"/>
    public static void Downscale<T>(this RefImage<T> image, int factor, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
        => image.Downscale(factor, factor, destination, stride);

    /// <inheritdoc cref="Downscale(IImage, int, int, Span{byte}, int)"/>
    public static void Downscale<T>(this RefImage<T> image, int factorX, int factorY, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        (int width, int height) = GetDownscaledSize(image.Width, image.Height, factorX, factorY);
        ImageValidation.ValidateDestination(image, destination, stride, width, height);

        Downscale(image, factorX, factorY, width, height, destination, stride);
    }

    /// <summary>
    /// Gets the size of an image of <paramref name="width"/> x <paramref name="height"/> pixels downscaled by <paramref name="factor"/> in both directions.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="factor">The width and height of the blocks that are averaged into one pixel.</param>
    /// <returns><c>size / factor</c> in each direction, but at least 1, and 0 for a size of 0.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is negative, or <paramref name="factor"/> is less than 1.</exception>
    public static (int Width, int Height) GetDownscaledSize(int width, int height, int factor) => GetDownscaledSize(width, height, factor, factor);

    /// <summary>
    /// Gets the size of an image of <paramref name="width"/> x <paramref name="height"/> pixels downscaled by <paramref name="factorX"/> and <paramref name="factorY"/>.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="factorX">The width of the blocks that are averaged into one pixel.</param>
    /// <param name="factorY">The height of the blocks that are averaged into one pixel.</param>
    /// <returns><c>size / factor</c> in each direction, but at least 1, and 0 for a size of 0.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is negative, or <paramref name="factorX"/> or <paramref name="factorY"/> is less than 1.</exception>
    public static (int Width, int Height) GetDownscaledSize(int width, int height, int factorX, int factorY)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        ArgumentOutOfRangeException.ThrowIfLessThan(factorX, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(factorY, 1);

        return (width == 0 ? 0 : Math.Max(1, width / factorX),
                height == 0 ? 0 : Math.Max(1, height / factorY));
    }

    private static void Downscale<T>(RefImage<T> image, int factorX, int factorY, int width, int height, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        if ((width == 0) || (height == 0)) return;

        int blockWidth = Math.Min(factorX, image.Width);
        int blockHeight = Math.Min(factorY, image.Height);
        int blockBytes = blockWidth * T.ColorFormat.BytesPerPixel;
        bool vectorized = Avx2.IsSupported && (width >= 8);

        if ((blockWidth == 1) && (blockHeight == 1))
            image.CopyTo(destination, stride);
        else if (vectorized && (blockWidth is >= 2 and <= 4) && (blockHeight <= DOWNSCALE_BAND_HEIGHT) && ((blockWidth > 2) || (width >= 16)))
            DownscaleSmallBlocks(image, blockWidth, blockHeight, width, height, destination, stride);
        else if (vectorized && (blockBytes >= 16) && (blockWidth <= DOWNSCALE_MAX_WIDE_BLOCK_WIDTH) && (((long)blockWidth * blockHeight) <= DOWNSCALE_MAX_WIDE_BLOCK_COUNT))
            DownscaleWideBlocks(image, blockWidth, blockHeight, width, height, destination, stride);
        else if ((blockHeight <= DOWNSCALE_MAX_USHORT_COUNT) && (blockBytes <= DOWNSCALE_SEGMENT_BYTES))
            DownscaleRows(image, blockWidth, blockHeight, width, height, destination, stride);
        else
            DownscaleLargeBlocks(image, blockWidth, blockHeight, width, height, destination, stride);
    }

    // Blocks of 2 to 4 pixels in width are added up within a row with a shuffle and pmaddubsw, 16 or 8 output pixels at a time.
    // The last output pixels of a row overlap the ones before, so rows need to be at least 16 or 8 output pixels wide.
    private static void DownscaleSmallBlocks<T>(RefImage<T> image, int blockWidth, int blockHeight, int width, int height, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        int bpp = T.ColorFormat.BytesPerPixel;
        int count = blockWidth * blockHeight;
        Vector256<byte> shuffle = blockWidth == 2 ? GetPairShuffle(bpp) : GetShuffle(bpp, 0, blockWidth, 16 - (blockWidth * bpp), blockWidth);

        ref byte source = ref Unsafe.AsRef(in image.GetPinnableReference());
        ref byte target = ref MemoryMarshal.GetReference(destination);
        nint sourceStride = image.Stride;

        for (int y = 0; y < height; y++)
        {
            ref byte sourceRow = ref Unsafe.Add(ref source, (nint)y * blockHeight * sourceStride);
            ref byte targetRow = ref Unsafe.Add(ref target, (nint)y * stride);

            if (blockWidth == 2)
                DownscaleRowOfPairs<T>(ref sourceRow, sourceStride, blockHeight, width, shuffle, count, ref targetRow);
            else
                DownscaleRowOfSmallBlocks<T>(ref sourceRow, sourceStride, blockHeight, blockWidth, width, shuffle, count, ref targetRow);
        }
    }

    // Each 128 bit lane holds 4 source pixels: 2 output pixels with 4 shorts each.
    // 3 byte pixels are loaded as 16 bytes from offset 0 and 8, so the second lane starts 4 bytes into its pixels.
    private static Vector256<byte> GetPairShuffle(int bpp)
        => bpp == 4
               ? Vector256.Create((byte)0, 4, 1, 5, 2, 6, 3, 7, 8, 12, 9, 13, 10, 14, 11, 15,
                                  0, 4, 1, 5, 2, 6, 3, 7, 8, 12, 9, 13, 10, 14, 11, 15)
               : Vector256.Create(0, 3, 1, 4, 2, 5, 0x80, 0x80, 6, 9, 7, 10, 8, 11, 0x80, 0x80,
                                  4, 7, 5, 8, 6, 9, 0x80, 0x80, 10, 13, 11, 14, 12, 15, 0x80, 0x80);

    // Groups the bytes of each channel of up to 4 pixels per 128 bit lane into 4 bytes, padded with zeros.
    // The pixels of the first lane start offset0 bytes into it, the ones of the second lane offset1 bytes.
    private static Vector256<byte> GetShuffle(int bpp, int offset0, int pixels0, int offset1, int pixels1)
    {
        Span<byte> mask = stackalloc byte[32];
        mask.Fill(0x80);

        for (int channel = 0; channel < bpp; channel++)
        {
            for (int i = 0; i < pixels0; i++)
                mask[(channel * 4) + i] = (byte)(offset0 + (i * bpp) + channel);

            for (int i = 0; i < pixels1; i++)
                mask[16 + (channel * 4) + i] = (byte)(offset1 + (i * bpp) + channel);
        }

        return Vector256.Create(mask);
    }

    private static void DownscaleRowOfPairs<T>(ref byte source, nint stride, int rows, int width, Vector256<byte> shuffle, int count, ref byte target)
        where T : unmanaged, IColor<T>
    {
        int bpp = T.ColorFormat.BytesPerPixel;
        int shift = BitOperations.IsPow2(count) ? BitOperations.Log2((uint)count) : 0;
        Vector256<ushort> half = Vector256.Create((ushort)(shift > 0 ? (1 << (shift - 1)) - 1 : 0));
        bool odd = (count & 1) != 0;
        Vector256<float> divisor = Vector256.Create(odd ? 1f / count : count);

        int last = width - 16;
        for (int x = 0; ; x = Math.Min(x + 16, last))
        {
            ref byte line = ref Unsafe.Add(ref source, x * 2 * bpp);

            Vector256<ushort> sum0 = Vector256<ushort>.Zero;
            Vector256<ushort> sum1 = Vector256<ushort>.Zero;
            Vector256<ushort> sum2 = Vector256<ushort>.Zero;
            Vector256<ushort> sum3 = Vector256<ushort>.Zero;
            for (int row = 0; row < rows; row++)
            {
                sum0 += AddPairs(Load8Pixels<T>(ref line), shuffle);
                sum1 += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref line, 8 * bpp)), shuffle);
                sum2 += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref line, 16 * bpp)), shuffle);
                sum3 += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref line, 24 * bpp)), shuffle);
                line = ref Unsafe.Add(ref line, stride);
            }

            if (shift > 0)
            {
                sum0 = DivideByShift(sum0, shift, half);
                sum1 = DivideByShift(sum1, shift, half);
                sum2 = DivideByShift(sum2, shift, half);
                sum3 = DivideByShift(sum3, shift, half);
            }
            else
            {
                sum0 = Divide(sum0, divisor, odd);
                sum1 = Divide(sum1, divisor, odd);
                sum2 = Divide(sum2, divisor, odd);
                sum3 = Divide(sum3, divisor, odd);
            }

            // (0, 1, 4, 5 | 2, 3, 6, 7) sorted into (0, 1, 2, 3 | 4, 5, 6, 7).
            ref byte pixels = ref Unsafe.Add(ref target, x * bpp);
            Store8Pixels<T>(Avx2.Permute4x64(Avx2.PackUnsignedSaturate(sum0.AsInt16(), sum1.AsInt16()).AsUInt64(), 0b11_01_10_00).AsByte(), ref pixels);
            Store8Pixels<T>(Avx2.Permute4x64(Avx2.PackUnsignedSaturate(sum2.AsInt16(), sum3.AsInt16()).AsUInt64(), 0b11_01_10_00).AsByte(), ref Unsafe.Add(ref pixels, 8 * bpp));

            if (x == last) return;
        }
    }

    // Each 128 bit lane holds one block. The second lane is loaded from the end of the 2 blocks.
    private static void DownscaleRowOfSmallBlocks<T>(ref byte source, nint stride, int rows, int blockWidth, int width, Vector256<byte> shuffle, int count, ref byte target)
        where T : unmanaged, IColor<T>
    {
        int bpp = T.ColorFormat.BytesPerPixel;
        int shift = BitOperations.IsPow2(count) ? BitOperations.Log2((uint)count) : 0;
        Vector256<uint> half = Vector256.Create((uint)(shift > 0 ? (1 << (shift - 1)) - 1 : 0));
        bool odd = (count & 1) != 0;
        Vector256<float> divisor = Vector256.Create(odd ? 1f / count : count);
        int pairBytes = 2 * blockWidth * bpp;
        nuint secondLane = (nuint)pairBytes - 16;

        int last = width - 8;
        for (int x = 0; ; x = Math.Min(x + 8, last))
        {
            ref byte line = ref Unsafe.Add(ref source, x * blockWidth * bpp);

            Vector256<ushort> sum0 = Vector256<ushort>.Zero;
            Vector256<ushort> sum1 = Vector256<ushort>.Zero;
            Vector256<ushort> sum2 = Vector256<ushort>.Zero;
            Vector256<ushort> sum3 = Vector256<ushort>.Zero;
            for (int row = 0; row < rows; row++)
            {
                sum0 += AddPairs(LoadLanes(ref line, 0, secondLane), shuffle);
                sum1 += AddPairs(LoadLanes(ref Unsafe.Add(ref line, pairBytes), 0, secondLane), shuffle);
                sum2 += AddPairs(LoadLanes(ref Unsafe.Add(ref line, 2 * pairBytes), 0, secondLane), shuffle);
                sum3 += AddPairs(LoadLanes(ref Unsafe.Add(ref line, 3 * pairBytes), 0, secondLane), shuffle);
                line = ref Unsafe.Add(ref line, stride);
            }

            StoreQuotients<T>(Quotients(AddPartialSums(sum0), shift, half, divisor, odd),
                              Quotients(AddPartialSums(sum1), shift, half, divisor, odd),
                              Quotients(AddPartialSums(sum2), shift, half, divisor, odd),
                              Quotients(AddPartialSums(sum3), shift, half, divisor, odd),
                              ref Unsafe.Add(ref target, x * bpp));

            if (x == last) return;
        }
    }

    // Blocks of at least 16 bytes: each block is added up in vectors of 8 pixels, the bytes of each channel grouped like in DownscaleSmallBlocks.
    // The up to 7 pixels left at the end of a block are loaded with lanes that end at the block, so no byte outside of it is read.
    // Output rows are processed in segments, and the rows of the blocks in bands, so only a few rows are read at the same time.
    private static void DownscaleWideBlocks<T>(RefImage<T> image, int blockWidth, int blockHeight, int width, int height, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        int bpp = T.ColorFormat.BytesPerPixel;
        int blockBytes = blockWidth * bpp;
        int count = blockWidth * blockHeight;

        int fullVectors = blockWidth / 8;
        int rest = blockWidth % 8;
        int restPixels1 = Math.Min(rest, 4);
        int restStart1 = (blockWidth - restPixels1) * bpp;
        int restLane0 = Math.Max(0, restStart1 - 16);
        int restLane1 = blockBytes - 16;
        Vector256<byte> fullShuffle = GetShuffle(bpp, 0, 4, bpp == 4 ? 0 : 4, 4);
        Vector256<byte> restShuffle = GetShuffle(bpp, (fullVectors * 8 * bpp) - restLane0, rest - restPixels1, restStart1 - restLane1, restPixels1);
        int bandHeight = Math.Min(DOWNSCALE_BAND_HEIGHT, DOWNSCALE_MAX_SHORT_SUMS / (fullVectors + (rest > 0 ? 1 : 0)));

        int shift = BitOperations.IsPow2(count) ? BitOperations.Log2((uint)count) : 0;
        bool odd = (count & 1) != 0;
        bool vectorDivision = (shift > 0) || (odd ? count < 16384 : count < 65536);
        Vector256<uint> half = Vector256.Create((uint)(shift > 0 ? (1 << (shift - 1)) - 1 : 0));
        Vector256<float> divisor = Vector256.Create(odd ? 1f / count : count);

        Span<Vector256<uint>> sums = stackalloc Vector256<uint>[DOWNSCALE_WIDE_SEGMENT / 2];
        ref byte source = ref Unsafe.AsRef(in image.GetPinnableReference());
        ref byte target = ref MemoryMarshal.GetReference(destination);
        nint sourceStride = image.Stride;

        for (int y = 0; y < height; y++)
        {
            ref byte sourceRow = ref Unsafe.Add(ref source, (nint)y * blockHeight * sourceStride);
            ref byte targetRow = ref Unsafe.Add(ref target, (nint)y * stride);

            for (int segment = 0; segment < width; segment += DOWNSCALE_WIDE_SEGMENT)
            {
                int start = Math.Min(segment, width - 8);
                int pixels = Math.Min(DOWNSCALE_WIDE_SEGMENT, width - start);
                int chunks = (pixels + 7) / 8;

                for (int band = 0; band < blockHeight; band += bandHeight)
                {
                    ref byte blocks = ref Unsafe.Add(ref sourceRow, ((nint)band * sourceStride) + ((nint)start * blockBytes));
                    int rows = Math.Min(bandHeight, blockHeight - band);

                    for (int chunk = 0; chunk < chunks; chunk++)
                        SumWideBlocks<T>(ref Unsafe.Add(ref blocks, Math.Min(chunk * 8, pixels - 8) * blockBytes), sourceStride, rows, blockBytes,
                                         fullVectors, fullShuffle, rest > 0, (nuint)restLane0, (nuint)restLane1, restShuffle, sums.Slice(chunk * 4, 4), band == 0);
                }

                for (int chunk = 0; chunk < chunks; chunk++)
                {
                    Span<Vector256<uint>> chunkSums = sums.Slice(chunk * 4, 4);
                    ref byte chunkTarget = ref Unsafe.Add(ref targetRow, (start + Math.Min(chunk * 8, pixels - 8)) * bpp);

                    if (vectorDivision)
                        StoreQuotients<T>(Quotients(chunkSums[0], shift, half, divisor, odd), Quotients(chunkSums[1], shift, half, divisor, odd),
                                          Quotients(chunkSums[2], shift, half, divisor, odd), Quotients(chunkSums[3], shift, half, divisor, odd), ref chunkTarget);
                    else
                    {
                        StoreRoundedAverages(chunkSums[0], count, bpp, ref chunkTarget);
                        StoreRoundedAverages(chunkSums[1], count, bpp, ref Unsafe.Add(ref chunkTarget, 2 * bpp));
                        StoreRoundedAverages(chunkSums[2], count, bpp, ref Unsafe.Add(ref chunkTarget, 4 * bpp));
                        StoreRoundedAverages(chunkSums[3], count, bpp, ref Unsafe.Add(ref chunkTarget, 6 * bpp));
                    }
                }
            }
        }
    }

    // Adds up the rows of 8 neighbouring blocks into (0 | 1), (2 | 3), (4 | 5) and (6 | 7).
    private static void SumWideBlocks<T>(ref byte blocks, nint stride, int rows, int blockBytes,
                                         int fullVectors, Vector256<byte> fullShuffle, bool hasRest, nuint restLane0, nuint restLane1, Vector256<byte> restShuffle,
                                         Span<Vector256<uint>> sums, bool first)
        where T : unmanaged, IColor<T>
    {
        nint block1 = blockBytes;
        nint block2 = 2 * block1;
        nint block3 = 3 * block1;
        nint block4 = 4 * block1;
        nint block5 = 5 * block1;
        nint block6 = 6 * block1;
        nint block7 = 7 * block1;

        Vector256<ushort> sum0 = Vector256<ushort>.Zero;
        Vector256<ushort> sum1 = Vector256<ushort>.Zero;
        Vector256<ushort> sum2 = Vector256<ushort>.Zero;
        Vector256<ushort> sum3 = Vector256<ushort>.Zero;
        Vector256<ushort> sum4 = Vector256<ushort>.Zero;
        Vector256<ushort> sum5 = Vector256<ushort>.Zero;
        Vector256<ushort> sum6 = Vector256<ushort>.Zero;
        Vector256<ushort> sum7 = Vector256<ushort>.Zero;
        for (int row = 0; row < rows; row++)
        {
            if (fullVectors > 1)
            {
                sum0 += SumBlockRow<T>(ref blocks, fullVectors, fullShuffle);
                sum1 += SumBlockRow<T>(ref Unsafe.Add(ref blocks, block1), fullVectors, fullShuffle);
                sum2 += SumBlockRow<T>(ref Unsafe.Add(ref blocks, block2), fullVectors, fullShuffle);
                sum3 += SumBlockRow<T>(ref Unsafe.Add(ref blocks, block3), fullVectors, fullShuffle);
                sum4 += SumBlockRow<T>(ref Unsafe.Add(ref blocks, block4), fullVectors, fullShuffle);
                sum5 += SumBlockRow<T>(ref Unsafe.Add(ref blocks, block5), fullVectors, fullShuffle);
                sum6 += SumBlockRow<T>(ref Unsafe.Add(ref blocks, block6), fullVectors, fullShuffle);
                sum7 += SumBlockRow<T>(ref Unsafe.Add(ref blocks, block7), fullVectors, fullShuffle);
            }
            else if (fullVectors == 1)
            {
                sum0 += AddPairs(Load8Pixels<T>(ref blocks), fullShuffle);
                sum1 += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref blocks, block1)), fullShuffle);
                sum2 += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref blocks, block2)), fullShuffle);
                sum3 += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref blocks, block3)), fullShuffle);
                sum4 += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref blocks, block4)), fullShuffle);
                sum5 += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref blocks, block5)), fullShuffle);
                sum6 += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref blocks, block6)), fullShuffle);
                sum7 += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref blocks, block7)), fullShuffle);
            }

            if (hasRest)
            {
                sum0 += AddPairs(LoadLanes(ref blocks, restLane0, restLane1), restShuffle);
                sum1 += AddPairs(LoadLanes(ref Unsafe.Add(ref blocks, block1), restLane0, restLane1), restShuffle);
                sum2 += AddPairs(LoadLanes(ref Unsafe.Add(ref blocks, block2), restLane0, restLane1), restShuffle);
                sum3 += AddPairs(LoadLanes(ref Unsafe.Add(ref blocks, block3), restLane0, restLane1), restShuffle);
                sum4 += AddPairs(LoadLanes(ref Unsafe.Add(ref blocks, block4), restLane0, restLane1), restShuffle);
                sum5 += AddPairs(LoadLanes(ref Unsafe.Add(ref blocks, block5), restLane0, restLane1), restShuffle);
                sum6 += AddPairs(LoadLanes(ref Unsafe.Add(ref blocks, block6), restLane0, restLane1), restShuffle);
                sum7 += AddPairs(LoadLanes(ref Unsafe.Add(ref blocks, block7), restLane0, restLane1), restShuffle);
            }

            blocks = ref Unsafe.Add(ref blocks, stride);
        }

        Vector256<uint> pixels01 = AddLanes(AddPartialSums(sum0), AddPartialSums(sum1));
        Vector256<uint> pixels23 = AddLanes(AddPartialSums(sum2), AddPartialSums(sum3));
        Vector256<uint> pixels45 = AddLanes(AddPartialSums(sum4), AddPartialSums(sum5));
        Vector256<uint> pixels67 = AddLanes(AddPartialSums(sum6), AddPartialSums(sum7));

        if (first)
        {
            sums[0] = pixels01;
            sums[1] = pixels23;
            sums[2] = pixels45;
            sums[3] = pixels67;
        }
        else
        {
            sums[0] += pixels01;
            sums[1] += pixels23;
            sums[2] += pixels45;
            sums[3] += pixels67;
        }
    }

    // The vectors of a block are read one after the other: reading the 8 blocks vector by vector is a lot slower for wide blocks.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ushort> SumBlockRow<T>(ref byte block, int fullVectors, Vector256<byte> fullShuffle)
        where T : unmanaged, IColor<T>
    {
        int vectorBytes = 8 * T.ColorFormat.BytesPerPixel;

        Vector256<ushort> sum = AddPairs(Load8Pixels<T>(ref block), fullShuffle);
        for (int i = 1; i < fullVectors; i++)
            sum += AddPairs(Load8Pixels<T>(ref Unsafe.Add(ref block, i * vectorBytes)), fullShuffle);

        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ushort> AddPairs(Vector256<byte> bytes, Vector256<byte> shuffle)
        => Avx2.MultiplyAddAdjacent(Avx2.Shuffle(bytes, shuffle), Vector256<sbyte>.One).AsUInt16();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> AddPartialSums(Vector256<ushort> sums)
        => (sums.AsUInt32() & Vector256.Create(0xFFFFu)) + (sums.AsUInt32() >>> 16);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> AddLanes(Vector256<uint> first, Vector256<uint> second)
        => Avx2.Permute2x128(first, second, 0x20) + Avx2.Permute2x128(first, second, 0x31);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<byte> Load8Pixels<T>(ref byte source)
        where T : unmanaged, IColor<T>
        => T.ColorFormat.BytesPerPixel == 4 ? Vector256.LoadUnsafe(ref source) : Vector256.Create(Vector128.LoadUnsafe(ref source), Vector128.LoadUnsafe(ref source, 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<byte> LoadLanes(ref byte source, nuint first, nuint second)
        => Vector256.Create(Vector128.LoadUnsafe(ref source, first), Vector128.LoadUnsafe(ref source, second));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> Quotients(Vector256<uint> sums, int shift, Vector256<uint> half, Vector256<float> divisor, bool odd)
        => shift > 0 ? DivideByShift(sums, shift, half) : Divide(sums, divisor, odd);

    // Each vector holds the 4 channels of 2 pixels: (0 | 1), (2 | 3), ... After packing they are in the order (0, 2, 4, 6 | 1, 3, 5, 7).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StoreQuotients<T>(Vector256<uint> pixels01, Vector256<uint> pixels23, Vector256<uint> pixels45, Vector256<uint> pixels67, ref byte target)
        where T : unmanaged, IColor<T>
    {
        Vector256<byte> packed = Avx2.PackUnsignedSaturate(Avx2.PackUnsignedSaturate(pixels01.AsInt32(), pixels23.AsInt32()).AsInt16(),
                                                           Avx2.PackUnsignedSaturate(pixels45.AsInt32(), pixels67.AsInt32()).AsInt16());
        Store8Pixels<T>(Avx2.PermuteVar8x32(packed.AsUInt32(), Vector256.Create(0u, 4, 1, 5, 2, 6, 3, 7)).AsByte(), ref target);
    }

    private static void StoreRoundedAverages(Vector256<uint> pixels, int count, int bpp, ref byte target)
    {
        for (int channel = 0; channel < bpp; channel++)
        {
            Unsafe.Add(ref target, channel) = RoundedAverage(pixels.GetElement(channel), count);
            Unsafe.Add(ref target, bpp + channel) = RoundedAverage(pixels.GetElement(4 + channel), count);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Store8Pixels<T>(Vector256<byte> pixels, ref byte target)
        where T : unmanaged, IColor<T>
    {
        if (T.ColorFormat.BytesPerPixel == 4)
            pixels.StoreUnsafe(ref target);
        else
        {
            Vector256<byte> narrow = Vector256.Create(0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 0x80, 0x80, 0x80, 0x80,
                                                      0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 0x80, 0x80, 0x80, 0x80);
            Vector256<uint> join = Vector256.Create(0u, 1, 2, 4, 5, 6, 3, 7);
            Store3BytesRow(pixels.AsUInt32(), ref target, narrow, join);
        }
    }

    // Rounds to the nearest value, ties to even: the remainder rounds up if it is more than half, or exactly half and the quotient is odd.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> DivideByShift(Vector256<uint> sum, int shift, Vector256<uint> half)
        => (sum + half + ((sum >>> shift) & Vector256<uint>.One)) >>> shift;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ushort> DivideByShift(Vector256<ushort> sum, int shift, Vector256<ushort> half)
        => (sum + half + ((sum >>> shift) & Vector256<ushort>.One)) >>> shift;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ushort> DivideByShift(Vector128<ushort> sum, int shift, Vector128<ushort> half)
        => (sum + half + ((sum >>> shift) & Vector128<ushort>.One)) >>> shift;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> Divide(Vector256<uint> sum, Vector256<float> divisor, bool odd)
        => Quotient(Vector256.ConvertToSingle(sum.AsInt32()), divisor, odd).AsUInt32();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ushort> Divide(Vector256<ushort> sum, Vector256<float> divisor, bool odd)
    {
        (Vector256<uint> lower, Vector256<uint> upper) = Vector256.Widen(sum);
        return Vector256.Narrow(Divide(lower, divisor, odd), Divide(upper, divisor, odd));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ushort> Divide(Vector128<ushort> sum, Vector128<float> divisor, bool odd)
    {
        (Vector128<uint> lower, Vector128<uint> upper) = Vector128.Widen(sum);
        return Vector128.Narrow(Quotient(Vector128.ConvertToSingle(lower.AsInt32()), divisor, odd).AsUInt32(),
                                Quotient(Vector128.ConvertToSingle(upper.AsInt32()), divisor, odd).AsUInt32());
    }

    // For an odd count no sum lies exactly halfway between two quotients and the others are at least 1 / (2 * count) away from it,
    // so multiplying with the reciprocal is exact enough. For an even count the division is exact: sums are below 2^24 and counts below 2^16.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<int> Quotient(Vector256<float> sum, Vector256<float> divisor, bool odd)
        => Vector256.ConvertToInt32Native(Vector256.Round(odd ? sum * divisor : sum / divisor));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> Quotient(Vector128<float> sum, Vector128<float> divisor, bool odd)
        => Vector128.ConvertToInt32Native(Vector128.Round(odd ? sum * divisor : sum / divisor));

    // Without AVX2 and for the cases the kernels above don't cover. Each row of blocks is processed in segments of up to DOWNSCALE_SEGMENT_BYTES source bytes:
    // the rows of the blocks are added up per byte, then the columns of each block, then the sums are divided.
    private static void DownscaleRows<T>(RefImage<T> image, int blockWidth, int blockHeight, int width, int height, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        int bpp = T.ColorFormat.BytesPerPixel;
        int blockBytes = blockWidth * bpp;
        int count = blockWidth * blockHeight;
        int segmentPixels = DOWNSCALE_SEGMENT_BYTES / blockBytes;

        // The padding is for the 8 byte accesses of 3 byte pixels.
        Span<ushort> columnSums = stackalloc ushort[DOWNSCALE_SEGMENT_BYTES + 4];
        Span<ushort> blockSums = stackalloc ushort[(DOWNSCALE_SEGMENT_BYTES / 2) + 4];

        ref byte source = ref Unsafe.AsRef(in image.GetPinnableReference());
        ref byte target = ref MemoryMarshal.GetReference(destination);
        nint sourceStride = image.Stride;

        for (int y = 0; y < height; y++)
        {
            ref byte sourceRow = ref Unsafe.Add(ref source, (nint)y * blockHeight * sourceStride);
            ref byte targetRow = ref Unsafe.Add(ref target, (nint)y * stride);

            for (int x = 0; x < width; x += segmentPixels)
            {
                int pixels = Math.Min(segmentPixels, width - x);
                ref byte targetSegment = ref Unsafe.Add(ref targetRow, x * bpp);

                for (int band = 0; band < blockHeight; band += DOWNSCALE_BAND_HEIGHT)
                    SumColumns(ref Unsafe.Add(ref sourceRow, ((nint)band * sourceStride) + (x * blockBytes)), sourceStride,
                               Math.Min(DOWNSCALE_BAND_HEIGHT, blockHeight - band), pixels * blockBytes, columnSums, band > 0);

                if (count > DOWNSCALE_MAX_USHORT_COUNT)
                    SumAndDivideBlocks(columnSums, bpp, blockWidth, pixels, count, ref targetSegment);
                else if (blockWidth == 1)
                    DivideSums(columnSums, pixels * bpp, count, ref targetSegment);
                else
                {
                    SumBlocks(columnSums, bpp, blockWidth, pixels, blockSums);
                    DivideSums(blockSums, pixels * bpp, count, ref targetSegment);
                }
            }
        }
    }

    private static void SumColumns(ref byte source, nint stride, int rows, int length, Span<ushort> sums, bool add)
    {
        ref ushort sum = ref MemoryMarshal.GetReference(sums);
        int i = 0;

        if (Vector256.IsHardwareAccelerated)
            for (; i <= (length - Vector256<byte>.Count); i += Vector256<byte>.Count)
            {
                ref byte column = ref Unsafe.Add(ref source, i);
                Vector256<ushort> lower = add ? Vector256.LoadUnsafe(ref sum, (nuint)i) : Vector256<ushort>.Zero;
                Vector256<ushort> upper = add ? Vector256.LoadUnsafe(ref sum, (nuint)i + (nuint)Vector256<ushort>.Count) : Vector256<ushort>.Zero;
                for (int row = 0; row < rows; row++)
                {
                    (Vector256<ushort> rowLower, Vector256<ushort> rowUpper) = Vector256.Widen(Vector256.LoadUnsafe(ref Unsafe.Add(ref column, row * stride)));
                    lower += rowLower;
                    upper += rowUpper;
                }

                lower.StoreUnsafe(ref sum, (nuint)i);
                upper.StoreUnsafe(ref sum, (nuint)i + (nuint)Vector256<ushort>.Count);
            }

        if (Vector128.IsHardwareAccelerated)
            for (; i <= (length - Vector128<byte>.Count); i += Vector128<byte>.Count)
            {
                ref byte column = ref Unsafe.Add(ref source, i);
                Vector128<ushort> lower = add ? Vector128.LoadUnsafe(ref sum, (nuint)i) : Vector128<ushort>.Zero;
                Vector128<ushort> upper = add ? Vector128.LoadUnsafe(ref sum, (nuint)i + (nuint)Vector128<ushort>.Count) : Vector128<ushort>.Zero;
                for (int row = 0; row < rows; row++)
                {
                    (Vector128<ushort> rowLower, Vector128<ushort> rowUpper) = Vector128.Widen(Vector128.LoadUnsafe(ref Unsafe.Add(ref column, row * stride)));
                    lower += rowLower;
                    upper += rowUpper;
                }

                lower.StoreUnsafe(ref sum, (nuint)i);
                upper.StoreUnsafe(ref sum, (nuint)i + (nuint)Vector128<ushort>.Count);
            }

        for (; i < length; i++)
        {
            int columnSum = add ? Unsafe.Add(ref sum, i) : 0;
            for (int row = 0; row < rows; row++)
                columnSum += Unsafe.Add(ref source, i + (row * stride));

            Unsafe.Add(ref sum, i) = (ushort)columnSum;
        }
    }

    // All channels of a pixel are added in one ulong. For 3 byte pixels the top 16 bits belong to the next pixel and are ignored.
    private static void SumBlocks(Span<ushort> columnSums, int bpp, int blockWidth, int pixels, Span<ushort> blockSums)
    {
        int pixelBytes = bpp * sizeof(ushort);
        ref byte columns = ref Unsafe.As<ushort, byte>(ref MemoryMarshal.GetReference(columnSums));
        ref byte blocks = ref Unsafe.As<ushort, byte>(ref MemoryMarshal.GetReference(blockSums));

        int pixel = 0;
        if ((bpp == 4) && Vector128.IsHardwareAccelerated)
        {
            if (blockWidth == 2)
                pixel = SumPairsOf4Bytes(ref Unsafe.As<byte, ulong>(ref columns), ref Unsafe.As<byte, ulong>(ref blocks), pixels);
            else if (blockWidth == 4)
                pixel = SumQuadsOf4Bytes(ref Unsafe.As<byte, ulong>(ref columns), ref Unsafe.As<byte, ulong>(ref blocks), pixels);
        }

        for (; pixel < pixels; pixel++)
        {
            ref byte block = ref Unsafe.Add(ref columns, pixel * blockWidth * pixelBytes);

            ulong sum = 0;
            for (int i = 0; i < blockWidth; i++)
                sum += Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref block, i * pixelBytes));

            Unsafe.WriteUnaligned(ref Unsafe.Add(ref blocks, pixel * pixelBytes), sum);
        }
    }

    // Each ulong holds the 4 channel sums of a pixel. The low and high halves of two vectors are joined, so that one add sums up 2 blocks.
    private static int SumPairsOf4Bytes(ref ulong columns, ref ulong blocks, int pixels)
    {
        int pixel = 0;
        for (; pixel <= (pixels - 2); pixel += 2)
        {
            Vector128<ulong> first = Vector128.LoadUnsafe(ref columns, (nuint)pixel * 2);
            Vector128<ulong> second = Vector128.LoadUnsafe(ref columns, ((nuint)pixel * 2) + 2);

            (JoinLower(first, second).AsUInt16() + JoinUpper(first, second).AsUInt16()).AsUInt64().StoreUnsafe(ref blocks, (nuint)pixel);
        }

        return pixel;
    }

    private static int SumQuadsOf4Bytes(ref ulong columns, ref ulong blocks, int pixels)
    {
        int pixel = 0;
        for (; pixel <= (pixels - 2); pixel += 2)
        {
            Vector128<ulong> first = (Vector128.LoadUnsafe(ref columns, (nuint)pixel * 4).AsUInt16() + Vector128.LoadUnsafe(ref columns, ((nuint)pixel * 4) + 2).AsUInt16()).AsUInt64();
            Vector128<ulong> second = (Vector128.LoadUnsafe(ref columns, ((nuint)pixel * 4) + 4).AsUInt16() + Vector128.LoadUnsafe(ref columns, ((nuint)pixel * 4) + 6).AsUInt16()).AsUInt64();

            (JoinLower(first, second).AsUInt16() + JoinUpper(first, second).AsUInt16()).AsUInt64().StoreUnsafe(ref blocks, (nuint)pixel);
        }

        return pixel;
    }

    private static void SumAndDivideBlocks(Span<ushort> columnSums, int bpp, int blockWidth, int pixels, int count, ref byte target)
    {
        int pixelBytes = bpp * sizeof(ushort);
        ref byte columns = ref Unsafe.As<ushort, byte>(ref MemoryMarshal.GetReference(columnSums));

        for (int pixel = 0; pixel < pixels; pixel++)
        {
            ref byte block = ref Unsafe.Add(ref columns, pixel * blockWidth * pixelBytes);

            Vector128<uint> sum = Vector128<uint>.Zero;
            for (int i = 0; i < blockWidth; i++)
                sum += Vector128.WidenLower(Vector128.CreateScalarUnsafe(Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref block, i * pixelBytes))).AsUInt16());

            ref byte result = ref Unsafe.Add(ref target, pixel * bpp);
            for (int channel = 0; channel < bpp; channel++)
                Unsafe.Add(ref result, channel) = RoundedAverage(sum.GetElement(channel), count);
        }
    }

    private static void DivideSums(Span<ushort> sums, int length, int count, ref byte target)
    {
        ref ushort sum = ref MemoryMarshal.GetReference(sums);
        int shift = BitOperations.IsPow2(count) ? BitOperations.Log2((uint)count) : 0;
        ushort halfMinusOne = (ushort)(shift > 0 ? (1 << (shift - 1)) - 1 : 0);
        bool odd = (count & 1) != 0;
        int i = 0;

        if (Vector256.IsHardwareAccelerated && (length >= Vector256<byte>.Count))
        {
            Vector256<ushort> half = Vector256.Create(halfMinusOne);
            Vector256<float> divisor = Vector256.Create(odd ? 1f / count : count);

            int last = length - Vector256<byte>.Count;
            while (true)
            {
                Vector256<ushort> lower = Vector256.LoadUnsafe(ref sum, (nuint)i);
                Vector256<ushort> upper = Vector256.LoadUnsafe(ref sum, (nuint)i + (nuint)Vector256<ushort>.Count);

                Vector256<byte> result = shift > 0
                                             ? Vector256.Narrow(DivideByShift(lower, shift, half), DivideByShift(upper, shift, half))
                                             : Vector256.Narrow(Divide(lower, divisor, odd), Divide(upper, divisor, odd));
                result.StoreUnsafe(ref target, (nuint)i);

                if (i == last) return;
                i = Math.Min(i + Vector256<byte>.Count, last);
            }
        }

        if (Vector128.IsHardwareAccelerated && (length >= Vector128<byte>.Count))
        {
            Vector128<ushort> half = Vector128.Create(halfMinusOne);
            Vector128<float> divisor = Vector128.Create(odd ? 1f / count : count);

            int last = length - Vector128<byte>.Count;
            while (true)
            {
                Vector128<ushort> lower = Vector128.LoadUnsafe(ref sum, (nuint)i);
                Vector128<ushort> upper = Vector128.LoadUnsafe(ref sum, (nuint)i + (nuint)Vector128<ushort>.Count);

                Vector128<byte> result = shift > 0
                                             ? Vector128.Narrow(DivideByShift(lower, shift, half), DivideByShift(upper, shift, half))
                                             : Vector128.Narrow(Divide(lower, divisor, odd), Divide(upper, divisor, odd));
                result.StoreUnsafe(ref target, (nuint)i);

                if (i == last) return;
                i = Math.Min(i + Vector128<byte>.Count, last);
            }
        }

        for (; i < length; i++)
        {
            int value = Unsafe.Add(ref sum, i);
            Unsafe.Add(ref target, i) = shift > 0
                                            ? (byte)((value + halfMinusOne + ((value >> shift) & 1)) >> shift)
                                            : RoundedAverage(value, count);
        }
    }

    // Blocks too high or too wide for the other paths are added up one by one.
    private static void DownscaleLargeBlocks<T>(RefImage<T> image, int blockWidth, int blockHeight, int width, int height, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        int bpp = T.ColorFormat.BytesPerPixel;
        long count = (long)blockWidth * blockHeight;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Generic4LongData sum = SumBytes(image.Slice(x * blockWidth, y * blockHeight, blockWidth, blockHeight));
                Span<byte> pixel = destination.Slice((y * stride) + (x * bpp), bpp);

                pixel[0] = RoundedAverage(sum.L1, count);
                pixel[1] = RoundedAverage(sum.L2, count);
                pixel[2] = RoundedAverage(sum.L3, count);
                if (bpp == 4) pixel[3] = RoundedAverage(sum.L4, count);
            }
    }

    #endregion

    private readonly struct DownscaleOperation(int factorX, int factorY) : IImageOperation<IImage>
    {
        public IImage Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image.Downscale(factorX, factorY);
    }

    private readonly ref struct DownscaleIntoOperation(int factorX, int factorY, Span<byte> destination, int stride) : IImageOperation<bool>
    {
        private readonly Span<byte> _destination = destination;

        public bool Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T>
        {
            image.Downscale(factorX, factorY, _destination, stride);
            return true;
        }
    }
}
