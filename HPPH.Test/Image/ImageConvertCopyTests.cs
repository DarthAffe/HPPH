using System.Runtime.InteropServices;

namespace HPPH.Test.Image;

[TestClass]
public class ImageConvertCopyTests
{
    private const int WIDTH = 37;
    private const int HEIGHT = 23;
    private const int PADDING = 13;
    private const byte SENTINEL = 0xA5;

    [TestMethod]
    public void ConvertToSpan()
    {
        ConvertToSpan<ColorRGBA, ColorBGRA>();
        ConvertToSpan<ColorRGBA, ColorRGBA>();
        ConvertToSpan<ColorRGB, ColorABGR>();
        ConvertToSpan<ColorARGB, ColorBGR>();
        ConvertToSpan<ColorBGR, ColorRGB>();
    }

    private static void ConvertToSpan<TSource, TTarget>()
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        foreach ((string name, IImage<TSource> image) in Images<TSource>())
        {
            TTarget[] expected = Expected<TSource, TTarget>(image);

            TTarget[] destination = new TTarget[expected.Length + 3];
            image.ConvertTo(destination.AsSpan());
            Assert.AreSequenceEqual(expected, destination[..expected.Length], $"{name}: IImage.ConvertTo(Span)");

            TTarget[] fromRef = new TTarget[expected.Length];
            image.AsRefImage().ConvertTo(fromRef.AsSpan());
            Assert.AreSequenceEqual(expected, fromRef, $"{name}: RefImage.ConvertTo(Span)");

            IImage<TTarget> converted = image.ConvertTo<TTarget>();
            Assert.AreEqual(image.Width, converted.Width);
            Assert.AreEqual(image.Height, converted.Height);
            Assert.AreSequenceEqual(expected, converted.ToArray(), $"{name}: ConvertTo()");

            Image<TTarget> convertedRef = image.AsRefImage().ConvertTo<TTarget>();
            Assert.AreEqual(image.Width, convertedRef.Width);
            Assert.AreEqual(image.Height, convertedRef.Height);
            Assert.AreSequenceEqual(expected, convertedRef.ToArray(), $"{name}: RefImage.ConvertTo()");
        }
    }

    [TestMethod]
    public void ConvertToStridedBuffer()
    {
        ConvertToStridedBuffer<ColorRGBA, ColorBGRA>();
        ConvertToStridedBuffer<ColorRGB, ColorBGRA>();
        ConvertToStridedBuffer<ColorBGRA, ColorRGB>();
    }

    private static void ConvertToStridedBuffer<TSource, TTarget>()
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        int targetBpp = TTarget.ColorFormat.BytesPerPixel;

        foreach ((string name, IImage<TSource> image) in Images<TSource>())
        {
            TTarget[] expected = Expected<TSource, TTarget>(image);
            int rowSize = image.Width * targetBpp;

            foreach (int padding in new[] { 0, 5, 64 })
            {
                int stride = rowSize + padding;
                byte[] destination = new byte[((image.Height - 1) * stride) + rowSize + 8];
                destination.AsSpan().Fill(SENTINEL);

                image.ConvertTo<TTarget>(destination, stride);

                for (int y = 0; y < image.Height; y++)
                {
                    ReadOnlySpan<TTarget> row = MemoryMarshal.Cast<byte, TTarget>(destination.AsSpan(y * stride, rowSize));
                    Assert.AreSequenceEqual(expected.AsSpan(y * image.Width, image.Width).ToArray(), row.ToArray(), $"{name}, padding {padding}: row {y}");

                    int paddingEnd = y == (image.Height - 1) ? destination.Length : (y + 1) * stride;
                    Assert.AreEqual(-1, destination.AsSpan((y * stride) + rowSize, paddingEnd - ((y * stride) + rowSize)).IndexOfAnyExcept(SENTINEL), $"{name}, padding {padding}: padding after row {y} was written");
                }
            }

            byte[] exact = new byte[((image.Height - 1) * (rowSize + 3)) + rowSize];
            image.ConvertTo<TTarget>(exact, rowSize + 3);
            Assert.ThrowsExactly<ArgumentException>(() => image.ConvertTo<TTarget>(new byte[exact.Length - 1], rowSize + 3), name);
            Assert.ThrowsExactly<ArgumentException>(() => image.ConvertTo<TTarget>(new byte[exact.Length * 2], rowSize - 1), name);
        }
    }

    [TestMethod]
    public void ConvertToSpanTooSmall()
    {
        Image<ColorRGBA> image = CreateContiguousImage<ColorRGBA>(4);
        Assert.ThrowsExactly<ArgumentException>(() => image.ConvertTo(new ColorBGRA[(WIDTH * HEIGHT) - 1].AsSpan()));
    }

    [TestMethod]
    public void RefImageCopyToBytes()
    {
        foreach ((string name, IImage<ColorRGB> image) in Images<ColorRGB>())
        {
            byte[] expected = MemoryMarshal.AsBytes(Expected<ColorRGB, ColorRGB>(image).AsSpan()).ToArray();

            Assert.AreSequenceEqual(expected, image.AsRefImage().ToRawArray(), $"{name}: RefImage.ToRawArray");
            Assert.AreSequenceEqual(expected, image.ToRawArray(), $"{name}: Image.ToRawArray");

            byte[] destination = new byte[expected.Length + 5];
            destination.AsSpan().Fill(SENTINEL);
            image.AsRefImage().CopyTo(destination.AsSpan());
            Assert.AreSequenceEqual(expected, destination[..expected.Length], $"{name}: RefImage.CopyTo(Span<byte>)");
            Assert.AreEqual(-1, destination.AsSpan(expected.Length).IndexOfAnyExcept(SENTINEL), $"{name}: wrote after the data");

            Assert.AreEqual(expected.Length, image.AsRefImage().SizeInBytes);
            Assert.ThrowsExactly<ArgumentException>(() => image.AsRefImage().CopyTo(new byte[expected.Length - 1].AsSpan()), name);
        }
    }

    [TestMethod]
    public void CopyToStridedBuffer()
    {
        CopyToStridedBuffer<ColorRGB>();
        CopyToStridedBuffer<ColorBGRA>();
    }

    private static void CopyToStridedBuffer<T>()
        where T : unmanaged, IColor<T>
    {
        foreach ((string name, Image<T> image) in Images<T>())
        {
            byte[] expected = MemoryMarshal.AsBytes(Expected<T, T>(image).AsSpan()).ToArray();
            int rowSize = image.Width * T.ColorFormat.BytesPerPixel;

            foreach (int padding in new[] { 0, 5, 64 })
            {
                int stride = rowSize + padding;
                int length = ((image.Height - 1) * stride) + rowSize + 8;

                byte[] fromImage = new byte[length];
                byte[] fromInterface = new byte[length];
                byte[] fromRef = new byte[length];
                fromImage.AsSpan().Fill(SENTINEL);
                fromInterface.AsSpan().Fill(SENTINEL);
                fromRef.AsSpan().Fill(SENTINEL);

                image.CopyTo(fromImage, stride);
                ((IImage)image).CopyTo(fromInterface, stride);
                image.AsRefImage().CopyTo(fromRef, stride);

                Assert.AreSequenceEqual(fromImage, fromInterface, $"{name}, padding {padding}: IImage");
                Assert.AreSequenceEqual(fromImage, fromRef, $"{name}, padding {padding}: RefImage");

                for (int y = 0; y < image.Height; y++)
                {
                    Assert.AreSequenceEqual(expected.AsSpan(y * rowSize, rowSize).ToArray(), fromImage.AsSpan(y * stride, rowSize).ToArray(), $"{name}, padding {padding}: row {y}");

                    int paddingEnd = y == (image.Height - 1) ? fromImage.Length : (y + 1) * stride;
                    Assert.AreEqual(-1, fromImage.AsSpan((y * stride) + rowSize, paddingEnd - ((y * stride) + rowSize)).IndexOfAnyExcept(SENTINEL), $"{name}, padding {padding}: padding after row {y} was written");
                }
            }

            byte[] exact = new byte[((image.Height - 1) * (rowSize + 3)) + rowSize];
            image.CopyTo(exact, rowSize + 3);
            Assert.ThrowsExactly<ArgumentException>(() => image.CopyTo(new byte[exact.Length - 1], rowSize + 3), name);
            Assert.ThrowsExactly<ArgumentException>(() => image.CopyTo(new byte[exact.Length * 2], rowSize - 1), name);
        }
    }

    [TestMethod]
    public void EnumeratorsVisitEveryPixelInOrder()
    {
        foreach ((string name, IImage<ColorBGRA> image) in Images<ColorBGRA>())
        {
            ColorBGRA[] expected = Expected<ColorBGRA, ColorBGRA>(image);

            List<ColorBGRA> generic = [.. image];
            Assert.AreSequenceEqual(expected, generic, $"{name}: IImage<T>");

            List<IColor> colors = [.. image];
            Assert.AreSequenceEqual(expected, colors.Select(c => (ColorBGRA)c).ToArray(), $"{name}: IImage");

            List<ColorBGRA> refImage = [.. image.AsRefImage()];
            Assert.AreSequenceEqual(expected, refImage, $"{name}: RefImage<T>");

            List<ColorBGRA> rows = [];
            foreach (ImageRow<ColorBGRA> row in image.Rows)
                foreach (ColorBGRA color in row)
                    rows.Add(color);
            Assert.AreSequenceEqual(expected, rows, $"{name}: ImageRow<T>");

            using IEnumerator<ColorBGRA> enumerator = image.GetEnumerator();
            while (enumerator.MoveNext()) { }
            Assert.IsFalse(enumerator.MoveNext(), $"{name}: MoveNext after the end");
        }
    }

    [TestMethod]
    public void EnumeratorsOfEmptyImages()
    {
        foreach ((int width, int height) in new[] { (0, 0), (0, 5), (5, 0) })
        {
            Image<ColorRGB> image = Image<ColorRGB>.Create(new ColorRGB[width * height], width, height);

            int count = 0;
            foreach (ColorRGB _ in image) count++;
            foreach (ColorRGB _ in image.AsRefImage()) count++;
            foreach (IColor _ in (IEnumerable<IColor>)image) count++;

            Assert.AreEqual(0, count, $"{width}x{height}");
        }
    }

    /// <summary>
    /// A WIDTH x HEIGHT image with PADDING bytes after every row except the last one.
    /// </summary>
    private static Image<T> CreatePaddedImage<T>(int seed)
        where T : unmanaged, IColor<T>
    {
        int stride = (WIDTH * T.ColorFormat.BytesPerPixel) + PADDING;
        byte[] buffer = new byte[((HEIGHT - 1) * stride) + (WIDTH * T.ColorFormat.BytesPerPixel)];
        new Random(seed).NextBytes(buffer);
        return Image<T>.Wrap(buffer, WIDTH, HEIGHT, stride);
    }

    private static Image<T> CreateContiguousImage<T>(int seed)
        where T : unmanaged, IColor<T>
    {
        T[] pixels = new T[WIDTH * HEIGHT];
        new Random(seed).NextBytes(MemoryMarshal.AsBytes(pixels.AsSpan()));
        return Image<T>.Create(pixels, WIDTH, HEIGHT);
    }

    private static IEnumerable<(string Name, IImage<T> Image)> Images<T>()
        where T : unmanaged, IColor<T>
    {
        yield return ("contiguous", CreateContiguousImage<T>(1));

        Image<T> padded = CreatePaddedImage<T>(2);
        yield return ("padded", padded);
        yield return ("region", (IImage<T>)((IImage)padded).Slice(3, 2, WIDTH - 7, HEIGHT - 5));
        yield return ("full-width rows", (IImage<T>)((IImage)CreateContiguousImage<T>(3)).Slice(0, 4, WIDTH, HEIGHT - 8));
    }

    private static TTarget[] Expected<TSource, TTarget>(IImage<TSource> image)
        where TSource : unmanaged, IColor<TSource>
        where TTarget : unmanaged, IColor<TTarget>
    {
        TTarget[] expected = new TTarget[image.Width * image.Height];
        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                TSource color = image[x, y];
                expected[(y * image.Width) + x] = TTarget.Create(color.R, color.G, color.B, color.A);
            }

        return expected;
    }
}
