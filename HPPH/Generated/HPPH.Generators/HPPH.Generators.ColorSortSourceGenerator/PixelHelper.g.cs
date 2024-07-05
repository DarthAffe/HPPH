using System.Buffers;

namespace HPPH;

public static unsafe partial class PixelHelper
{
    public static partial void SortByRed<T>(Span<T> colors) where T : unmanaged, IColor
    {
        fixed (T* ptr = colors)
        {
            T* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (T* color = ptr; color < end; color++)
                histogram[(*color).R]++;
    
            T[] bucketsArray = ArrayPool<T>.Shared.Rent(colors.Length);
            try
            {
                Span<T> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (T* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).R]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(bucketsArray);
            }
        }
    }
    public static partial void SortByGreen<T>(Span<T> colors) where T : unmanaged, IColor
    {
        fixed (T* ptr = colors)
        {
            T* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (T* color = ptr; color < end; color++)
                histogram[(*color).G]++;
    
            T[] bucketsArray = ArrayPool<T>.Shared.Rent(colors.Length);
            try
            {
                Span<T> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (T* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).G]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(bucketsArray);
            }
        }
    }
    public static partial void SortByBlue<T>(Span<T> colors) where T : unmanaged, IColor
    {
        fixed (T* ptr = colors)
        {
            T* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (T* color = ptr; color < end; color++)
                histogram[(*color).B]++;
    
            T[] bucketsArray = ArrayPool<T>.Shared.Rent(colors.Length);
            try
            {
                Span<T> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (T* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).B]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(bucketsArray);
            }
        }
    }
    public static partial void SortByAlpha<T>(Span<T> colors) where T : unmanaged, IColor
    {
        fixed (T* ptr = colors)
        {
            T* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (T* color = ptr; color < end; color++)
                histogram[(*color).A]++;
    
            T[] bucketsArray = ArrayPool<T>.Shared.Rent(colors.Length);
            try
            {
                Span<T> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (T* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).A]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<T>.Shared.Return(bucketsArray);
            }
        }
    }
    internal static partial void SortB1(Span<Generic3ByteData> colors) 
    {
        fixed (Generic3ByteData* ptr = colors)
        {
            Generic3ByteData* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (Generic3ByteData* color = ptr; color < end; color++)
                histogram[(*color).B1]++;
    
            Generic3ByteData[] bucketsArray = ArrayPool<Generic3ByteData>.Shared.Rent(colors.Length);
            try
            {
                Span<Generic3ByteData> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (Generic3ByteData* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).B1]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<Generic3ByteData>.Shared.Return(bucketsArray);
            }
        }
    }
    internal static partial void SortB2(Span<Generic3ByteData> colors) 
    {
        fixed (Generic3ByteData* ptr = colors)
        {
            Generic3ByteData* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (Generic3ByteData* color = ptr; color < end; color++)
                histogram[(*color).B2]++;
    
            Generic3ByteData[] bucketsArray = ArrayPool<Generic3ByteData>.Shared.Rent(colors.Length);
            try
            {
                Span<Generic3ByteData> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (Generic3ByteData* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).B2]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<Generic3ByteData>.Shared.Return(bucketsArray);
            }
        }
    }
    internal static partial void SortB3(Span<Generic3ByteData> colors) 
    {
        fixed (Generic3ByteData* ptr = colors)
        {
            Generic3ByteData* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (Generic3ByteData* color = ptr; color < end; color++)
                histogram[(*color).B3]++;
    
            Generic3ByteData[] bucketsArray = ArrayPool<Generic3ByteData>.Shared.Rent(colors.Length);
            try
            {
                Span<Generic3ByteData> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (Generic3ByteData* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).B3]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<Generic3ByteData>.Shared.Return(bucketsArray);
            }
        }
    }
    internal static partial void SortB1(Span<Generic4ByteData> colors) 
    {
        fixed (Generic4ByteData* ptr = colors)
        {
            Generic4ByteData* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (Generic4ByteData* color = ptr; color < end; color++)
                histogram[(*color).B1]++;
    
            Generic4ByteData[] bucketsArray = ArrayPool<Generic4ByteData>.Shared.Rent(colors.Length);
            try
            {
                Span<Generic4ByteData> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (Generic4ByteData* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).B1]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<Generic4ByteData>.Shared.Return(bucketsArray);
            }
        }
    }
    internal static partial void SortB2(Span<Generic4ByteData> colors) 
    {
        fixed (Generic4ByteData* ptr = colors)
        {
            Generic4ByteData* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (Generic4ByteData* color = ptr; color < end; color++)
                histogram[(*color).B2]++;
    
            Generic4ByteData[] bucketsArray = ArrayPool<Generic4ByteData>.Shared.Rent(colors.Length);
            try
            {
                Span<Generic4ByteData> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (Generic4ByteData* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).B2]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<Generic4ByteData>.Shared.Return(bucketsArray);
            }
        }
    }
    internal static partial void SortB3(Span<Generic4ByteData> colors) 
    {
        fixed (Generic4ByteData* ptr = colors)
        {
            Generic4ByteData* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (Generic4ByteData* color = ptr; color < end; color++)
                histogram[(*color).B3]++;
    
            Generic4ByteData[] bucketsArray = ArrayPool<Generic4ByteData>.Shared.Rent(colors.Length);
            try
            {
                Span<Generic4ByteData> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (Generic4ByteData* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).B3]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<Generic4ByteData>.Shared.Return(bucketsArray);
            }
        }
    }
    internal static partial void SortB4(Span<Generic4ByteData> colors) 
    {
        fixed (Generic4ByteData* ptr = colors)
        {
            Generic4ByteData* end = ptr + colors.Length;
    
            Span<int> histogram = stackalloc int[256];
            histogram.Clear();
            for (Generic4ByteData* color = ptr; color < end; color++)
                histogram[(*color).B4]++;
    
            Generic4ByteData[] bucketsArray = ArrayPool<Generic4ByteData>.Shared.Rent(colors.Length);
            try
            {
                Span<Generic4ByteData> buckets = bucketsArray.AsSpan()[..colors.Length];
                Span<int> currentBucketIndex = stackalloc int[256];
    
                int offset = 0;
                for (int i = 0; i < histogram.Length; i++)
                {
                    currentBucketIndex[i] = offset;
                    offset += histogram[i];
                }
    
                for (Generic4ByteData* color = ptr; color < end; color++)
                    buckets[currentBucketIndex[(*color).B4]++] = (*color);
    
                buckets.CopyTo(colors);
            }
            finally
            {
                ArrayPool<Generic4ByteData>.Shared.Return(bucketsArray);
            }
        }
    }

}