using System.Runtime.InteropServices;
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
    public void ConvertInPlaceStaysInBounds()
    {
        ConvertInPlaceStaysInBounds<ColorRGB, ColorBGR>();
        ConvertInPlaceStaysInBounds<ColorRGBA, ColorABGR>();
        ConvertInPlaceStaysInBounds<ColorARGB, ColorBGRA>();
    }

    private static void ConvertInPlaceStaysInBounds<TSource, TTarget>()
        where TSource : struct, IColor
        where TTarget : struct, IColor
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
        where TSource : struct, IColor
        where TTarget : struct, IColor
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
        where T : struct, IColor
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
        where TSource : struct, IColor
        where TTarget : struct, IColor
    {
        TTarget[] result = new TTarget[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            TSource color = source[i];
            result[i] = (TTarget)TTarget.Create(color.R, color.G, color.B, color.A);
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
