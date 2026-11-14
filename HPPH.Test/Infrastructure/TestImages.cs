using System.Runtime.InteropServices;

namespace HPPH.Test.Infrastructure;

/// <summary>
/// Test images in the memory layouts operations have to handle, and checks for their results.
/// </summary>
internal static class TestImages
{
    #region Constants

    public const byte SENTINEL = 0xA5;

    public static readonly (int Width, int Height)[] SIZES = [(1, 1), (1, 9), (9, 1), (2, 2), (3, 5), (7, 13), (31, 17), (64, 36)];

    #endregion

    #region Methods

    public static IEnumerable<(string Name, Image<T> Image)> Variants<T>(int width, int height, int seed)
        where T : unmanaged, IColor<T>
    {
        Random random = new(seed);

        yield return ("contiguous", Create<T>(random, width, height, 0));
        yield return ("padded", Create<T>(random, width, height, 13));
        yield return ("region", Create<T>(random, width + 10, height + 8, 7).Slice(3, 5, width, height));
    }

    public static Image<T> Create<T>(Random random, int width, int height, int padding)
        where T : unmanaged, IColor<T>
    {
        int rowSize = width * T.ColorFormat.BytesPerPixel;
        int stride = rowSize + padding;

        byte[] buffer = new byte[height == 0 ? 0 : ((height - 1) * stride) + rowSize];
        random.NextBytes(buffer);

        return Image<T>.Wrap(buffer, width, height, stride);
    }

    public static byte[] CreateSentinelBuffer(int length)
    {
        byte[] buffer = new byte[length];
        buffer.AsSpan().Fill(SENTINEL);
        return buffer;
    }

    public static void AssertEqual<T>(Image<T> expected, Image<T> actual, string message)
        where T : unmanaged, IColor<T>
        => AssertEqual(expected, actual.AsRefImage(), message);

    /// <inheritdoc cref="AssertEqual{T}(Image{T}, Image{T}, string)"/>
    public static void AssertEqual<T>(Image<T> expected, RefImage<T> actual, string message)
        where T : unmanaged, IColor<T>
    {
        Assert.AreEqual(expected.Width, actual.Width, $"{message}: width");
        Assert.AreEqual(expected.Height, actual.Height, $"{message}: height");

        if (expected.ContentEquals(actual)) return;

        for (int y = 0; y < expected.Height; y++)
            for (int x = 0; x < expected.Width; x++)
                if (!expected[x, y].Equals(actual[x, y]))
                    Assert.Fail($"{message}: first difference at ({x}, {y}), expected {expected[x, y]}, got {actual[x, y]}.");
    }

    public static void AssertStrided<T>(Image<T> expected, byte[] destination, int stride, string message)
        where T : unmanaged, IColor<T>
    {
        int rowSize = expected.Width * T.ColorFormat.BytesPerPixel;
        int written = expected.Height == 0 ? 0 : ((expected.Height - 1) * stride) + rowSize;

        if (expected.Height > 0)
            AssertEqual(expected, RefImage<T>.Wrap(destination, expected.Width, expected.Height, stride), message);

        for (int y = 0; y < (expected.Height - 1); y++)
            AssertUntouched(destination.AsSpan((y * stride) + rowSize, stride - rowSize), $"{message}: padding after row {y}");

        AssertUntouched(destination.AsSpan(written), $"{message}: after the last row");
    }

    public static void AssertCompact<T>(Image<T> expected, Span<T> destination, string message)
        where T : unmanaged, IColor<T>
    {
        int pixels = expected.Width * expected.Height;

        AssertEqual(expected, RefImage<T>.Wrap(destination[..pixels], expected.Width, expected.Height), message);
        AssertUntouched(MemoryMarshal.AsBytes(destination[pixels..]), $"{message}: after the last pixel");
    }

    public static T[] CreateSentinelPixels<T>(int pixels)
        where T : unmanaged, IColor<T>
    {
        T[] destination = new T[pixels + 3];
        MemoryMarshal.AsBytes(destination.AsSpan()).Fill(SENTINEL);
        return destination;
    }

    private static void AssertUntouched(ReadOnlySpan<byte> bytes, string message)
    {
        int index = bytes.IndexOfAnyExcept(SENTINEL);
        if (index >= 0) Assert.Fail($"{message}: byte {index} was written.");
    }

    #endregion
}
