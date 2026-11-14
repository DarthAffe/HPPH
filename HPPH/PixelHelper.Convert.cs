using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace HPPH;

public static partial class PixelHelper
{
    #region Constants

    // All masks of one format pair live in one array. The constants are offsets into it, the comments give the length.
    // A pair uses only the layout of its pixel sizes, so offsets of different layouts overlap.
    // map: target byte k of a pixel is source byte map[k] of the same pixel. Index 0x80 in a shuffle mask writes 0.
    private const int MASK_3_128 = 0;        // 6 x 16: per output vector a mask for the load at window and one for window + 2
    private const int MASK_3_512 = 96;       // 3 x 64: vpermi2b indices per output vector, 64+ selects the load at window + 2
    private const int MASK_3_MAP = 288;      // 3: map
    private const int MASK_4_LANE = 0;       // 16: pshufb indices for 4 pixels, repeated in every 16-byte lane
    private const int MASK_4_MAP = 16;       // 4: map
    private const int MASK_WIDEN = 0;        // 64: vpermb indices for 16 pixels, the first 16 serve as pshufb lane mask
    private const int MASK_WIDEN_ALPHA = 64; // 64: or-ed into the result, 0xFF where the target byte is alpha
    private const int MASK_WIDEN_MAP = 128;  // 4: map, alpha reads byte 0 and gets 0xFF from the alpha mask
    private const int MASK_NARROW_LANE = 0;  // 16: packs 4 pixels into the first 12 bytes of a lane
    private const int MASK_NARROW_512 = 16;  // 64: vpermb indices, packs 16 pixels into the first 48 bytes
    private const int MASK_NARROW_MAP = 80;  // 3: map

    // Block sizes in bytes of the 3-byte kernels: 3 vectors, a whole number of pixels.
    private const int SHUFFLE_3_BLOCK_128 = 48;
    private const int SHUFFLE_3_BLOCK_512 = 192;

    #endregion

    #region Methods

    /// <summary>
    /// Converts <paramref name="colors"/> to the color format <typeparamref name="TTarget"/> in place.
    /// </summary>
    /// <param name="colors">The colors to convert.</param>
    /// <typeparam name="TSource">The color format to convert from.</typeparam>
    /// <typeparam name="TTarget">The color format to convert to. Needs the same bytes per pixel as <typeparamref name="TSource"/>.</typeparam>
    /// <returns>The converted colors, sharing the memory of <paramref name="colors"/>.</returns>
    /// <exception cref="NotSupportedException"><typeparamref name="TSource"/> and <typeparamref name="TTarget"/> have different bytes per pixel.</exception>
    public static Span<TTarget> ConvertInPlace<TSource, TTarget>(this Span<TSource> colors)
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        if (colors.Length == 0) return MemoryMarshal.Cast<TSource, TTarget>(colors);

        IColorFormat sourceFormat = TSource.ColorFormat;
        IColorFormat targetFormat = TTarget.ColorFormat;

        if (sourceFormat == targetFormat) return MemoryMarshal.Cast<TSource, TTarget>(colors);
        if (sourceFormat.BytesPerPixel != targetFormat.BytesPerPixel) throw new NotSupportedException("In-place conversion requires the same BPP for source and target.");

        Span<byte> data = MemoryMarshal.AsBytes(colors);
        Convert<TSource, TTarget>(data, data);

