using System.Runtime.InteropServices;
using HPPH.Reference;
using HPPH.Test.Infrastructure;

namespace HPPH.Test.PixelHelper;

/// <summary>
/// Runs the SIMD entry points on data that touches an inaccessible page at its end or its start,
/// so any access outside the data crashes the test run.
/// </summary>
[TestClass]
public class GuardPageTests
{
    // Every length up to a few vectors, plus lengths around common image widths.
    private static readonly int[] LENGTHS = [.. Enumerable.Range(0, 131), 255, 256, 257, 1279, 1280, 1281, 1919, 1920, 1921, 3840, 4095, 4096, 4097];

    [TestMethod]
    public void SumStaysInBounds()
    {
        SumStaysInBounds<ColorRGB>();
        SumStaysInBounds<ColorBGRA>();
    }

    private static void SumStaysInBounds<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((int length, GuardedBuffer buffer) in Buffers<T>())
            using (buffer)
            {
                T[] copy = buffer.As<T>().ToArray();
                Assert.AreEqual(ReferencePixelHelper.Sum(new ReadOnlySpan<T>(copy)), ((ReadOnlySpan<T>)buffer.As<T>()).Sum(), $"{T.ColorFormat.Name}, {length} px");
            }
    }

    [TestMethod]
    public void MinMaxStaysInBounds()
    {
        MinMaxStaysInBounds<ColorRGB>();
        MinMaxStaysInBounds<ColorBGRA>();
    }

    private static void MinMaxStaysInBounds<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((int length, GuardedBuffer buffer) in Buffers<T>())
            using (buffer)
            {
                if (length == 0) continue;

                T[] copy = buffer.As<T>().ToArray();
                Assert.AreEqual(ReferencePixelHelper.MinMax(new ReadOnlySpan<T>(copy)), ((ReadOnlySpan<T>)buffer.As<T>()).MinMax(), $"{T.ColorFormat.Name}, {length} px");
            }
    }

    [TestMethod]
    public void AverageStaysInBounds()
    {
        AverageStaysInBounds<ColorRGB>();
        AverageStaysInBounds<ColorBGRA>();
    }

    private static void AverageStaysInBounds<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((int length, GuardedBuffer buffer) in Buffers<T>())
            using (buffer)
            {
                T[] copy = buffer.As<T>().ToArray();
                Assert.AreEqual(ReferencePixelHelper.Average(new ReadOnlySpan<T>(copy)), ((ReadOnlySpan<T>)buffer.As<T>()).Average(), $"{T.ColorFormat.Name}, {length} px");
            }
    }

    [TestMethod]
    public void ImageReductionsStayInBounds()
    {
        ImageReductionsStayInBounds<ColorRGB>();
        ImageReductionsStayInBounds<ColorBGRA>();
    }

    private static void ImageReductionsStayInBounds<T>()
        where T : unmanaged, IColor<T>
    {
        int bpp = T.ColorFormat.BytesPerPixel;

        foreach (int width in (int[])[1, 2, 3, 7, 10, 16, 21, 32, 40, 64])
            foreach (int height in (int[])[1, 2, 5])
                foreach (int padding in (int[])[0, 5])
                {
                    int stride = (width * bpp) + padding;
                    int length = ((height - 1) * stride) + (width * bpp);

                    using GuardedBuffer buffer = GuardedBuffer.AtEnd(length);
                    buffer.Fill(width * height);

                    RefImage<T> image = RefImage<T>.Wrap(buffer.Bytes, width, height, stride);
                    string message = $"{T.ColorFormat.Name} {width}x{height}, stride {stride}";

                    Assert.AreEqual(ReferencePixelHelper.Sum(image), image.Sum(), message);
                    Assert.AreEqual(ReferencePixelHelper.MinMax(image), image.MinMax(), message);
                    Assert.AreEqual(ReferencePixelHelper.Average(image), image.Average(), message);
                }
    }

    [TestMethod]
    public void FlipStaysInBounds()
    {
        FlipStaysInBounds<ColorRGB>();
        FlipStaysInBounds<ColorBGRA>();
    }

    private static void FlipStaysInBounds<T>()
        where T : unmanaged, IColor<T>
    {
        int bpp = T.ColorFormat.BytesPerPixel;

        // 3-byte rows use vectors of 5 and 21 pixels, which need 6 and 22 pixels to start. 4-byte rows use vectors of 4, 8 and 16 pixels.
        foreach (int width in (int[])[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 15, 16, 17, 21, 22, 23, 32, 40, 43, 64])
            foreach (int height in (int[])[1, 2, 5])
                foreach (int padding in (int[])[0, 5])
                    foreach (bool sourceAtEnd in (bool[])[true, false])
                        foreach (ImageFlip flip in (ImageFlip[])[ImageFlip.None, ImageFlip.Horizontal, ImageFlip.Vertical, ImageFlip.Horizontal | ImageFlip.Vertical])
                        {
                            int stride = (width * bpp) + padding;
                            int length = ((height - 1) * stride) + (width * bpp);

                            // Source and destination sit against opposite guard pages.
                            using GuardedBuffer source = sourceAtEnd ? GuardedBuffer.AtEnd(length) : GuardedBuffer.AtStart(length);
                            using GuardedBuffer destination = sourceAtEnd ? GuardedBuffer.AtStart(length) : GuardedBuffer.AtEnd(length);
                            source.Fill(width * height);

                            RefImage<T> image = RefImage<T>.Wrap(source.Bytes, width, height, stride);
                            Image<T> expected = ReferencePixelHelper.Flip(image, flip);

                            image.Flip(flip, destination.Bytes, stride);

                            Assert.IsTrue(expected.ContentEquals(RefImage<T>.Wrap(destination.Bytes, width, height, stride)),
                                          $"{T.ColorFormat.Name} {width}x{height}, stride {stride}, {flip}, source at {(sourceAtEnd ? "end" : "start")}");
                        }
    }

    [TestMethod]
    public void ConvertInPlaceStaysInBounds()
    {
        ConvertInPlaceStaysInBounds<ColorRGB, ColorBGR>();
        ConvertInPlaceStaysInBounds<ColorRGBA, ColorABGR>();
        ConvertInPlaceStaysInBounds<ColorARGB, ColorBGRA>();
    }

    private static void ConvertInPlaceStaysInBounds<TSource, TTarget>()
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        foreach ((int length, GuardedBuffer buffer) in Buffers<TSource>())
            using (buffer)
            {
                TTarget[] expected = ConvertScalar<TSource, TTarget>(buffer.As<TSource>());
                buffer.As<TSource>().ConvertInPlace<TSource, TTarget>();

                AssertEqual(expected, buffer.As<TTarget>(), $"{TSource.ColorFormat.Name} -> {TTarget.ColorFormat.Name} in place, {length} px");
            }
    }

    [TestMethod]
    public void ConvertStaysInBounds()
    {
        ConvertStaysInBounds<ColorRGB, ColorBGR>();
        ConvertStaysInBounds<ColorRGBA, ColorBGRA>();
        ConvertStaysInBounds<ColorRGB, ColorBGRA>();  // widen, alpha last
        ConvertStaysInBounds<ColorBGR, ColorARGB>();  // widen, alpha first
        ConvertStaysInBounds<ColorRGBA, ColorRGB>();  // narrow
        ConvertStaysInBounds<ColorARGB, ColorBGR>();  // narrow, alpha first
    }

    private static void ConvertStaysInBounds<TSource, TTarget>()
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        int targetBpp = TTarget.ColorFormat.BytesPerPixel;

        foreach ((int length, GuardedBuffer source) in Buffers<TSource>())
            using (source)
            {
                bool atEnd = (length % 2) == 0; // Alternate where the target touches its guard page.
                using GuardedBuffer target = atEnd ? GuardedBuffer.AtEnd(length * targetBpp) : GuardedBuffer.AtStart(length * targetBpp);

                TTarget[] expected = ConvertScalar<TSource, TTarget>(source.As<TSource>());
                ((ReadOnlySpan<TSource>)source.As<TSource>()).Convert(target.As<TTarget>());

                AssertEqual(expected, target.As<TTarget>(), $"{TSource.ColorFormat.Name} -> {TTarget.ColorFormat.Name}, {length} px");
            }
    }

    private static IEnumerable<(int Length, GuardedBuffer Buffer)> Buffers<T>()
        where T : unmanaged, IColor<T>
    {
        int bpp = T.ColorFormat.BytesPerPixel;

        foreach (int length in LENGTHS)
        {
            GuardedBuffer atEnd = GuardedBuffer.AtEnd(length * bpp);
            atEnd.Fill(length);
            yield return (length, atEnd);

            GuardedBuffer atStart = GuardedBuffer.AtStart(length * bpp);
            atStart.Fill(length);
            yield return (length, atStart);
        }
    }

    private static TTarget[] ConvertScalar<TSource, TTarget>(ReadOnlySpan<TSource> source)
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        TTarget[] result = new TTarget[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            TSource color = source[i];
            result[i] = TTarget.Create(color.R, color.G, color.B, color.A);
        }

        return result;
    }

    private static void AssertEqual<T>(T[] expected, ReadOnlySpan<T> actual, string message)
        where T : struct
    {
        ReadOnlySpan<byte> expectedBytes = MemoryMarshal.AsBytes(expected.AsSpan());
        ReadOnlySpan<byte> actualBytes = MemoryMarshal.AsBytes(actual);

        if (!expectedBytes.SequenceEqual(actualBytes))
            Assert.Fail($"{message}: first difference at byte {expectedBytes.CommonPrefixLength(actualBytes)}.");
    }
}
