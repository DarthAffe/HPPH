using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HPPH;

internal static unsafe class WuQuantizer
{
    #region Constants

    private const int INDEX_BITS = 5;
    private const int SHIFT = 8 - INDEX_BITS;

    private const int SIDE = (1 << INDEX_BITS) + 1;
    private const int SIDE_SQUARED = SIDE * SIDE;
    private const int TABLE_SIZE = SIDE * SIDE * SIDE;

    private const int CELL_VALUES = 6;

    private const int MAX_BOXES = 1 << (3 * INDEX_BITS);
    private const int MAX_STACKALLOC_BOXES = 256;

    private const int MIN_PARALLEL_HISTOGRAM_SIZE = 1024 * 1024;
    private const int MIN_PARALLEL_CHUNK_SIZE = 256 * 1024;

    #endregion

    #region Methods

    internal static T[] CreateColorPalette<T>(byte* data, nuint rowPixels, nuint stride, nuint rows, int paletteSize)
        where T : unmanaged, IColor
    {
        WuMoment[] moments = ArrayPool<WuMoment>.Shared.Rent(TABLE_SIZE);
        try
        {
            BuildHistogram<T>(data, rowPixels, stride, rows, moments);
            ComputeCumulativeMoments(moments);

            return BuildPalette<T>(moments, paletteSize);
        }
        finally
        {
            ArrayPool<WuMoment>.Shared.Return(moments);
        }
    }

    private static void BuildHistogram<T>(byte* data, nuint rowPixels, nuint stride, nuint rows, WuMoment[] moments)
        where T : unmanaged, IColor
    {
        nuint pixels = rowPixels * rows;

        // Contiguous data is split by pixels, everything else by rows.
        nuint parts = rows == 1 ? pixels : rows;
        int chunks = (int)Math.Min(Math.Min((nuint)Environment.ProcessorCount, pixels / MIN_PARALLEL_CHUNK_SIZE), parts);

        if ((pixels < MIN_PARALLEL_HISTOGRAM_SIZE) || (chunks < 2))
            AddColors<T>(data, rowPixels, stride, rows, moments);
        else
        {
            WuMoment[][] chunkMoments = new WuMoment[chunks][];
            chunkMoments[0] = moments;
            try
            {
                for (int i = 1; i < chunks; i++)
                    chunkMoments[i] = ArrayPool<WuMoment>.Shared.Rent(TABLE_SIZE);

                nint dataAddress = (nint)data;
                Parallel.For(0, chunks, chunk =>
                {
                    nuint start = (parts * (nuint)chunk) / (nuint)chunks;
                    nuint end = (parts * ((nuint)chunk + 1)) / (nuint)chunks;

                    if (rows == 1)
                        AddColors<T>((byte*)dataAddress + (start * (nuint)sizeof(T)), end - start, stride, 1, chunkMoments[chunk]);
                    else
                        AddColors<T>((byte*)dataAddress + (start * stride), rowPixels, stride, end - start, chunkMoments[chunk]);
                });

                for (int i = 1; i < chunks; i++)
                    Add(moments, chunkMoments[i]);
            }
            finally
            {
                for (int i = 1; i < chunks; i++)
                    if (chunkMoments[i] != null)
                        ArrayPool<WuMoment>.Shared.Return(chunkMoments[i]);
            }
        }
    }

    private static void AddColors<T>(byte* data, nuint rowPixels, nuint stride, nuint rows, WuMoment[] moments)
        where T : unmanaged, IColor
    {
        moments.AsSpan(0, TABLE_SIZE).Clear();
        ref WuMoment table = ref MemoryMarshal.GetArrayDataReference(moments);

        for (nuint row = 0; row < rows; row++)
        {
            T* color = (T*)(data + (row * stride));
            T* end = color + rowPixels;

            for (; color < end; color++)
            {
                int r = (*color).R;
                int g = (*color).G;
                int b = (*color).B;

                ref WuMoment moment = ref Unsafe.Add(ref table, (((r >> SHIFT) + 1) * SIDE_SQUARED) + (((g >> SHIFT) + 1) * SIDE) + ((b >> SHIFT) + 1));
                moment.Weight++;
                moment.R += r;
                moment.G += g;
                moment.B += b;
                moment.A += (*color).A;
                moment.Squares += (r * r) + (g * g) + (b * b);
            }
        }
    }

