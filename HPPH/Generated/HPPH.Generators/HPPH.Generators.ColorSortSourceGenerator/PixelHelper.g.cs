#nullable enable

using System.Buffers;

namespace HPPH;

public static unsafe partial class PixelHelper
{
    public static partial void SortByRed<T>(this Span<T> colors) where T : unmanaged, IColor
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
    public static partial void SortByGreen<T>(this Span<T> colors) where T : unmanaged, IColor
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
    public static partial void SortByBlue<T>(this Span<T> colors) where T : unmanaged, IColor
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
    public static partial void SortByAlpha<T>(this Span<T> colors) where T : unmanaged, IColor
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

}