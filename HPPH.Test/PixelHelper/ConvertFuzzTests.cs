using System.Runtime.InteropServices;

namespace HPPH.Test.PixelHelper;

/// <summary>
/// Random lengths and unaligned start offsets for every conversion pair, out of place and in place.
/// Checks the result against a scalar conversion and that nothing around the target was written.
/// </summary>
[TestClass]
public class ConvertFuzzTests
{
    private const int ITERATIONS = 60;
    private const int MAX_PIXELS = 3000;
    private const int MAX_OFFSET = 64;
    private const byte SENTINEL = 0xA5;

    [TestMethod]
    public void ConvertRandomLengthsAndOffsets()
    {
        Random random = new(4711);

        ConvertFrom<ColorRGB>(random);
        ConvertFrom<ColorBGR>(random);
        ConvertFrom<ColorRGBA>(random);
        ConvertFrom<ColorBGRA>(random);
        ConvertFrom<ColorARGB>(random);
        ConvertFrom<ColorABGR>(random);
    }

    private static void ConvertFrom<TSource>(Random random)
        where TSource : unmanaged, IColor
    {
        ConvertBetween<TSource, ColorRGB>(random);
        ConvertBetween<TSource, ColorBGR>(random);
        ConvertBetween<TSource, ColorRGBA>(random);
        ConvertBetween<TSource, ColorBGRA>(random);
        ConvertBetween<TSource, ColorARGB>(random);
        ConvertBetween<TSource, ColorABGR>(random);
    }

    private static void ConvertBetween<TSource, TTarget>(Random random)
        where TSource : unmanaged, IColor
        where TTarget : unmanaged, IColor
    {
        int sourceBpp = TSource.ColorFormat.BytesPerPixel;
        int targetBpp = TTarget.ColorFormat.BytesPerPixel;

        for (int iteration = 0; iteration < ITERATIONS; iteration++)
        {
            // Small lengths more often, they hit the tails.
            int length = iteration < (ITERATIONS / 2) ? random.Next(0, 100) : random.Next(0, MAX_PIXELS);
            string message = $"{TSource.ColorFormat.Name} -> {TTarget.ColorFormat.Name}, {length} px";

            byte[] sourceBuffer = new byte[(length * sourceBpp) + (2 * MAX_OFFSET)];
            random.NextBytes(sourceBuffer);
            int sourceOffset = random.Next(0, MAX_OFFSET);
            Span<TSource> source = MemoryMarshal.Cast<byte, TSource>(sourceBuffer.AsSpan(sourceOffset, length * sourceBpp));

            TTarget[] expected = ConvertScalar<TSource, TTarget>(source);

            byte[] targetBuffer = new byte[(length * targetBpp) + (2 * MAX_OFFSET)];
            targetBuffer.AsSpan().Fill(SENTINEL);
            int targetOffset = random.Next(0, MAX_OFFSET);
            Span<TTarget> target = MemoryMarshal.Cast<byte, TTarget>(targetBuffer.AsSpan(targetOffset, length * targetBpp));

            byte[] sourceBefore = [.. sourceBuffer];
            ((ReadOnlySpan<TSource>)source).Convert(target);

            AssertEqual(expected, target, message);
            AssertUntouched(targetBuffer, targetOffset, length * targetBpp, message);
            Assert.AreSequenceEqual(sourceBefore, sourceBuffer, $"{message}: source modified");

            if (sourceBpp == targetBpp)
            {
                Span<TTarget> inPlace = source.ConvertInPlace<TSource, TTarget>();

                AssertEqual(expected, inPlace, $"{message} in place");
                AssertUnchanged(sourceBefore, sourceBuffer, sourceOffset, length * sourceBpp, $"{message} in place");
            }
        }
    }

    private static TTarget[] ConvertScalar<TSource, TTarget>(ReadOnlySpan<TSource> source)
        where TSource : unmanaged, IColor
        where TTarget : unmanaged, IColor
    {
        TTarget[] result = new TTarget[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            TSource color = source[i];
            result[i] = (TTarget)TTarget.Create(color.R, color.G, color.B, TSource.ColorFormat.BytesPerPixel == 3 ? byte.MaxValue : color.A);
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

    private static void AssertUntouched(byte[] buffer, int offset, int length, string message)
    {
        int before = buffer.AsSpan(0, offset).IndexOfAnyExcept(SENTINEL);
        if (before >= 0) Assert.Fail($"{message}: wrote {offset - before} bytes before the target.");

        int after = buffer.AsSpan(offset + length).IndexOfAnyExcept(SENTINEL);
        if (after >= 0) Assert.Fail($"{message}: wrote after the target (byte {after} past its end).");
    }

    private static void AssertUnchanged(byte[] original, byte[] buffer, int offset, int length, string message)
    {
        if (!original.AsSpan(0, offset).SequenceEqual(buffer.AsSpan(0, offset)))
            Assert.Fail($"{message}: wrote before the data.");

        if (!original.AsSpan(offset + length).SequenceEqual(buffer.AsSpan(offset + length)))
            Assert.Fail($"{message}: wrote after the data.");
    }
}