    private static void Add(WuMoment[] target, WuMoment[] source)
    {
        ref long targetValues = ref Unsafe.As<WuMoment, long>(ref MemoryMarshal.GetArrayDataReference(target));
        ref long sourceValues = ref Unsafe.As<WuMoment, long>(ref MemoryMarshal.GetArrayDataReference(source));
        const nuint length = TABLE_SIZE * CELL_VALUES;

        nuint i = 0;
        if (Vector256.IsHardwareAccelerated)
            for (; (i + (nuint)Vector256<long>.Count) <= length; i += (nuint)Vector256<long>.Count)
                (Vector256.LoadUnsafe(ref targetValues, i) + Vector256.LoadUnsafe(ref sourceValues, i)).StoreUnsafe(ref targetValues, i);

        if (Vector128.IsHardwareAccelerated)
            for (; (i + (nuint)Vector128<long>.Count) <= length; i += (nuint)Vector128<long>.Count)
                (Vector128.LoadUnsafe(ref targetValues, i) + Vector128.LoadUnsafe(ref sourceValues, i)).StoreUnsafe(ref targetValues, i);

        for (; i < length; i++)
            Unsafe.Add(ref targetValues, i) += Unsafe.Add(ref sourceValues, i);
    }

    private static void ComputeCumulativeMoments(WuMoment[] moments)
    {
        ref long table = ref Unsafe.As<WuMoment, long>(ref MemoryMarshal.GetArrayDataReference(moments));

        const nuint CELL = CELL_VALUES;
        const nuint ROW = SIDE * CELL;
        const nuint PLANE = SIDE_SQUARED * CELL;

        Span<long> areaValues = stackalloc long[(int)ROW];
        ref long area = ref MemoryMarshal.GetReference(areaValues);

        for (nuint r = 1; r < SIDE; r++)
        {
            areaValues.Clear();
            ref long plane = ref Unsafe.Add(ref table, r * PLANE);

            for (nuint g = 1; g < SIDE; g++)
            {
                ref long row = ref Unsafe.Add(ref plane, g * ROW);

                Vector256<long> line0 = Vector256<long>.Zero;
                Vector128<long> line1 = Vector128<long>.Zero;
                for (nuint b = 1; b < SIDE; b++)
                {
                    nuint offset = b * CELL;
                    ref long cell = ref Unsafe.Add(ref row, offset);
                    ref long areaCell = ref Unsafe.Add(ref area, offset);
                    ref long previous = ref Unsafe.Subtract(ref cell, PLANE);

                    line0 += Vector256.LoadUnsafe(ref cell);
                    line1 += Vector128.LoadUnsafe(ref cell, 4);

                    Vector256<long> area0 = Vector256.LoadUnsafe(ref areaCell) + line0;
                    Vector128<long> area1 = Vector128.LoadUnsafe(ref areaCell, 4) + line1;
                    area0.StoreUnsafe(ref areaCell);
                    area1.StoreUnsafe(ref areaCell, 4);

                    (Vector256.LoadUnsafe(ref previous) + area0).StoreUnsafe(ref cell);
                    (Vector128.LoadUnsafe(ref previous, 4) + area1).StoreUnsafe(ref cell, 4);
                }
            }
        }
    }

    private static T[] BuildPalette<T>(WuMoment[] moments, int paletteSize)
        where T : unmanaged, IColor
    {
        int capacity = Math.Min(paletteSize, MAX_BOXES);

        Span<WuBox> boxes = capacity <= MAX_STACKALLOC_BOXES ? stackalloc WuBox[MAX_STACKALLOC_BOXES] : new WuBox[capacity];
        boxes[0] = new WuBox { R1 = SIDE - 1, G1 = SIDE - 1, B1 = SIDE - 1 };

        int count = SplitBoxes(moments, boxes, capacity);

        T[] result = new T[count];
        for (int i = 0; i < count; i++)
        {
            WuMoment moment = GetVolume(moments, boxes[i]);
            result[i] = (T)T.Create(Average(moment.R, moment.Weight),
                                    Average(moment.G, moment.Weight),
                                    Average(moment.B, moment.Weight),
                                    Average(moment.A, moment.Weight));
        }

        return result;
    }

    private static int SplitBoxes(WuMoment[] moments, Span<WuBox> boxes, int capacity)
    {
        Span<double> variances = stackalloc double[MAX_STACKALLOC_BOXES];
        variances[0] = 0;

        int count = 1;
        int next = 0;
        while (count < capacity)
        {
            if (TryCut(moments, ref boxes[next], out WuBox upper))
            {
                boxes[count] = upper;
                variances[next] = GetVariance(moments, boxes[next]);
                variances[count] = GetVariance(moments, upper);
                count++;
            }
            else
                variances[next] = 0;

            next = 0;
            double maxVariance = variances[0];
            for (int i = 1; i < count; i++)
                if (variances[i] > maxVariance)
                {
                    maxVariance = variances[i];
                    next = i;
                }

            if (maxVariance <= 0) break;
        }

        return count;
    }

