using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace HPPH;

public static partial class PixelHelper
{
    #region Properties & Fields

    // Target byte 3q + c (pixel q, channel c) comes from vector byte 1 + 3 * (20 - q) + c. The last byte is the one left over.
    private static ReadOnlySpan<byte> Reverse3BytesMask512 =>
    [
        61, 62, 63, 58, 59, 60, 55, 56, 57, 52, 53, 54, 49, 50, 51, 46, 47, 48, 43, 44, 45,
        40, 41, 42, 37, 38, 39, 34, 35, 36, 31, 32, 33, 28, 29, 30, 25, 26, 27, 22, 23, 24,
        19, 20, 21, 16, 17, 18, 13, 14, 15, 10, 11, 12, 7, 8, 9, 4, 5, 6, 1, 2, 3, 0
    ];

    #endregion

    #region Methods

    /// <summary>
    /// Copies <paramref name="image"/> into a new image, flipped in the given directions.
    /// </summary>
    /// <remarks>
    /// The new image has the same color format and size as <paramref name="image"/> and no row padding.
    /// Flipping in both directions gives the same result as rotating by 180 degrees.
    /// </remarks>
    /// <param name="image">The image to flip.</param>
    /// <param name="flip">The directions to flip in.</param>
    /// <returns>The flipped image.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    public static IImage Flip(this IImage image, ImageFlip flip)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<FlipOperation, IImage>(new FlipOperation(flip));
    }

    /// <summary>
    /// Writes <paramref name="image"/>, flipped in the given directions, into a buffer whose rows are <paramref name="stride"/> bytes apart.
    /// </summary>
    /// <remarks>
    /// Flipping in both directions gives the same result as rotating by 180 degrees.
    /// </remarks>
    /// <param name="image">The image to flip.</param>
    /// <param name="flip">The directions to flip in.</param>
    /// <param name="destination">The buffer to write to. It can't overlap the memory of <paramref name="image"/>. The bytes between rows are left as they are.</param>
    /// <param name="stride">The distance between the starts of two rows in <paramref name="destination"/>, in bytes. The last row needs no padding.</param>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="stride"/> is smaller than a row, <paramref name="destination"/> is too small, or <paramref name="destination"/> overlaps the memory of <paramref name="image"/>.
    /// </exception>
    public static void Flip(this IImage image, ImageFlip flip, Span<byte> destination, int stride)
    {
        ArgumentNullException.ThrowIfNull(image);

        image.Execute<FlipIntoOperation, bool>(new FlipIntoOperation(flip, destination, stride));
    }

    /// <inheritdoc cref="Flip(IImage, ImageFlip)"/>
    public static Image<T> Flip<T>(this Image<T> image, ImageFlip flip)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().Flip(flip);
    }

    /// <summary>
    /// Writes <paramref name="image"/>, flipped in the given directions, into <paramref name="destination"/>, row after row without padding.
    /// </summary>
    /// <remarks>
    /// Flipping in both directions gives the same result as rotating by 180 degrees.
    /// </remarks>
    /// <param name="image">The image to flip.</param>
    /// <param name="flip">The directions to flip in.</param>
    /// <param name="destination">The buffer to write to. Needs at least <c>Width * Height</c> elements and can't overlap the memory of <paramref name="image"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is too small or overlaps the memory of <paramref name="image"/>.</exception>
    public static void Flip<T>(this Image<T> image, ImageFlip flip, Span<T> destination)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        image.AsRefImage().Flip(flip, destination);
    }

    /// <inheritdoc cref="Flip(IImage, ImageFlip, Span{byte}, int)"/>
    public static void Flip<T>(this Image<T> image, ImageFlip flip, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        image.AsRefImage().Flip(flip, destination, stride);
    }

    /// <inheritdoc cref="Flip(IImage, ImageFlip)"/>
    public static Image<T> Flip<T>(this RefImage<T> image, ImageFlip flip)
        where T : unmanaged, IColor<T>
    {
        int rowSize = image.Width * T.ColorFormat.BytesPerPixel;
        byte[] buffer = GC.AllocateUninitializedArray<byte>(rowSize * image.Height);
        Flip(image, (flip & ImageFlip.Horizontal) != 0, (flip & ImageFlip.Vertical) != 0, buffer, rowSize);

        return Image<T>.Wrap(buffer, image.Width, image.Height, rowSize);
    }

    /// <inheritdoc cref="Flip{T}(Image{T}, ImageFlip, Span{T})"/>
    public static void Flip<T>(this RefImage<T> image, ImageFlip flip, Span<T> destination)
        where T : unmanaged, IColor<T>
    {
        Span<byte> target = ImageValidation.ValidateDestination(image, destination, image.Width, image.Height);

        Flip(image, (flip & ImageFlip.Horizontal) != 0, (flip & ImageFlip.Vertical) != 0, target, image.Width * T.ColorFormat.BytesPerPixel);
    }

    /// <inheritdoc cref="Flip(IImage, ImageFlip, Span{byte}, int)"/>
    public static void Flip<T>(this RefImage<T> image, ImageFlip flip, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        ImageValidation.ValidateDestination(image, destination, stride, image.Width, image.Height);

        Flip(image, (flip & ImageFlip.Horizontal) != 0, (flip & ImageFlip.Vertical) != 0, destination, stride);
    }

    internal static void Flip<T>(RefImage<T> image, bool horizontal, bool vertical, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        int rowSize = image.Width * T.ColorFormat.BytesPerPixel;
        if ((rowSize == 0) || (image.Height == 0)) return;

        if (!horizontal && !vertical)
        {
            image.CopyTo(destination, stride);
            return;
        }

        // A vertical flip walks one of the two images bottom to top. Memory that was just allocated seems to be much slower to write backwards.
        for (int y = 0; y < image.Height; y++)
        {
            ReadOnlySpan<byte> source = image.GetRowBytes(vertical ? image.Height - 1 - y : y);
            Span<byte> target = destination.Slice(y * stride, rowSize);

            if (horizontal)
                CopyReversed<T>(source, target);
            else
                source.CopyTo(target);
        }
    }

    private static void CopyReversed<T>(ReadOnlySpan<byte> source, Span<byte> target)
        where T : unmanaged, IColor<T>
    {
        switch (T.ColorFormat.BytesPerPixel)
        {
            case 3:
                CopyReversed3Bytes(source, target);
                break;

            case 4:
                CopyReversed4Bytes(source, target);
                break;

            default:
                throw new NotSupportedException("Data is not of a supported valid color-type.");
        }
    }

    private static void CopyReversed4Bytes(ReadOnlySpan<byte> source, Span<byte> target)
    {
        ref uint src = ref Unsafe.As<byte, uint>(ref MemoryMarshal.GetReference(source));
        ref uint tar = ref Unsafe.As<byte, uint>(ref MemoryMarshal.GetReference(target));
        nuint pixels = (nuint)source.Length / 4;

        if (Vector512.IsHardwareAccelerated && Avx512F.IsSupported && (pixels >= 16))
        {
            Vector512<uint> mask = Vector512.Create(15u, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);
            nuint last = pixels - 16;
            for (nuint i = 0; i < last; i += 16)
                Avx512F.PermuteVar16x32(Vector512.LoadUnsafe(ref src, last - i), mask).StoreUnsafe(ref tar, i);
            Avx512F.PermuteVar16x32(Vector512.LoadUnsafe(ref src), mask).StoreUnsafe(ref tar, last);
        }
        else if (Vector256.IsHardwareAccelerated && Avx2.IsSupported && (pixels >= 8))
        {
            Vector256<uint> mask = Vector256.Create(7u, 6, 5, 4, 3, 2, 1, 0);
            nuint last = pixels - 8;
            for (nuint i = 0; i < last; i += 8)
                Avx2.PermuteVar8x32(Vector256.LoadUnsafe(ref src, last - i), mask).StoreUnsafe(ref tar, i);
            Avx2.PermuteVar8x32(Vector256.LoadUnsafe(ref src), mask).StoreUnsafe(ref tar, last);
        }
        else if (Vector128.IsHardwareAccelerated && (pixels >= 4))
        {
            Vector128<uint> mask = Vector128.Create(3u, 2, 1, 0);
            nuint last = pixels - 4;
            for (nuint i = 0; i < last; i += 4)
                Vector128.Shuffle(Vector128.LoadUnsafe(ref src, last - i), mask).StoreUnsafe(ref tar, i);
            Vector128.Shuffle(Vector128.LoadUnsafe(ref src), mask).StoreUnsafe(ref tar, last);
        }
        else
        {
            for (nuint i = 0; i < pixels; i++)
                Unsafe.Add(ref tar, i) = Unsafe.Add(ref src, pixels - 1 - i);
        }
    }

    private static void CopyReversed3Bytes(ReadOnlySpan<byte> source, Span<byte> target)
    {
        ref byte src = ref MemoryMarshal.GetReference(source);
        ref byte tar = ref MemoryMarshal.GetReference(target);
        nint pixels = source.Length / 3;
        nint i = 0; 

        if (Vector512.IsHardwareAccelerated && Avx512Vbmi.IsSupported && (pixels > 21))
        {
            Vector512<byte> mask = Vector512.Create(Reverse3BytesMask512);
            for (; i < (pixels - 21); i += 21)
                Avx512Vbmi.PermuteVar64x8(Vector512.LoadUnsafe(ref src, (nuint)((3 * (pixels - 21 - i)) - 1)), mask).StoreUnsafe(ref tar, (nuint)(3 * i));
        }

        if (Vector128.IsHardwareAccelerated && ((pixels - i) > 5))
        {
            Vector128<byte> mask = Vector128.Create((byte)13, 14, 15, 10, 11, 12, 7, 8, 9, 4, 5, 6, 1, 2, 3, 0);
            for (; i < (pixels - 5); i += 5)
                Vector128.ShuffleNative(Vector128.LoadUnsafe(ref src, (nuint)((3 * (pixels - 5 - i)) - 1)), mask).StoreUnsafe(ref tar, (nuint)(3 * i));
        }

        for (; i < pixels; i++)
        {
            nint s = 3 * (pixels - 1 - i);
            nint t = 3 * i;

            Unsafe.Add(ref tar, t) = Unsafe.Add(ref src, s);
            Unsafe.Add(ref tar, t + 1) = Unsafe.Add(ref src, s + 1);
            Unsafe.Add(ref tar, t + 2) = Unsafe.Add(ref src, s + 2);
        }
    }

    #endregion

    private readonly struct FlipOperation(ImageFlip flip) : IImageOperation<IImage>
    {
        public IImage Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image.Flip(flip);
    }

    private readonly ref struct FlipIntoOperation(ImageFlip flip, Span<byte> destination, int stride) : IImageOperation<bool>
    {
        private readonly Span<byte> _destination = destination;

        public bool Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T>
        {
            image.Flip(flip, _destination, stride);
            return true;
        }
    }
}
