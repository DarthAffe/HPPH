using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace HPPH;

public static partial class PixelHelper
{
    #region Constants

    private const int ROTATE_BAND_HEIGHT = 1024;
    private const int ROTATE_PREFETCH_BLOCKS = 16;
    private const int SCALAR_ROTATE_TILE_SIZE = 16;

    #endregion

    #region Methods

    /// <summary>
    /// Copies <paramref name="image"/> into a new image, rotated clockwise.
    /// </summary>
    /// <remarks>
    /// The new image has the same color format as <paramref name="image"/> and no row padding.
    /// For <see cref="ImageRotation.Rotate90"/> and <see cref="ImageRotation.Rotate270"/> its width is the height of <paramref name="image"/> and the other way around.
    /// </remarks>
    /// <param name="image">The image to rotate.</param>
    /// <param name="rotation">The rotation.</param>
    /// <returns>The rotated image.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rotation"/> is not one of the values of <see cref="ImageRotation"/>.</exception>
    public static IImage Rotate(this IImage image, ImageRotation rotation)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Execute<RotateOperation, IImage>(new RotateOperation(rotation));
    }

    /// <summary>
    /// Writes <paramref name="image"/>, rotated clockwise, into a buffer whose rows are <paramref name="stride"/> bytes apart.
    /// </summary>
    /// <remarks>
    /// For <see cref="ImageRotation.Rotate90"/> and <see cref="ImageRotation.Rotate270"/> the result is <c>Height</c> pixels wide and <c>Width</c> pixels high.
    /// </remarks>
    /// <param name="image">The image to rotate.</param>
    /// <param name="rotation">The rotation.</param>
    /// <param name="destination">The buffer to write to. It can't overlap the memory of <paramref name="image"/>. The bytes between rows are left as they are.</param>
    /// <param name="stride">The distance between the starts of two rows in <paramref name="destination"/>, in bytes. The last row needs no padding.</param>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rotation"/> is not one of the values of <see cref="ImageRotation"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="stride"/> is smaller than a row of the result, <paramref name="destination"/> is too small, or <paramref name="destination"/> overlaps the memory of <paramref name="image"/>.
    /// </exception>
    public static void Rotate(this IImage image, ImageRotation rotation, Span<byte> destination, int stride)
    {
        ArgumentNullException.ThrowIfNull(image);

        image.Execute<RotateIntoOperation, bool>(new RotateIntoOperation(rotation, destination, stride));
    }

    /// <inheritdoc cref="Rotate(IImage, ImageRotation)"/>
    public static Image<T> Rotate<T>(this Image<T> image, ImageRotation rotation)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.AsRefImage().Rotate(rotation);
    }

    /// <summary>
    /// Writes <paramref name="image"/>, rotated clockwise, into <paramref name="destination"/>, row after row without padding.
    /// </summary>
    /// <remarks>
    /// For <see cref="ImageRotation.Rotate90"/> and <see cref="ImageRotation.Rotate270"/> the result is <c>Height</c> pixels wide and <c>Width</c> pixels high.
    /// </remarks>
    /// <param name="image">The image to rotate.</param>
    /// <param name="rotation">The rotation.</param>
    /// <param name="destination">The buffer to write to. Needs at least <c>Width * Height</c> elements and can't overlap the memory of <paramref name="image"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rotation"/> is not one of the values of <see cref="ImageRotation"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is too small or overlaps the memory of <paramref name="image"/>.</exception>
    public static void Rotate<T>(this Image<T> image, ImageRotation rotation, Span<T> destination)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        image.AsRefImage().Rotate(rotation, destination);
    }

    /// <inheritdoc cref="Rotate(IImage, ImageRotation, Span{byte}, int)"/>
    public static void Rotate<T>(this Image<T> image, ImageRotation rotation, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        ArgumentNullException.ThrowIfNull(image);

        image.AsRefImage().Rotate(rotation, destination, stride);
    }

    /// <inheritdoc cref="Rotate(IImage, ImageRotation)"/>
    public static Image<T> Rotate<T>(this RefImage<T> image, ImageRotation rotation)
        where T : unmanaged, IColor<T>
    {
        (int width, int height) = GetRotatedSize(image.Width, image.Height, rotation);

        int rowSize = width * T.ColorFormat.BytesPerPixel;
        byte[] buffer = GC.AllocateUninitializedArray<byte>(rowSize * height);
        RotateUnchecked(image, rotation, buffer, rowSize);

        return Image<T>.Wrap(buffer, width, height, rowSize);
    }

    /// <inheritdoc cref="Rotate{T}(Image{T}, ImageRotation, Span{T})"/>
    public static void Rotate<T>(this RefImage<T> image, ImageRotation rotation, Span<T> destination)
        where T : unmanaged, IColor<T>
    {
        (int width, int height) = GetRotatedSize(image.Width, image.Height, rotation);
        Span<byte> target = ImageValidation.ValidateDestination(image, destination, width, height);

        RotateUnchecked(image, rotation, target, width * T.ColorFormat.BytesPerPixel);
    }

    /// <inheritdoc cref="Rotate(IImage, ImageRotation, Span{byte}, int)"/>
    public static void Rotate<T>(this RefImage<T> image, ImageRotation rotation, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        (int width, int height) = GetRotatedSize(image.Width, image.Height, rotation);
        ImageValidation.ValidateDestination(image, destination, stride, width, height);

        RotateUnchecked(image, rotation, destination, stride);
    }

    private static void RotateUnchecked<T>(RefImage<T> image, ImageRotation rotation, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        if ((image.Width == 0) || (image.Height == 0)) return;

        switch (rotation)
        {
            case ImageRotation.Rotate90:
                Rotate(image, true, destination, stride);
                break;

            case ImageRotation.Rotate180:
                Flip(image, true, true, destination, stride);
                break;

            case ImageRotation.Rotate270:
                Rotate(image, false, destination, stride);
                break;

            default:
                image.CopyTo(destination, stride);
                break;
        }
    }

    private static void Rotate<T>(RefImage<T> image, bool clockwise, Span<byte> destination, int stride)
        where T : unmanaged, IColor<T>
    {
        int targetWidth = image.Height;
        int targetHeight = image.Width;
        int block = GetRotateBlockSize<T>();

        int blocksWidth = block == 0 ? 0 : targetWidth - (targetWidth % block);
        int blocksHeight = block == 0 ? 0 : targetHeight - (targetHeight % block);

        if ((blocksWidth > 0) && (blocksHeight > 0))
            RotateBlocks(image, clockwise, destination, stride, blocksWidth, blocksHeight);

        RotateScalar(image, clockwise, destination, stride, blocksWidth, targetWidth, 0, targetHeight);
        RotateScalar(image, clockwise, destination, stride, 0, blocksWidth, blocksHeight, targetHeight);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetRotateBlockSize<T>()
        where T : unmanaged, IColor<T>
        => T.ColorFormat.BytesPerPixel switch
        {
            4 when Vector512.IsHardwareAccelerated && Avx512F.IsSupported => 16,
            4 or 3 when Vector256.IsHardwareAccelerated && Avx2.IsSupported => 8,
            4 when Vector128.IsHardwareAccelerated => 8,
            _ => 0
        };

    private static void RotateBlocks<T>(RefImage<T> image, bool clockwise, Span<byte> destination, int stride, int blocksWidth, int blocksHeight)
        where T : unmanaged, IColor<T>
    {
        int block = GetRotateBlockSize<T>();
        int bpp = T.ColorFormat.BytesPerPixel;

        ref byte src = ref Unsafe.AsRef(in image.GetPinnableReference());
        ref byte tar = ref MemoryMarshal.GetReference(destination);
        nint sourceStride = image.Stride;
        nint targetStride = stride;

        nint sourceStep = clockwise ? -sourceStride : sourceStride;
        nint targetStep = clockwise ? targetStride : -targetStride;

        for (int band = 0; band < blocksHeight; band += ROTATE_BAND_HEIGHT)
        {
            int bandEnd = Math.Min(band + ROTATE_BAND_HEIGHT, blocksHeight);

            for (int x = 0; x < blocksWidth; x += block)
                for (int i = band; i < bandEnd; i += block)
                {
                    int y = clockwise ? i : blocksHeight - block - i;
                    nint sourceRow = clockwise ? image.Height - 1 - x : x;
                    nint sourceColumn = clockwise ? y : image.Width - block - y;

                    ref byte s = ref Unsafe.Add(ref src, (sourceRow * sourceStride) + (sourceColumn * bpp));
                    ref byte t = ref Unsafe.Add(ref tar, (GetBlockTargetRow(y, block, clockwise) * targetStride) + ((nint)x * bpp));

                    int ahead = i + (ROTATE_PREFETCH_BLOCKS * block);
                    int aheadX = x;
                    if (ahead >= bandEnd)
                    {
                        ahead = band + (ahead - bandEnd);
                        aheadX = x + block;
                    }

                    if (Sse.IsSupported && (ahead < bandEnd) && (aheadX < blocksWidth))
                    {
                        int aheadY = clockwise ? ahead : blocksHeight - block - ahead;
                        PrefetchRows(ref Unsafe.Add(ref tar, (GetBlockTargetRow(aheadY, block, clockwise) * targetStride) + ((nint)aheadX * bpp)), targetStep, block);
                    }

                    if (bpp == 3)
                        Transpose8x8x3Bytes(ref s, sourceStep, ref t, targetStep);
                    else if (block == 16)
                        Transpose16x16x4Bytes(ref s, sourceStep, ref t, targetStep);
                    else if (Vector256.IsHardwareAccelerated && Avx2.IsSupported)
                        Transpose8x8x4Bytes(ref s, sourceStep, ref t, targetStep);
                    else
                        Transpose8x8x4BytesPortable(ref s, sourceStep, ref t, targetStep);
                }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint GetBlockTargetRow(int y, int block, bool clockwise) => clockwise ? y : (y + block) - 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void PrefetchRows(ref byte first, nint step, int rows)
    {
        byte* address = (byte*)Unsafe.AsPointer(ref first);
        for (int row = 0; row < rows; row++)
            Sse.Prefetch0(address + (row * step));
    }

    private static void RotateScalar<T>(RefImage<T> image, bool clockwise, Span<byte> destination, int stride, int fromX, int toX, int fromY, int toY)
        where T : unmanaged, IColor<T>
    {
        if ((fromX >= toX) || (fromY >= toY)) return;

        int bpp = T.ColorFormat.BytesPerPixel;
        ref byte src = ref Unsafe.AsRef(in image.GetPinnableReference());
        ref byte tar = ref MemoryMarshal.GetReference(destination);
        nint sourceStep = clockwise ? -image.Stride : image.Stride;

        for (int tileY = fromY; tileY < toY; tileY += SCALAR_ROTATE_TILE_SIZE)
        {
            int tileYEnd = Math.Min(tileY + SCALAR_ROTATE_TILE_SIZE, toY);

            for (int tileX = fromX; tileX < toX; tileX += SCALAR_ROTATE_TILE_SIZE)
            {
                int tileXEnd = Math.Min(tileX + SCALAR_ROTATE_TILE_SIZE, toX);

                for (int y = tileY; y < tileYEnd; y++)
                {
                    int sourceX = clockwise ? y : image.Width - 1 - y;
                    int sourceY = clockwise ? image.Height - 1 - tileX : tileX;
                    nint sourceOffset = ((nint)sourceY * image.Stride) + ((nint)sourceX * bpp);
                    ref T target = ref Unsafe.As<byte, T>(ref Unsafe.Add(ref tar, (nint)y * stride));

                    for (int x = tileX; x < tileXEnd; x++)
                    {
                        Unsafe.Add(ref target, x) = Unsafe.As<byte, T>(ref Unsafe.Add(ref src, sourceOffset));
                        sourceOffset += sourceStep;
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose8x8x4Bytes(ref byte source, nint sourceStep, ref byte target, nint targetStep)
    {
        Vector256<uint> r0 = Vector256.LoadUnsafe(ref Unsafe.As<byte, uint>(ref source));
        Vector256<uint> r1 = Vector256.LoadUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref source, sourceStep)));
        Vector256<uint> r2 = Vector256.LoadUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref source, 2 * sourceStep)));
        Vector256<uint> r3 = Vector256.LoadUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref source, 3 * sourceStep)));
        Vector256<uint> r4 = Vector256.LoadUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref source, 4 * sourceStep)));
        Vector256<uint> r5 = Vector256.LoadUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref source, 5 * sourceStep)));
        Vector256<uint> r6 = Vector256.LoadUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref source, 6 * sourceStep)));
        Vector256<uint> r7 = Vector256.LoadUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref source, 7 * sourceStep)));

        Transpose8x8(ref r0, ref r1, ref r2, ref r3, ref r4, ref r5, ref r6, ref r7);

        r0.StoreUnsafe(ref Unsafe.As<byte, uint>(ref target));
        r1.StoreUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref target, targetStep)));
        r2.StoreUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref target, 2 * targetStep)));
        r3.StoreUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref target, 3 * targetStep)));
        r4.StoreUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref target, 4 * targetStep)));
        r5.StoreUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref target, 5 * targetStep)));
        r6.StoreUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref target, 6 * targetStep)));
        r7.StoreUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref target, 7 * targetStep)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose8x8x4BytesPortable(ref byte source, nint sourceStep, ref byte target, nint targetStep)
    {
        Transpose4x4x4Bytes(ref source, sourceStep, ref target, targetStep);
        Transpose4x4x4Bytes(ref Unsafe.Add(ref source, 16), sourceStep, ref Unsafe.Add(ref target, 4 * targetStep), targetStep);
        Transpose4x4x4Bytes(ref Unsafe.Add(ref source, 4 * sourceStep), sourceStep, ref Unsafe.Add(ref target, 16), targetStep);
        Transpose4x4x4Bytes(ref Unsafe.Add(ref source, (4 * sourceStep) + 16), sourceStep, ref Unsafe.Add(ref target, (4 * targetStep) + 16), targetStep);
    }

    // Joining the halves of two rows gives (a0, a1, b0, b1), the shuffle (a0, b0, a1, b1), and joining the halves of those again the columns.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose4x4x4Bytes(ref byte source, nint sourceStep, ref byte target, nint targetStep)
    {
        Vector128<ulong> r0 = Vector128.LoadUnsafe(ref Unsafe.As<byte, ulong>(ref source));
        Vector128<ulong> r1 = Vector128.LoadUnsafe(ref Unsafe.As<byte, ulong>(ref Unsafe.Add(ref source, sourceStep)));
        Vector128<ulong> r2 = Vector128.LoadUnsafe(ref Unsafe.As<byte, ulong>(ref Unsafe.Add(ref source, 2 * sourceStep)));
        Vector128<ulong> r3 = Vector128.LoadUnsafe(ref Unsafe.As<byte, ulong>(ref Unsafe.Add(ref source, 3 * sourceStep)));

        Vector128<uint> order = Vector128.Create(0u, 2, 1, 3);
        Vector128<ulong> t0 = Vector128.Shuffle(JoinLower(r0, r1).AsUInt32(), order).AsUInt64();
        Vector128<ulong> t1 = Vector128.Shuffle(JoinUpper(r0, r1).AsUInt32(), order).AsUInt64();
        Vector128<ulong> t2 = Vector128.Shuffle(JoinLower(r2, r3).AsUInt32(), order).AsUInt64();
        Vector128<ulong> t3 = Vector128.Shuffle(JoinUpper(r2, r3).AsUInt32(), order).AsUInt64();

        JoinLower(t0, t2).StoreUnsafe(ref Unsafe.As<byte, ulong>(ref target));
        JoinUpper(t0, t2).StoreUnsafe(ref Unsafe.As<byte, ulong>(ref Unsafe.Add(ref target, targetStep)));
        JoinLower(t1, t3).StoreUnsafe(ref Unsafe.As<byte, ulong>(ref Unsafe.Add(ref target, 2 * targetStep)));
        JoinUpper(t1, t3).StoreUnsafe(ref Unsafe.As<byte, ulong>(ref Unsafe.Add(ref target, 3 * targetStep)));
    }

    // Like Transpose8x8x4Bytes, but rows are 24 bytes: each is read with two overlapping 16-byte loads and widened to 4 bytes per pixel,
    // then narrowed back after the transpose and written as 16 + 8 bytes, so no byte outside the block is touched.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose8x8x3Bytes(ref byte source, nint sourceStep, ref byte target, nint targetStep)
    {
        Vector256<byte> widen = Vector256.Create(0, 1, 2, 0x80, 3, 4, 5, 0x80, 6, 7, 8, 0x80, 9, 10, 11, 0x80,
                                                 4, 5, 6, 0x80, 7, 8, 9, 0x80, 10, 11, 12, 0x80, 13, 14, 15, 0x80);
        Vector256<byte> narrow = Vector256.Create(0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 0x80, 0x80, 0x80, 0x80,
                                                  0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 0x80, 0x80, 0x80, 0x80);
        Vector256<uint> join = Vector256.Create(0u, 1, 2, 4, 5, 6, 3, 7);

        Vector256<uint> r0 = Load3BytesRow(ref source, widen);
        Vector256<uint> r1 = Load3BytesRow(ref Unsafe.Add(ref source, sourceStep), widen);
        Vector256<uint> r2 = Load3BytesRow(ref Unsafe.Add(ref source, 2 * sourceStep), widen);
        Vector256<uint> r3 = Load3BytesRow(ref Unsafe.Add(ref source, 3 * sourceStep), widen);
        Vector256<uint> r4 = Load3BytesRow(ref Unsafe.Add(ref source, 4 * sourceStep), widen);
        Vector256<uint> r5 = Load3BytesRow(ref Unsafe.Add(ref source, 5 * sourceStep), widen);
        Vector256<uint> r6 = Load3BytesRow(ref Unsafe.Add(ref source, 6 * sourceStep), widen);
        Vector256<uint> r7 = Load3BytesRow(ref Unsafe.Add(ref source, 7 * sourceStep), widen);

        Transpose8x8(ref r0, ref r1, ref r2, ref r3, ref r4, ref r5, ref r6, ref r7);

        Store3BytesRow(r0, ref target, narrow, join);
        Store3BytesRow(r1, ref Unsafe.Add(ref target, targetStep), narrow, join);
        Store3BytesRow(r2, ref Unsafe.Add(ref target, 2 * targetStep), narrow, join);
        Store3BytesRow(r3, ref Unsafe.Add(ref target, 3 * targetStep), narrow, join);
        Store3BytesRow(r4, ref Unsafe.Add(ref target, 4 * targetStep), narrow, join);
        Store3BytesRow(r5, ref Unsafe.Add(ref target, 5 * targetStep), narrow, join);
        Store3BytesRow(r6, ref Unsafe.Add(ref target, 6 * targetStep), narrow, join);
        Store3BytesRow(r7, ref Unsafe.Add(ref target, 7 * targetStep), narrow, join);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> Load3BytesRow(ref byte source, Vector256<byte> widen)
        => Avx2.Shuffle(Vector256.Create(Vector128.LoadUnsafe(ref source), Vector128.LoadUnsafe(ref source, 8)), widen).AsUInt32();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Store3BytesRow(Vector256<uint> row, ref byte target, Vector256<byte> narrow, Vector256<uint> join)
    {
        Vector256<byte> packed = Avx2.PermuteVar8x32(Avx2.Shuffle(row.AsByte(), narrow).AsUInt32(), join).AsByte();
        packed.GetLower().StoreUnsafe(ref target);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref target, 16), packed.GetUpper().AsUInt64().ToScalar());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose8x8(ref Vector256<uint> r0, ref Vector256<uint> r1, ref Vector256<uint> r2, ref Vector256<uint> r3,
                                     ref Vector256<uint> r4, ref Vector256<uint> r5, ref Vector256<uint> r6, ref Vector256<uint> r7)
    {
        Vector256<uint> t0 = Avx2.UnpackLow(r0, r1);
        Vector256<uint> t1 = Avx2.UnpackHigh(r0, r1);
        Vector256<uint> t2 = Avx2.UnpackLow(r2, r3);
        Vector256<uint> t3 = Avx2.UnpackHigh(r2, r3);
        Vector256<uint> t4 = Avx2.UnpackLow(r4, r5);
        Vector256<uint> t5 = Avx2.UnpackHigh(r4, r5);
        Vector256<uint> t6 = Avx2.UnpackLow(r6, r7);
        Vector256<uint> t7 = Avx2.UnpackHigh(r6, r7);

        Vector256<uint> u0 = Avx2.UnpackLow(t0.AsUInt64(), t2.AsUInt64()).AsUInt32();
        Vector256<uint> u1 = Avx2.UnpackHigh(t0.AsUInt64(), t2.AsUInt64()).AsUInt32();
        Vector256<uint> u2 = Avx2.UnpackLow(t1.AsUInt64(), t3.AsUInt64()).AsUInt32();
        Vector256<uint> u3 = Avx2.UnpackHigh(t1.AsUInt64(), t3.AsUInt64()).AsUInt32();
        Vector256<uint> u4 = Avx2.UnpackLow(t4.AsUInt64(), t6.AsUInt64()).AsUInt32();
        Vector256<uint> u5 = Avx2.UnpackHigh(t4.AsUInt64(), t6.AsUInt64()).AsUInt32();
        Vector256<uint> u6 = Avx2.UnpackLow(t5.AsUInt64(), t7.AsUInt64()).AsUInt32();
        Vector256<uint> u7 = Avx2.UnpackHigh(t5.AsUInt64(), t7.AsUInt64()).AsUInt32();

        r0 = Avx2.Permute2x128(u0, u4, 0x20);
        r1 = Avx2.Permute2x128(u1, u5, 0x20);
        r2 = Avx2.Permute2x128(u2, u6, 0x20);
        r3 = Avx2.Permute2x128(u3, u7, 0x20);
        r4 = Avx2.Permute2x128(u0, u4, 0x31);
        r5 = Avx2.Permute2x128(u1, u5, 0x31);
        r6 = Avx2.Permute2x128(u2, u6, 0x31);
        r7 = Avx2.Permute2x128(u3, u7, 0x31);
    }

    // Like Transpose8x8x4Bytes with 16 rows of 16 pixels.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose16x16x4Bytes(ref byte source, nint sourceStep, ref byte target, nint targetStep)
    {
        Vector512<uint> r0 = Load512(ref source, 0, sourceStep);
        Vector512<uint> r1 = Load512(ref source, 1, sourceStep);
        Vector512<uint> r2 = Load512(ref source, 2, sourceStep);
        Vector512<uint> r3 = Load512(ref source, 3, sourceStep);
        Vector512<uint> r4 = Load512(ref source, 4, sourceStep);
        Vector512<uint> r5 = Load512(ref source, 5, sourceStep);
        Vector512<uint> r6 = Load512(ref source, 6, sourceStep);
        Vector512<uint> r7 = Load512(ref source, 7, sourceStep);
        Vector512<uint> r8 = Load512(ref source, 8, sourceStep);
        Vector512<uint> r9 = Load512(ref source, 9, sourceStep);
        Vector512<uint> r10 = Load512(ref source, 10, sourceStep);
        Vector512<uint> r11 = Load512(ref source, 11, sourceStep);
        Vector512<uint> r12 = Load512(ref source, 12, sourceStep);
        Vector512<uint> r13 = Load512(ref source, 13, sourceStep);
        Vector512<uint> r14 = Load512(ref source, 14, sourceStep);
        Vector512<uint> r15 = Load512(ref source, 15, sourceStep);

        Transpose4x4Lanes(ref r0, ref r1, ref r2, ref r3);
        Transpose4x4Lanes(ref r4, ref r5, ref r6, ref r7);
        Transpose4x4Lanes(ref r8, ref r9, ref r10, ref r11);
        Transpose4x4Lanes(ref r12, ref r13, ref r14, ref r15);

        StoreColumns(r0, r4, r8, r12, ref target, targetStep);
        StoreColumns(r1, r5, r9, r13, ref Unsafe.Add(ref target, targetStep), targetStep);
        StoreColumns(r2, r6, r10, r14, ref Unsafe.Add(ref target, 2 * targetStep), targetStep);
        StoreColumns(r3, r7, r11, r15, ref Unsafe.Add(ref target, 3 * targetStep), targetStep);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<uint> Load512(ref byte source, int row, nint sourceStep)
        => Vector512.LoadUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref source, row * sourceStep)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose4x4Lanes(ref Vector512<uint> r0, ref Vector512<uint> r1, ref Vector512<uint> r2, ref Vector512<uint> r3)
    {
        Vector512<uint> t0 = Avx512F.UnpackLow(r0, r1);
        Vector512<uint> t1 = Avx512F.UnpackHigh(r0, r1);
        Vector512<uint> t2 = Avx512F.UnpackLow(r2, r3);
        Vector512<uint> t3 = Avx512F.UnpackHigh(r2, r3);

        r0 = Avx512F.UnpackLow(t0.AsUInt64(), t2.AsUInt64()).AsUInt32();
        r1 = Avx512F.UnpackHigh(t0.AsUInt64(), t2.AsUInt64()).AsUInt32();
        r2 = Avx512F.UnpackLow(t1.AsUInt64(), t3.AsUInt64()).AsUInt32();
        r3 = Avx512F.UnpackHigh(t1.AsUInt64(), t3.AsUInt64()).AsUInt32();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StoreColumns(Vector512<uint> a, Vector512<uint> b, Vector512<uint> c, Vector512<uint> d, ref byte target, nint targetStep)
    {
        Vector512<uint> v0 = Avx512F.Shuffle4x128(a, b, 0b10_00_10_00);
        Vector512<uint> v1 = Avx512F.Shuffle4x128(a, b, 0b11_01_11_01);
        Vector512<uint> v2 = Avx512F.Shuffle4x128(c, d, 0b10_00_10_00);
        Vector512<uint> v3 = Avx512F.Shuffle4x128(c, d, 0b11_01_11_01);

        Avx512F.Shuffle4x128(v0, v2, 0b10_00_10_00).StoreUnsafe(ref Unsafe.As<byte, uint>(ref target));
        Avx512F.Shuffle4x128(v1, v3, 0b10_00_10_00).StoreUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref target, 4 * targetStep)));
        Avx512F.Shuffle4x128(v0, v2, 0b11_01_11_01).StoreUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref target, 8 * targetStep)));
        Avx512F.Shuffle4x128(v1, v3, 0b11_01_11_01).StoreUnsafe(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref target, 12 * targetStep)));
    }

    private static (int Width, int Height) GetRotatedSize(int width, int height, ImageRotation rotation)
        => rotation switch
        {
            ImageRotation.None or ImageRotation.Rotate180 => (width, height),
            ImageRotation.Rotate90 or ImageRotation.Rotate270 => (height, width),
            _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, "The rotation has to be one of the values of ImageRotation.")
        };

    #endregion

    private readonly struct RotateOperation(ImageRotation rotation) : IImageOperation<IImage>
    {
        public IImage Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image.Rotate(rotation);
    }

    private readonly ref struct RotateIntoOperation(ImageRotation rotation, Span<byte> destination, int stride) : IImageOperation<bool>
    {
        private readonly Span<byte> _destination = destination;

        public bool Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T>
        {
            image.Rotate(rotation, _destination, stride);
            return true;
        }
    }
}