    private static bool TryCut(WuMoment[] moments, ref WuBox box, out WuBox upper)
    {
        WuMoment whole = GetVolume(moments, box);

        int axis = 0;
        double max = Maximize(moments, box, 0, whole, out int cut);

        double maxG = Maximize(moments, box, 1, whole, out int cutG);
        if (maxG > max)
        {
            axis = 1;
            max = maxG;
            cut = cutG;
        }

        double maxB = Maximize(moments, box, 2, whole, out int cutB);
        if (maxB > max)
        {
            axis = 2;
            cut = cutB;
        }

        if (cut < 0)
        {
            upper = default;
            return false;
        }

        upper = box.WithLower(axis, cut);
        box = box.WithUpper(axis, cut);
        return true;
    }

    private static double Maximize(WuMoment[] moments, in WuBox box, int axis, in WuMoment whole, out int cut)
    {
        cut = -1;
        double max = -1;

        ref long table = ref Unsafe.As<WuMoment, long>(ref MemoryMarshal.GetArrayDataReference(moments));

        // Offsets of the 4 corners of a plane, made up of the other two axes.
        (int stride, int u0, int u1, int v0, int v1) = axis switch
        {
            0 => (SIDE_SQUARED, box.G0 * SIDE, box.G1 * SIDE, box.B0, box.B1),
            1 => (SIDE, box.R0 * SIDE_SQUARED, box.R1 * SIDE_SQUARED, box.B0, box.B1),
            _ => (1, box.R0 * SIDE_SQUARED, box.R1 * SIDE_SQUARED, box.G0 * SIDE, box.G1 * SIDE)
        };

        Vector256<long> wholeValues = Vector256.Create(whole.Weight, whole.R, whole.G, whole.B);
        Vector256<long> bottom = GetPlane(ref table, box.GetLower(axis) * stride, u0, u1, v0, v1);

        int upper = box.GetUpper(axis);
        for (int position = box.GetLower(axis) + 1; position < upper; position++)
        {
            Vector256<long> half = GetPlane(ref table, position * stride, u0, u1, v0, v1) - bottom;
            if (half.GetElement(0) == 0) continue;

            Vector256<long> other = wholeValues - half;
            if (other.GetElement(0) == 0) break; // The upper half only gets smaller from here on.

            double score = GetScore(half) + GetScore(other);
            if (score > max)
            {
                max = score;
                cut = position;
            }
        }

        return max;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<long> GetPlane(ref long table, int plane, int u0, int u1, int v0, int v1)
        => Vector256.LoadUnsafe(ref table, (nuint)((plane + u1 + v1) * CELL_VALUES))
         - Vector256.LoadUnsafe(ref table, (nuint)((plane + u1 + v0) * CELL_VALUES))
         - Vector256.LoadUnsafe(ref table, (nuint)((plane + u0 + v1) * CELL_VALUES))
         + Vector256.LoadUnsafe(ref table, (nuint)((plane + u0 + v0) * CELL_VALUES));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double GetScore(Vector256<long> moment)
    {
        double r = moment.GetElement(1);
        double g = moment.GetElement(2);
        double b = moment.GetElement(3);

        return ((r * r) + (g * g) + (b * b)) / moment.GetElement(0);
    }

    private static double GetVariance(WuMoment[] moments, in WuBox box)
    {
        if (box.Volume <= 1) return 0;

        WuMoment moment = GetVolume(moments, box);
        if (moment.Weight == 0) return 0;

        Int128 numerator = ((Int128)moment.Squares * moment.Weight)
                         - ((Int128)moment.R * moment.R)
                         - ((Int128)moment.G * moment.G)
                         - ((Int128)moment.B * moment.B);

        return (double)numerator / moment.Weight;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static WuMoment GetVolume(WuMoment[] moments, in WuBox box)
    {
        ref WuMoment table = ref MemoryMarshal.GetArrayDataReference(moments);

        int r1 = box.R1 * SIDE_SQUARED;
        int r0 = box.R0 * SIDE_SQUARED;
        int g1 = box.G1 * SIDE;
        int g0 = box.G0 * SIDE;

        return Unsafe.Add(ref table, r1 + g1 + box.B1)
             - Unsafe.Add(ref table, r1 + g1 + box.B0)
             - Unsafe.Add(ref table, r1 + g0 + box.B1)
             + Unsafe.Add(ref table, r1 + g0 + box.B0)
             - Unsafe.Add(ref table, r0 + g1 + box.B1)
             + Unsafe.Add(ref table, r0 + g1 + box.B0)
             + Unsafe.Add(ref table, r0 + g0 + box.B1)
             - Unsafe.Add(ref table, r0 + g0 + box.B0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte Average(long sum, long weight) => (byte)MathF.Round(sum / (float)weight);

    #endregion
}
