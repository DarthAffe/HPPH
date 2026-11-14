namespace HPPH.Test.Validation;

[TestClass]
public class ValidationTests
{
    [TestMethod]
    public void StrideSmallerThanRowInBytesIsRejected()
    {
        // 10 RGB pixels need 30 bytes per row.
        Assert.ThrowsExactly<ArgumentException>(() => Image<ColorRGB>.Wrap(new byte[100], 10, 10, 10));
        Assert.ThrowsExactly<ArgumentException>(() => Image<ColorRGB>.Create(new byte[100], 10, 10, 10));
        Assert.ThrowsExactly<ArgumentException>(() => RefImage<ColorRGB>.Wrap(new byte[100], 10, 10, 10));
    }

    [TestMethod]
    public void NegativeSizeIsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Image<ColorRGB>.Wrap(new byte[100], -1, 1, 30));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Image<ColorRGB>.Wrap(new byte[100], 1, -1, 30));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => RefImage<ColorRGB>.Wrap(new byte[100], -1, 1, 30));
    }

    [TestMethod]
    public void LastRowNeedsNoPadding()
    {
        // 4x3 RGBA with a stride of 20 bytes: 2 * 20 + 16 = 56 bytes are enough.
        byte[] buffer = new byte[56];
        for (int i = 0; i < buffer.Length; i++) buffer[i] = (byte)i;

        Image<ColorRGBA> image = Image<ColorRGBA>.Wrap(buffer, 4, 3, 20);
        Assert.AreEqual(new ColorRGBA(52, 53, 54, 55), image[3, 2]);

        RefImage<ColorRGBA> refImage = RefImage<ColorRGBA>.Wrap(buffer, 4, 3, 20);
        Assert.AreEqual(new ColorRGBA(52, 53, 54, 55), refImage[3, 2]);

        Assert.ThrowsExactly<ArgumentException>(() => Image<ColorRGBA>.Wrap(new byte[55], 4, 3, 20));
    }

    [TestMethod]
    public void EmptySpansAreAccepted()
    {
        Assert.AreEqual(default, Span<ColorRGB>.Empty.Average());
        Assert.AreEqual(default, ReadOnlySpan<ColorRGBA>.Empty.Average());
        Assert.AreEqual(default, Array.Empty<ColorRGB>().AsSpan().Average());

        Assert.IsEmpty(ReadOnlySpan<ColorRGB>.Empty.Convert<ColorRGB, ColorBGR>());
        Assert.HasCount(0, Span<ColorRGBA>.Empty.ConvertInPlace<ColorRGBA, ColorBGRA>());
        ReadOnlySpan<ColorRGB>.Empty.Convert(Span<ColorBGRA>.Empty);

        _ = ReadOnlySpan<ColorRGB>.Empty.MinMax();

        Image<ColorRGB> empty = Image<ColorRGB>.Create(ReadOnlySpan<ColorRGB>.Empty, 0, 0);
        empty.CopyTo(Span<byte>.Empty);
        empty.CopyTo(Span<ColorRGB>.Empty);
    }
}