        return MemoryMarshal.Cast<byte, TTarget>(data);
    }

    /// <summary>
    /// Allocates a new array and converts <paramref name="colors"/> into it.
    /// </summary>
    /// <param name="colors">The colors to convert.</param>
    /// <typeparam name="TSource">The color format to convert from.</typeparam>
    /// <typeparam name="TTarget">The color format to convert to.</typeparam>
    /// <returns>The new array containing the converted colors.</returns>
    public static TTarget[] Convert<TSource, TTarget>(this ReadOnlySpan<TSource> colors)
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        TTarget[] buffer = GC.AllocateUninitializedArray<TTarget>(colors.Length);
        colors.Convert(buffer.AsSpan());
        return buffer;
    }

    /// <summary>
    /// Converts <paramref name="source"/> into <paramref name="target"/>.
    /// </summary>
    /// <param name="source">The colors to convert.</param>
    /// <param name="target">The buffer to write to. Needs at least as many elements as <paramref name="source"/>.</param>
    /// <typeparam name="TSource">The color format to convert from.</typeparam>
    /// <typeparam name="TTarget">The color format to convert to.</typeparam>
    /// <exception cref="ArgumentException"><paramref name="target"/> is shorter than <paramref name="source"/>.</exception>
    public static void Convert<TSource, TTarget>(this ReadOnlySpan<TSource> source, Span<TTarget> target)
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        if (target.Length < source.Length) throw new ArgumentException($"Target-buffer is not big enough. {target.Length} < {source.Length}", nameof(target));

        Convert<TSource, TTarget>(MemoryMarshal.AsBytes(source), MemoryMarshal.AsBytes(target));
    }

    private static void Convert<TSource, TTarget>(ReadOnlySpan<byte> source, Span<byte> target)
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        if (source.Length == 0) return;

        if (TSource.ColorFormat == TTarget.ColorFormat)
        {
            source.CopyTo(target);
            return;
        }

        ReadOnlySpan<byte> masks = ConvertMasks<TSource, TTarget>.Masks;

        switch (TSource.ColorFormat.BytesPerPixel, TTarget.ColorFormat.BytesPerPixel)
        {
            case (3, 3):
                Convert3Bytes(source, target, masks);
                break;

            case (4, 4):
                Convert4Bytes(source, target, masks);
                break;

            case (3, 4):
                ConvertWiden3To4Bytes(source, target, masks);
                break;

            case (4, 3):
                ConvertNarrow4To3Bytes(source, target, masks);
                break;

            default:
                throw new NotSupportedException("Data is not of a supported valid color-type.");
        }
    }
    
    // 3-byte pixels don't fit vectors: a 16-byte vector holds 5 1/3 pixels. We therfore work on blocks of 3 vectors (48 or 192 bytes, a whole number of pixels)
    // Since the pixel windows doesn't line up with the 16-byte lanes of vpshufb, there is no AVX2 path. With the overhead it would cause to make that work the AVX-path becomes competitive.
    private static void Convert3Bytes(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> masks)
    {
        ref byte src = ref MemoryMarshal.GetReference(source);
        ref byte tar = ref MemoryMarshal.GetReference(target);
        nuint length = (nuint)source.Length;

        if (Vector512.IsHardwareAccelerated && Avx512Vbmi.IsSupported && (length >= SHUFFLE_3_BLOCK_512))
            Shuffle3BytesVbmi(ref src, ref tar, length, masks);
        else if (Vector128.IsHardwareAccelerated && (length >= SHUFFLE_3_BLOCK_128))
            Shuffle3Bytes(ref src, ref tar, length, masks);
        else
            Shuffle3BytesScalar(ref src, ref tar, length, masks);
    }

    private static void Shuffle3BytesVbmi(ref byte src, ref byte tar, nuint length, ReadOnlySpan<byte> masks)
    {
        // Windows 0, 63, 126, loads at window and window + 2, and one two-source byte permute (vpermi2b) per output vector instead of two shuffles and an or.
        ref byte m = ref Unsafe.Add(ref MemoryMarshal.GetReference(masks), MASK_3_512);
        Vector512<byte> idx0 = Vector512.LoadUnsafe(ref m, 0);
        Vector512<byte> idx1 = Vector512.LoadUnsafe(ref m, 64);
        Vector512<byte> idx2 = Vector512.LoadUnsafe(ref m, 128);

        nuint lastOffset = length - SHUFFLE_3_BLOCK_512;
        Vector512<byte> l0 = Vector512.LoadUnsafe(ref src, lastOffset);
        Vector512<byte> l1 = Vector512.LoadUnsafe(ref src, lastOffset + 2);
        Vector512<byte> l2 = Vector512.LoadUnsafe(ref src, lastOffset + 63);
        Vector512<byte> l3 = Vector512.LoadUnsafe(ref src, lastOffset + 65);
        Vector512<byte> l4 = Vector512.LoadUnsafe(ref src, lastOffset + 126);
        Vector512<byte> l5 = Vector512.LoadUnsafe(ref src, lastOffset + 128);

        for (nuint i = 0; i < lastOffset; i += SHUFFLE_3_BLOCK_512)
        {
            Vector512<byte> v0 = Vector512.LoadUnsafe(ref src, i);
            Vector512<byte> v1 = Vector512.LoadUnsafe(ref src, i + 2);
            Vector512<byte> v2 = Vector512.LoadUnsafe(ref src, i + 63);
            Vector512<byte> v3 = Vector512.LoadUnsafe(ref src, i + 65);
            Vector512<byte> v4 = Vector512.LoadUnsafe(ref src, i + 126);
            Vector512<byte> v5 = Vector512.LoadUnsafe(ref src, i + 128);

            Avx512Vbmi.PermuteVar64x8x2(v0, idx0, v1).StoreUnsafe(ref tar, i);
            Avx512Vbmi.PermuteVar64x8x2(v2, idx1, v3).StoreUnsafe(ref tar, i + 64);
            Avx512Vbmi.PermuteVar64x8x2(v4, idx2, v5).StoreUnsafe(ref tar, i + 128);
        }

        Avx512Vbmi.PermuteVar64x8x2(l0, idx0, l1).StoreUnsafe(ref tar, lastOffset);
        Avx512Vbmi.PermuteVar64x8x2(l2, idx1, l3).StoreUnsafe(ref tar, lastOffset + 64);
        Avx512Vbmi.PermuteVar64x8x2(l4, idx2, l5).StoreUnsafe(ref tar, lastOffset + 128);
    }

    private static void Shuffle3Bytes(ref byte src, ref byte tar, nuint length, ReadOnlySpan<byte> masks)
    {
        // Output vector k: windows 0, 15, 30, the two loads shuffled separately (0x80 zeroes) and or-ed.
        ref byte m = ref Unsafe.Add(ref MemoryMarshal.GetReference(masks), MASK_3_128);
        Vector128<byte> lo0 = Vector128.LoadUnsafe(ref m, 0);
        Vector128<byte> hi0 = Vector128.LoadUnsafe(ref m, 16);
        Vector128<byte> lo1 = Vector128.LoadUnsafe(ref m, 32);
        Vector128<byte> hi1 = Vector128.LoadUnsafe(ref m, 48);
        Vector128<byte> lo2 = Vector128.LoadUnsafe(ref m, 64);
        Vector128<byte> hi2 = Vector128.LoadUnsafe(ref m, 80);

        nuint lastOffset = length - SHUFFLE_3_BLOCK_128;
        Vector128<byte> l0 = Vector128.LoadUnsafe(ref src, lastOffset);
        Vector128<byte> l1 = Vector128.LoadUnsafe(ref src, lastOffset + 2);
        Vector128<byte> l2 = Vector128.LoadUnsafe(ref src, lastOffset + 15);
        Vector128<byte> l3 = Vector128.LoadUnsafe(ref src, lastOffset + 17);
        Vector128<byte> l4 = Vector128.LoadUnsafe(ref src, lastOffset + 30);
        Vector128<byte> l5 = Vector128.LoadUnsafe(ref src, lastOffset + 32);

        for (nuint i = 0; i < lastOffset; i += SHUFFLE_3_BLOCK_128)
        {
            Vector128<byte> v0 = Vector128.LoadUnsafe(ref src, i);
            Vector128<byte> v1 = Vector128.LoadUnsafe(ref src, i + 2);
            Vector128<byte> v2 = Vector128.LoadUnsafe(ref src, i + 15);
            Vector128<byte> v3 = Vector128.LoadUnsafe(ref src, i + 17);
            Vector128<byte> v4 = Vector128.LoadUnsafe(ref src, i + 30);
            Vector128<byte> v5 = Vector128.LoadUnsafe(ref src, i + 32);

            (Vector128.ShuffleNative(v0, lo0) | Vector128.ShuffleNative(v1, hi0)).StoreUnsafe(ref tar, i);
            (Vector128.ShuffleNative(v2, lo1) | Vector128.ShuffleNative(v3, hi1)).StoreUnsafe(ref tar, i + 16);
            (Vector128.ShuffleNative(v4, lo2) | Vector128.ShuffleNative(v5, hi2)).StoreUnsafe(ref tar, i + 32);
        }

        (Vector128.ShuffleNative(l0, lo0) | Vector128.ShuffleNative(l1, hi0)).StoreUnsafe(ref tar, lastOffset);
        (Vector128.ShuffleNative(l2, lo1) | Vector128.ShuffleNative(l3, hi1)).StoreUnsafe(ref tar, lastOffset + 16);
        (Vector128.ShuffleNative(l4, lo2) | Vector128.ShuffleNative(l5, hi2)).StoreUnsafe(ref tar, lastOffset + 32);
    }

    private static void Shuffle3BytesScalar(ref byte src, ref byte tar, nuint length, ReadOnlySpan<byte> masks)
    {
        nuint map0 = masks[MASK_3_MAP];
        nuint map1 = masks[MASK_3_MAP + 1];
        nuint map2 = masks[MASK_3_MAP + 2];

        for (nuint i = 0; i < length; i += 3)
        {
            byte b0 = Unsafe.Add(ref src, i + map0);
            byte b1 = Unsafe.Add(ref src, i + map1);
            byte b2 = Unsafe.Add(ref src, i + map2);

            Unsafe.Add(ref tar, i) = b0;
            Unsafe.Add(ref tar, i + 1) = b1;
            Unsafe.Add(ref tar, i + 2) = b2;
        }
    }
    
    private static void Convert4Bytes(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> masks)
    {
        ref byte src = ref MemoryMarshal.GetReference(source);
        ref byte tar = ref MemoryMarshal.GetReference(target);
        nuint length = (nuint)source.Length;

        if (length < (nuint)Vector128<byte>.Count || !Vector128.IsHardwareAccelerated)
        {
            Shuffle4BytesScalar(ref src, ref tar, length, masks);
            return;
        }

        Vector128<byte> mask = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(masks), MASK_4_LANE);

        if (Vector512.IsHardwareAccelerated && Avx512BW.IsSupported && (length >= (nuint)Vector512<byte>.Count))
            Shuffle4Bytes(ref src, ref tar, length, Vector512.Create(Vector256.Create(mask, mask), Vector256.Create(mask, mask)));
        else if (Avx2.IsSupported && (length >= (nuint)Vector256<byte>.Count))
            Shuffle4Bytes(ref src, ref tar, length, Vector256.Create(mask, mask));
        else
            Shuffle4Bytes(ref src, ref tar, length, mask);
    }

    private static void Shuffle4Bytes(ref byte src, ref byte tar, nuint length, Vector512<byte> mask)
    {
        nuint size = (nuint)Vector512<byte>.Count;
        nuint lastOffset = length - size;
        Vector512<byte> last = Vector512.LoadUnsafe(ref src, lastOffset);

        nuint i = 0;
        for (; (i + (4 * size)) <= length; i += 4 * size)
        {
            Vector512<byte> v0 = Vector512.LoadUnsafe(ref src, i);
            Vector512<byte> v1 = Vector512.LoadUnsafe(ref src, i + size);
            Vector512<byte> v2 = Vector512.LoadUnsafe(ref src, i + (2 * size));
            Vector512<byte> v3 = Vector512.LoadUnsafe(ref src, i + (3 * size));

            Avx512BW.Shuffle(v0, mask).StoreUnsafe(ref tar, i);
            Avx512BW.Shuffle(v1, mask).StoreUnsafe(ref tar, i + size);
            Avx512BW.Shuffle(v2, mask).StoreUnsafe(ref tar, i + (2 * size));
            Avx512BW.Shuffle(v3, mask).StoreUnsafe(ref tar, i + (3 * size));
        }

        for (; i < lastOffset; i += size)
            Avx512BW.Shuffle(Vector512.LoadUnsafe(ref src, i), mask).StoreUnsafe(ref tar, i);

        Avx512BW.Shuffle(last, mask).StoreUnsafe(ref tar, lastOffset);
    }

    private static void Shuffle4Bytes(ref byte src, ref byte tar, nuint length, Vector256<byte> mask)
    {
        nuint size = (nuint)Vector256<byte>.Count;
        nuint lastOffset = length - size;
        Vector256<byte> last = Vector256.LoadUnsafe(ref src, lastOffset);

        nuint i = 0;
        for (; (i + (4 * size)) <= length; i += 4 * size)
        {
            Vector256<byte> v0 = Vector256.LoadUnsafe(ref src, i);
            Vector256<byte> v1 = Vector256.LoadUnsafe(ref src, i + size);
            Vector256<byte> v2 = Vector256.LoadUnsafe(ref src, i + (2 * size));
            Vector256<byte> v3 = Vector256.LoadUnsafe(ref src, i + (3 * size));

            Avx2.Shuffle(v0, mask).StoreUnsafe(ref tar, i);
            Avx2.Shuffle(v1, mask).StoreUnsafe(ref tar, i + size);
            Avx2.Shuffle(v2, mask).StoreUnsafe(ref tar, i + (2 * size));
            Avx2.Shuffle(v3, mask).StoreUnsafe(ref tar, i + (3 * size));
        }

        for (; i < lastOffset; i += size)
            Avx2.Shuffle(Vector256.LoadUnsafe(ref src, i), mask).StoreUnsafe(ref tar, i);

        Avx2.Shuffle(last, mask).StoreUnsafe(ref tar, lastOffset);
    }

    private static void Shuffle4Bytes(ref byte src, ref byte tar, nuint length, Vector128<byte> mask)
    {
        nuint size = (nuint)Vector128<byte>.Count;
        nuint lastOffset = length - size;
        Vector128<byte> last = Vector128.LoadUnsafe(ref src, lastOffset);

        nuint i = 0;
        for (; (i + (4 * size)) <= length; i += 4 * size)
        {
            Vector128<byte> v0 = Vector128.LoadUnsafe(ref src, i);
            Vector128<byte> v1 = Vector128.LoadUnsafe(ref src, i + size);
            Vector128<byte> v2 = Vector128.LoadUnsafe(ref src, i + (2 * size));
            Vector128<byte> v3 = Vector128.LoadUnsafe(ref src, i + (3 * size));

            Vector128.ShuffleNative(v0, mask).StoreUnsafe(ref tar, i);
            Vector128.ShuffleNative(v1, mask).StoreUnsafe(ref tar, i + size);
            Vector128.ShuffleNative(v2, mask).StoreUnsafe(ref tar, i + (2 * size));
            Vector128.ShuffleNative(v3, mask).StoreUnsafe(ref tar, i + (3 * size));
        }

        for (; i < lastOffset; i += size)
            Vector128.ShuffleNative(Vector128.LoadUnsafe(ref src, i), mask).StoreUnsafe(ref tar, i);

        Vector128.ShuffleNative(last, mask).StoreUnsafe(ref tar, lastOffset);
    }

    private static void Shuffle4BytesScalar(ref byte src, ref byte tar, nuint length, ReadOnlySpan<byte> masks)
    {
        nuint map0 = masks[MASK_4_MAP];
        nuint map1 = masks[MASK_4_MAP + 1];
        nuint map2 = masks[MASK_4_MAP + 2];
        nuint map3 = masks[MASK_4_MAP + 3];

        for (nuint i = 0; i < length; i += 4)
        {
            byte b0 = Unsafe.Add(ref src, i + map0);
            byte b1 = Unsafe.Add(ref src, i + map1);
            byte b2 = Unsafe.Add(ref src, i + map2);
            byte b3 = Unsafe.Add(ref src, i + map3);

            Unsafe.Add(ref tar, i) = b0;
            Unsafe.Add(ref tar, i + 1) = b1;
            Unsafe.Add(ref tar, i + 2) = b2;
            Unsafe.Add(ref tar, i + 3) = b3;
        }
    }
    
    private static void ConvertWiden3To4Bytes(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> masks)
    {
        ref byte m = ref MemoryMarshal.GetReference(masks);
        ref byte src = ref MemoryMarshal.GetReference(source);
        ref byte tar = ref MemoryMarshal.GetReference(target);
        nuint sourceLength = (nuint)source.Length;
        nuint pixels = sourceLength / 3;
        nuint p = 0;

        // Every load must stay inside the source, but uses only 3/4 of what it reads.
        if (Vector512.IsHardwareAccelerated && Avx512Vbmi.IsSupported)
        {
            Vector512<byte> mask = Vector512.LoadUnsafe(ref m, MASK_WIDEN);
            Vector512<byte> alpha = Vector512.LoadUnsafe(ref m, MASK_WIDEN_ALPHA);

            for (; ((p * 3) + 64) <= sourceLength; p += 16)
                (Avx512Vbmi.PermuteVar64x8(Vector512.LoadUnsafe(ref src, p * 3), mask) | alpha).StoreUnsafe(ref tar, p * 4);
        }
        else if (Avx2.IsSupported)
        {
            // pshufb stays in its 16-byte lane: first move source bytes 12..23 into the upper lane.
            Vector256<uint> lanes = Vector256.Create(0u, 1, 2, 2, 3, 4, 5, 5);
            Vector128<byte> laneMask = Vector128.LoadUnsafe(ref m, MASK_WIDEN);
            Vector256<byte> mask = Vector256.Create(laneMask, laneMask);
            Vector256<byte> alpha = Vector256.LoadUnsafe(ref m, MASK_WIDEN_ALPHA);

            for (; ((p * 3) + 32) <= sourceLength; p += 8)
            {
                Vector256<byte> vector = Avx2.PermuteVar8x32(Vector256.LoadUnsafe(ref src, p * 3).AsUInt32(), lanes).AsByte();
                (Avx2.Shuffle(vector, mask) | alpha).StoreUnsafe(ref tar, p * 4);
            }
        }

        if (Vector128.IsHardwareAccelerated)
        {
            Vector128<byte> mask = Vector128.LoadUnsafe(ref m, MASK_WIDEN);
            Vector128<byte> alpha = Vector128.LoadUnsafe(ref m, MASK_WIDEN_ALPHA);

            for (; ((p * 3) + 16) <= sourceLength; p += 4)
                (Vector128.ShuffleNative(Vector128.LoadUnsafe(ref src, p * 3), mask) | alpha).StoreUnsafe(ref tar, p * 4);
        }

        nuint map0 = masks[MASK_WIDEN_MAP];
        nuint map1 = masks[MASK_WIDEN_MAP + 1];
        nuint map2 = masks[MASK_WIDEN_MAP + 2];
        nuint map3 = masks[MASK_WIDEN_MAP + 3];

        byte alpha0 = masks[MASK_WIDEN_ALPHA];
        byte alpha1 = masks[MASK_WIDEN_ALPHA + 1];
        byte alpha2 = masks[MASK_WIDEN_ALPHA + 2];
        byte alpha3 = masks[MASK_WIDEN_ALPHA + 3];

        for (; p < pixels; p++)
        {
            ref byte s = ref Unsafe.Add(ref src, p * 3);
            ref byte t = ref Unsafe.Add(ref tar, p * 4);

            t = (byte)(Unsafe.Add(ref s, map0) | alpha0);
            Unsafe.Add(ref t, 1) = (byte)(Unsafe.Add(ref s, map1) | alpha1);
            Unsafe.Add(ref t, 2) = (byte)(Unsafe.Add(ref s, map2) | alpha2);
            Unsafe.Add(ref t, 3) = (byte)(Unsafe.Add(ref s, map3) | alpha3);
        }
    }

    private static void ConvertNarrow4To3Bytes(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> masks)
    {
        ref byte m = ref MemoryMarshal.GetReference(masks);
        ref byte src = ref MemoryMarshal.GetReference(source);
        ref byte tar = ref MemoryMarshal.GetReference(target);
        nuint pixels = (nuint)source.Length / 4;
        nuint targetLength = pixels * 3;
        nuint p = 0;

        // Every store writes a whole vector but only 3/4 of it is valid; the next store overwrites the rest.
        // The stores must stay inside the converted pixels (the target may be longer than that).
        if (Vector512.IsHardwareAccelerated && Avx512Vbmi.IsSupported)
        {
            Vector512<byte> mask = Vector512.LoadUnsafe(ref m, MASK_NARROW_512);

            for (; ((p * 3) + 64) <= targetLength; p += 16)
                Avx512Vbmi.PermuteVar64x8(Vector512.LoadUnsafe(ref src, p * 4), mask).StoreUnsafe(ref tar, p * 3);
        }
        else if (Avx2.IsSupported)
        {
            // pshufb packs each lane into its first 12 bytes, then the upper lane's 12 bytes move down next to them.
            Vector128<byte> laneMask = Vector128.LoadUnsafe(ref m, MASK_NARROW_LANE);
            Vector256<byte> mask = Vector256.Create(laneMask, laneMask);
            Vector256<uint> lanes = Vector256.Create(0u, 1, 2, 4, 5, 6, 7, 7);

            for (; ((p * 3) + 32) <= targetLength; p += 8)
            {
                Vector256<byte> packed = Avx2.Shuffle(Vector256.LoadUnsafe(ref src, p * 4), mask);
                Avx2.PermuteVar8x32(packed.AsUInt32(), lanes).AsByte().StoreUnsafe(ref tar, p * 3);
            }
        }

        if (Vector128.IsHardwareAccelerated)
        {
            Vector128<byte> mask = Vector128.LoadUnsafe(ref m, MASK_NARROW_LANE);

            for (; ((p * 3) + 16) <= targetLength; p += 4)
                Vector128.ShuffleNative(Vector128.LoadUnsafe(ref src, p * 4), mask).StoreUnsafe(ref tar, p * 3);
        }

        nuint map0 = masks[MASK_NARROW_MAP], map1 = masks[MASK_NARROW_MAP + 1], map2 = masks[MASK_NARROW_MAP + 2];
        for (; p < pixels; p++)
        {
            ref byte s = ref Unsafe.Add(ref src, p * 4);
            ref byte t = ref Unsafe.Add(ref tar, p * 3);

            t = Unsafe.Add(ref s, map0);
            Unsafe.Add(ref t, 1) = Unsafe.Add(ref s, map1);
            Unsafe.Add(ref t, 2) = Unsafe.Add(ref s, map2);
        }
    }

    #endregion

    #region Masks

    private static class ConvertMasks<TSource, TTarget>
    where TSource : unmanaged, IColor<TSource>
    where TTarget : unmanaged, IColor<TTarget>
    {
        // ReSharper disable once StaticMemberInGenericType - that's exactly what we want here
        public static readonly byte[] Masks = CreateMasks(TSource.ColorFormat, TTarget.ColorFormat);
    }

    private static byte[] CreateMasks(IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        Span<byte> sourceIndices = stackalloc byte[4];
        ReadOnlySpan<byte> sourceMapping = sourceFormat.ByteMapping;
        for (int i = 0; i < sourceMapping.Length; i++)
            sourceIndices[sourceMapping[i]] = (byte)i;

        ReadOnlySpan<byte> targetMapping = targetFormat.ByteMapping;
        Span<byte> map = stackalloc byte[4];
        Span<bool> isAlpha = stackalloc bool[4];
        for (int i = 0; i < targetMapping.Length; i++)
        {
            bool isAlphaValue = (sourceFormat.BytesPerPixel == 3) && (targetMapping[i] == Color.A);
            isAlpha[i] = isAlphaValue;
            map[i] = isAlphaValue ? (byte)0 : sourceIndices[targetMapping[i]];
        }

        switch (sourceFormat.BytesPerPixel, targetFormat.BytesPerPixel)
        {
            case (3, 3):
                {
                    byte[] masks = new byte[MASK_3_MAP + 3];

                    // Output vector k needs the 18 source bytes starting at window k (the first pixel it touches),
                    // loaded as window..window+W-1 and window+2..window+W+1.
                    for (int k = 0; k < 3; k++)
                    {
                        int window = ((16 * k) / 3) * 3;
                        for (int j = 0; j < 16; j++)
                        {
                            int s = GetSource3((16 * k) + j, map) - window;
                            masks[MASK_3_128 + (32 * k) + j] = s < 16 ? (byte)s : (byte)0x80; // 0x80: pshufb/tbl write 0
                            masks[MASK_3_128 + (32 * k) + 16 + j] = s < 16 ? (byte)0x80 : (byte)(s - 2);
                        }

                        window = ((64 * k) / 3) * 3;
                        for (int j = 0; j < 64; j++)
                        {
                            int s = GetSource3((64 * k) + j, map) - window;
                            masks[MASK_3_512 + (64 * k) + j] = s < 64 ? (byte)s : (byte)(64 + (s - 2)); // 64+: second source
                        }
                    }

                    map[..3].CopyTo(masks.AsSpan(MASK_3_MAP));
                    return masks;
                }

            case (4, 4):
                {
                    byte[] masks = new byte[MASK_4_MAP + 4];

                    // Every index stays inside its pixel, so it never leaves its 16-byte lane: the same lane mask works
                    // for pshufb at every width.
                    for (int j = 0; j < 16; j++)
                        masks[MASK_4_LANE + j] = (byte)((4 * (j / 4)) + map[j % 4]);

                    map.CopyTo(masks.AsSpan(MASK_4_MAP));
                    return masks;
                }

            case (3, 4):
                {
                    byte[] masks = new byte[MASK_WIDEN_MAP + 4];

                    for (int j = 0; j < 64; j++)
                    {
                        masks[MASK_WIDEN + j] = (byte)((3 * (j / 4)) + map[j % 4]);
                        masks[MASK_WIDEN_ALPHA + j] = isAlpha[j % 4] ? byte.MaxValue : (byte)0;
                    }

                    map.CopyTo(masks.AsSpan(MASK_WIDEN_MAP));
                    return masks;
                }

            case (4, 3):
                {
                    byte[] masks = new byte[MASK_NARROW_MAP + 3];

                    // The lane mask packs 4 pixels into the first 12 bytes of a 16-byte lane,
                    // the full mask 16 pixels into the first 48 bytes of a 64-byte vector.
                    for (int j = 0; j < 16; j++)
                        masks[MASK_NARROW_LANE + j] = j < 12 ? (byte)((4 * (j / 3)) + map[j % 3]) : (byte)0x80;
                    for (int j = 0; j < 48; j++)
                        masks[MASK_NARROW_512 + j] = (byte)((4 * (j / 3)) + map[j % 3]);

                    map[..3].CopyTo(masks.AsSpan(MASK_NARROW_MAP));
                    return masks;
                }

            default:
                throw new NotSupportedException("Data is not of a supported valid color-type.");
        }
    }

    /// <summary>Source byte (relative to the block) of target byte <paramref name="targetByte"/> in a 3-byte conversion.</summary>
    private static int GetSource3(int targetByte, ReadOnlySpan<byte> map)
    {
        int pixel = targetByte / 3;
        return (pixel * 3) + map[targetByte - (pixel * 3)];
    }

    #endregion
}
