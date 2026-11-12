using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace HPPH.Test.Image;

[TestClass]
public class ImageTest
{
    #region Constants

    private readonly List<(int width, int height)> SIZES = [(1920, 1080), (1920, 1), (1, 1080), (200, 500), (1, 1)];

    #endregion

    #region Methods

    [TestMethod]
    public void ImageCreation()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            Assert.AreEqual(width, image.Width);
            Assert.AreEqual(height, image.Height);

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
        }
    }

    [TestMethod]
    public void ImageInnerFull()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
            image = image.Slice(0, 0, image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
        }
    }

    [TestMethod]
    public void ImageInnerPartial()
    {
        (int width, int height) = SIZES[0];

        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
        image = image.Slice(163, 280, 720, 13);

        Assert.AreEqual(720, image.Width);
        Assert.AreEqual(13, image.Height);

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(163 + x, 280 + y), image[x, y]);
    }

    [TestMethod]
    public void ImageInnerInnerPartial()
    {
        (int width, int height) = SIZES[0];

        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
        image = image.Slice(163, 280, 720, 13);
        image = image.Slice(15, 2, 47, 8);

        Assert.AreEqual(47, image.Width);
        Assert.AreEqual(8, image.Height);

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(178 + x, 282 + y), image[x, y]);
    }

    [TestMethod]
    public void ImageRowIndexer()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            Assert.AreEqual(image.Height, image.Rows.Count);

            for (int y = 0; y < image.Height; y++)
            {
                ImageRow<ColorARGB> row = image.Rows[y];
                Assert.AreEqual(image.Width, row.Length);
                for (int x = 0; x < row.Length; x++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), row[x]);
            }
        }
    }

    [TestMethod]
    public void ImageRowIndexerSubImage()
    {
        (int width, int height) = SIZES[0];

        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
        image = image.Slice(163, 280, 720, 13);

        Assert.AreEqual(image.Height, image.Rows.Count);

        for (int y = 0; y < image.Height; y++)
        {
            ImageRow<ColorARGB> row = image.Rows[y];
            Assert.AreEqual(image.Width, row.Length);
            for (int x = 0; x < row.Length; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(163 + x, 280 + y), row[x]);
        }
    }

    [TestMethod]
    public void ImageRowEnumerator()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int y = 0;
            foreach (ImageRow<ColorARGB> row in image.Rows)
            {
                for (int x = 0; x < row.Length; x++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), row[x]);

                y++;
            }
        }
    }

    [TestMethod]
    public void ImageColumnIndexer()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            Assert.AreEqual(image.Width, image.Columns.Count);

            for (int x = 0; x < image.Width; x++)
            {
                ImageColumn<ColorARGB> column = image.Columns[x];
                Assert.AreEqual(image.Height, column.Length);
                for (int y = 0; y < column.Length; y++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), column[y]);
            }
        }
    }

    [TestMethod]
    public void ImageColumnIndexerSubImage()
    {
        (int width, int height) = SIZES[0];

        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
        image = image.Slice(163, 280, 720, 13);

        Assert.AreEqual(image.Width, image.Columns.Count);

        for (int x = 0; x < image.Width; x++)
        {
            ImageColumn<ColorARGB> column = image.Columns[x];
            Assert.AreEqual(image.Height, column.Length);
            for (int y = 0; y < column.Length; y++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(163 + x, 280 + y), column[y]);
        }
    }

    [TestMethod]
    public void ImageColumnEnumerator()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int x = 0;
            foreach (ImageColumn<ColorARGB> column in image.Columns)
            {
                for (int y = 0; y < column.Length; y++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), column[y]);

                x++;
            }
        }
    }

    [TestMethod]
    public void AsRefImage()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            RefImage<ColorARGB> refImage = image.AsRefImage<ColorARGB>();

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    Assert.AreEqual(image[x, y], refImage[x, y]);
        }
    }

    [TestMethod]
    public void ConvertToInPlace()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            ColorARGB[] referenceData = image.ToArray();

            image.ConvertTo<ColorBGRA>();
            ColorARGB[] testData = image.ToArray();

            Assert.AreEqual(referenceData.Length, testData.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                IColor reference = referenceData[i];
                IColor test = testData[i];

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
            }
        }
    }

    [TestMethod]
    public void ConvertToCopy()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            ColorARGB[] referenceData = image.ToArray();

            image.ConvertTo<ColorBGR>();
            ColorARGB[] testData = image.ToArray();

            Assert.AreEqual(referenceData.Length, testData.Length);
            for (int i = 0; i < referenceData.Length; i++)
            {
                IColor reference = referenceData[i];
                IColor test = testData[i];

                Assert.AreEqual(reference.R, test.R, $"R differs at index {i}");
                Assert.AreEqual(reference.G, test.G, $"G differs at index {i}");
                Assert.AreEqual(reference.B, test.B, $"B differs at index {i}");
                Assert.AreEqual(reference.A, test.A, $"A differs at index {i}");
            }
        }
    }

    [TestMethod]
    public void ToArray()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
            ColorARGB[] testData = image.ToArray();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), testData[(y * width) + x]);
        }
    }

    [TestMethod]
    public void CopyTo()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
            ColorARGB[] testData = new ColorARGB[width * height];
            image.CopyTo(testData);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), testData[(y * width) + x]);
        }
    }

    [TestMethod]
    public void SubImage()
    {
        (int width, int height) = SIZES[0];

        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
        RefImage<ColorARGB> subImage = image[10, 20, 100, 200];

        for (int y = 0; y < 200; y++)
            for (int x = 0; x < 100; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(10 + x, 20 + y), subImage[x, y]);
    }

    [TestMethod]
    public void Slice()
    {
        (int width, int height) = SIZES[0];

        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
        Image<ColorARGB> slice = image.Slice(163, 280, 720, 13).Slice(15, 2, 47, 8);

        Assert.AreEqual(47, slice.Width);
        Assert.AreEqual(8, slice.Height);
        Assert.AreEqual(image.Stride, slice.Stride);

        for (int y = 0; y < slice.Height; y++)
            for (int x = 0; x < slice.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(178 + x, 282 + y), slice[x, y]);

        Assert.AreSequenceEqual(image.AsRefImage()[178, 282, 47, 8].ToArray(), slice.ToArray());
    }

    [TestMethod]
    public void SliceSharesData()
    {
        byte[] buffer = new byte[4 * 4 * 3];
        Image<ColorRGB> image = Image<ColorRGB>.Wrap(buffer, 4, 4, 4 * 3);
        Image<ColorRGB> slice = image.Slice(1, 2, 2, 2);

        buffer[(2 * 4 * 3) + (1 * 3)] = 42;

        Assert.AreEqual(42, slice[0, 0].R);
    }

    [TestMethod]
    public void SliceOutOfRange()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 10);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Slice(-1, 0, 5, 5));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Slice(0, -1, 5, 5));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Slice(0, 0, 0, 5));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Slice(0, 0, 5, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Slice(6, 0, 5, 5));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Slice(0, 6, 5, 5));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Slice(5, 0, int.MaxValue, 5));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Slice(0, 5, 5, int.MaxValue));
    }

    [TestMethod]
    public void SliceOutOfRangeDescribesTheProblem()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        AssertParameter(-1, 0, 5, 5, "x", -1);
        AssertParameter(0, -1, 5, 5, "y", -1);
        AssertParameter(0, 0, 0, 5, "width", 0);
        AssertParameter(0, 0, 5, -1, "height", -1);
        AssertParameter(6, 0, 5, 5, "width", 5);
        AssertParameter(0, 16, 5, 5, "height", 5);
        AssertParameter(5, 0, int.MaxValue, 5, "width", int.MaxValue);

        void AssertParameter(int x, int y, int width, int height, string expectedParamName, int expectedValue)
        {
            ArgumentOutOfRangeException ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => image.Slice(x, y, width, height));
            Assert.AreEqual(expectedParamName, ex.ParamName, $"Slice({x}, {y}, {width}, {height})");
            Assert.AreEqual(expectedValue, ex.ActualValue, $"Slice({x}, {y}, {width}, {height})");

            ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[x, y, width, height]);
            Assert.AreEqual(expectedParamName, ex.ParamName, $"[{x}, {y}, {width}, {height}]");
            Assert.AreEqual(expectedValue, ex.ActualValue, $"[{x}, {y}, {width}, {height}]");
        }
    }

    [TestMethod]
    public void IndexerOutOfRangeDescribesTheProblem()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        AssertParameter(-1, 0, "x", -1);
        AssertParameter(10, 0, "x", 10);
        AssertParameter(0, -1, "y", -1);
        AssertParameter(0, 20, "y", 20);
        AssertParameter(10, 20, "x", 10);

        void AssertParameter(int x, int y, string expectedParamName, int expectedValue)
        {
            ArgumentOutOfRangeException ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[x, y]);
            Assert.AreEqual(expectedParamName, ex.ParamName, $"[{x}, {y}]");
            Assert.AreEqual(expectedValue, ex.ActualValue, $"[{x}, {y}]");
        }
    }

    [TestMethod]
    public void RowAndColumnIndexerOutOfRangeDescribesTheProblem()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        AssertParameter(() => _ = image.Rows[-1], "row", -1, "Rows[-1]");
        AssertParameter(() => _ = image.Rows[20], "row", 20, "Rows[20]");
        AssertParameter(() => _ = image.Columns[-1], "column", -1, "Columns[-1]");
        AssertParameter(() => _ = image.Columns[10], "column", 10, "Columns[10]");
        AssertParameter(() => _ = image.Rows[0][-1], "x", -1, "Rows[0][-1]");
        AssertParameter(() => _ = image.Rows[0][10], "x", 10, "Rows[0][10]");
        AssertParameter(() => _ = image.Columns[0][-1], "y", -1, "Columns[0][-1]");
        AssertParameter(() => _ = image.Columns[0][20], "y", 20, "Columns[0][20]");

        static void AssertParameter(Action action, string expectedParamName, int expectedValue, string access)
        {
            ArgumentOutOfRangeException ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);
            Assert.AreEqual(expectedParamName, ex.ParamName, access);
            Assert.AreEqual(expectedValue, ex.ActualValue, access);
        }
    }

    [TestMethod]
    public void Stride()
    {
        const int STRIDE = (10 * 4) + 12;

        Image<ColorBGRA> image = Image<ColorBGRA>.Wrap(new byte[STRIDE * 5], 10, 5, STRIDE);

        Assert.AreEqual(STRIDE, image.Stride);
        Assert.AreEqual(STRIDE, ((IImage)image).Stride);
        Assert.AreEqual(STRIDE, image.Slice(2, 1, 3, 3).Stride);
        Assert.AreEqual(STRIDE, image.AsRefImage().Stride);
        Assert.AreEqual(10 * 3, TestDataHelper.CreateTestImage<ColorRGB>(10, 5).Stride);
    }

    [TestMethod]
    public void GenericRowsIterate()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int y = 0;
            foreach (ImageRow<ColorARGB> row in image.Rows)
            {
                int x = 0;
                foreach (ColorARGB color in row)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x++, y), color);

                y++;
            }
        }
    }

    [TestMethod]
    public void GenericRowsCopyTo()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int y = 0;
            foreach (ImageRow<ColorARGB> row in image.Rows)
            {
                ColorARGB[] colors = new ColorARGB[width];
                byte[] bytes = new byte[width * 4];
                row.CopyTo(colors);
                row.CopyTo(bytes);

                int x = 0;
                foreach (ColorARGB color in colors)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x++, y), color);

                for (x = 0; x < width; x++)
                {
                    ColorARGB reference = TestDataHelper.GetColorFromLocation<ColorARGB>(x, y);

                    Assert.AreEqual(reference.A, bytes[(x * 4) + 0]);
                    Assert.AreEqual(reference.R, bytes[(x * 4) + 1]);
                    Assert.AreEqual(reference.G, bytes[(x * 4) + 2]);
                    Assert.AreEqual(reference.B, bytes[(x * 4) + 3]);
                }

                y++;
            }
        }
    }

    [TestMethod]
    public void GenericRowsToArray()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int y = 0;
            foreach (ImageRow<ColorARGB> row in image.Rows)
            {
                ColorARGB[] data = row.ToArray();

                for (int x = 0; x < width; x++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), data[x]);

                y++;
            }
        }
    }

    [TestMethod]
    public void GenericColumnsIterate()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int x = 0;
            foreach (ImageColumn<ColorARGB> column in image.Columns)
            {
                int y = 0;
                foreach (ColorARGB color in column)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y++), color);

                x++;
            }
        }
    }

    [TestMethod]
    public void GenericColumnsCopyTo()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int x = 0;
            foreach (ImageColumn<ColorARGB> column in image.Columns)
            {
                ColorARGB[] colors = new ColorARGB[height];
                byte[] bytes = new byte[height * 4];
                column.CopyTo(colors);
                column.CopyTo(bytes);

                int y = 0;
                foreach (ColorARGB color in colors)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y++), color);

                for (y = 0; y < height; y++)
                {
                    ColorARGB reference = TestDataHelper.GetColorFromLocation<ColorARGB>(x, y);

                    Assert.AreEqual(reference.A, bytes[(y * 4) + 0]);
                    Assert.AreEqual(reference.R, bytes[(y * 4) + 1]);
                    Assert.AreEqual(reference.G, bytes[(y * 4) + 2]);
                    Assert.AreEqual(reference.B, bytes[(y * 4) + 3]);
                }

                x++;
            }
        }
    }

    [TestMethod]
    public void GenericColumnsToArray()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int x = 0;
            foreach (ImageColumn<ColorARGB> column in image.Columns)
            {
                ColorARGB[] data = column.ToArray();

                for (int y = 0; y < height; y++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), data[y]);

                x++;
            }
        }
    }

    [TestMethod]
    public void GenericEnumerate()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
            ColorARGB[] reference = TestDataHelper.GetPixelData<ColorARGB>(width, height);

            int i = 0;
            foreach (ColorARGB color in image)
                Assert.AreEqual(reference[i++], color);

            IEnumerable enumerable = image;
            i = 0;
            foreach (ColorARGB color in enumerable)
                Assert.AreEqual(reference[i++], color);
        }
    }

    [TestMethod]
    public unsafe void Pin()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
            ColorARGB[] reference = TestDataHelper.GetPixelData<ColorARGB>(width, height);

            fixed (byte* ptr = image)
            {
                for (int i = 0; i < reference.Length; i++)
                {
                    Assert.AreEqual(reference[i].A, ptr[(i * 4) + 0]);
                    Assert.AreEqual(reference[i].R, ptr[(i * 4) + 1]);
                    Assert.AreEqual(reference[i].G, ptr[(i * 4) + 2]);
                    Assert.AreEqual(reference[i].B, ptr[(i * 4) + 3]);
                }
            }
        }
    }

    [TestMethod]
    public void CreateWrongStride()
    {
        Assert.ThrowsExactly<ArgumentException>(() => Image<ColorRGB>.Create([], 10, 20, 9));
    }

    [TestMethod]
    public void CreateWrongBufferSize()
    {
        Assert.ThrowsExactly<ArgumentException>(() => Image<ColorRGB>.Create(new byte[299], 10, 10, 30));
    }

    [TestMethod]
    public void CreateDropsPadding()
    {
        const int WIDTH = 10;
        const int HEIGHT = 5;
        const int STRIDE = (WIDTH * 3) + 7;

        // Padding and trailing bytes are filled with garbage that must not end up in the image.
        byte[] buffer = new byte[(STRIDE * HEIGHT) + 100];
        buffer.AsSpan().Fill(0xAB);

        Image<ColorRGB> expected = TestDataHelper.CreateTestImage<ColorRGB>(WIDTH, HEIGHT);
        for (int y = 0; y < HEIGHT; y++)
            expected.Rows[y].CopyTo(buffer.AsSpan(y * STRIDE));

        Image<ColorRGB> image = Image<ColorRGB>.Create(buffer, WIDTH, HEIGHT, STRIDE);

        Assert.AreEqual(WIDTH * 3, image.Stride);
        Assert.AreEqual(WIDTH * HEIGHT * 3, image.SizeInBytes);
        Assert.IsTrue(image.ContentEquals(expected));
        Assert.AreSequenceEqual(expected.ToRawArray(), image.ToRawArray());
        Assert.AreEqual(expected.Sum(), image.Sum());
    }

    [TestMethod]
    public void CreateCopiesData()
    {
        byte[] buffer = new byte[4 * 4 * 3];
        Image<ColorRGB> image = Image<ColorRGB>.Create(buffer, 4, 4, 4 * 3);

        buffer[0] = 42;

        Assert.AreEqual(0, image[0, 0].R);
    }

    [TestMethod]
    public void CreateEmpty()
    {
        Image<ColorRGB> zeroHeight = Image<ColorRGB>.Create(new byte[10], 3, 0, 10);
        Assert.AreEqual(0, zeroHeight.SizeInBytes);
        Assert.AreEqual(3 * 3, zeroHeight.Stride);

        Image<ColorRGB> zeroWidth = Image<ColorRGB>.Create(new byte[100], 0, 5, 10);
        Assert.AreEqual(0, zeroWidth.SizeInBytes);
        Assert.AreEqual(0, zeroWidth.Stride);
    }

    [TestMethod]
    public void CopyToByteEmpty()
    {
        Assert.ThrowsExactly<ArgumentException>(() => TestDataHelper.CreateTestImage<ColorRGB>(10, 10).CopyTo((Span<byte>)default));
    }

    [TestMethod]
    public void CopyToColorEmpty()
    {
        Assert.ThrowsExactly<ArgumentException>(() => TestDataHelper.CreateTestImage<ColorRGB>(10, 10).CopyTo((Span<ColorRGB>)default));
    }

    [TestMethod]
    public void CopyToByteWrongSize()
    {
        Assert.ThrowsExactly<ArgumentException>(() => TestDataHelper.CreateTestImage<ColorRGB>(10, 10).CopyTo(new byte[(10 * 10 * 3) - 1]));
    }

    [TestMethod]
    public void CopyToColorWrongSize()
    {
        Assert.ThrowsExactly<ArgumentException>(() => TestDataHelper.CreateTestImage<ColorRGB>(10, 10).CopyTo(new ColorRGB[(10 * 10) - 1]));
    }

    [TestMethod]
    public void AsRefImageWrongType()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 10);
        Assert.ThrowsExactly<ArgumentException>(() => _ = image.AsRefImage<ColorBGR>());
    }

    [TestMethod]
    public void PinEmpty()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(0, 0);
        Assert.IsTrue(Unsafe.IsNullRef(in image.GetPinnableReference()));
    }

    [TestMethod]
    public void IndexerWrongXBig()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[10, 19]);
    }

    [TestMethod]
    public void IndexerWrongYBig()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[9, 20]);
    }

    [TestMethod]
    public void IndexerWrongXSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[-1, 19]);
    }

    [TestMethod]
    public void IndexerWrongYSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[9, -1]);
    }

    [TestMethod]
    public void SubImageWrongX()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[-1, 0, 10, 20]);
    }

    [TestMethod]
    public void SubImageWrongY()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[0, -1, 10, 20]);
    }

    [TestMethod]
    public void SubImageWrongWidth()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[0, 0, 0, 20]);
    }

    [TestMethod]
    public void SubImageWrongHeight()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[0, 0, 10, 0]);
    }

    [TestMethod]
    public void SubImageInvalidSizeWidth()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[1, 0, 10, 20]);
    }

    [TestMethod]
    public void SubImageInvalidSizeHeight()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image[0, 1, 10, 20]);
    }

    [TestMethod]
    public void SubImageInterfaceWrongX()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Slice(-1, 0, 10, 20));
    }

    [TestMethod]
    public void SubImageInterfaceWrongY()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Slice(0, -1, 10, 20));
    }

    [TestMethod]
    public void SubImageInterfaceWrongWidth()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Slice(0, 0, 0, 20));
    }

    [TestMethod]
    public void SubImageInterfaceWrongHeight()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Slice(0, 0, 10, 0));
    }

    [TestMethod]
    public void SubImageInterfaceInvalidSizeWidth()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Slice(1, 0, 10, 20));
    }

    [TestMethod]
    public void SubImageInterfaceInvalidSizeHeight()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Slice(0, 1, 10, 20));
    }

    [TestMethod]
    public void ColumnsIndexerToBig()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Columns[20]);
    }

    [TestMethod]
    public void ColumnsIndexerToSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Columns[-1]);
    }

    [TestMethod]
    public void RowsIndexerToBig()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Rows[20]);
    }

    [TestMethod]
    public void RowsIndexerToSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Rows[-1]);
    }

    [TestMethod]
    public void ColumnIndexerToBig()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Columns[1][20]);
    }

    [TestMethod]
    public void ColumnIndexerToSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Columns[1][-1]);
    }

    [TestMethod]
    public void RowIndexerToBig()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Rows[1][10]);
    }

    [TestMethod]
    public void RowIndexerToSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = image.Rows[1][-1]);
    }

    [TestMethod]
    public void ColumnCopyToByteEmpty()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentException>(() => image.Columns[1].CopyTo((Span<byte>)default));
    }

    [TestMethod]
    public void ColumnCopyToColorEmpty()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentException>(() => image.Columns[1].CopyTo((Span<ColorRGB>)default));
    }

    [TestMethod]
    public void ColumnCopyToByteToSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentException>(() => image.Columns[1].CopyTo(new byte[(20 * 3) - 1]));
    }

    [TestMethod]
    public void ColumnCopyToColorToSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentException>(() => image.Columns[1].CopyTo(new ColorRGB[19]));
    }

    [TestMethod]
    public void RowCopyToByteEmpty()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentException>(() => image.Rows[1].CopyTo((Span<byte>)default));
    }

    [TestMethod]
    public void RowCopyToColorEmpty()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentException>(() => image.Rows[1].CopyTo((Span<ColorRGB>)default));
    }

    [TestMethod]
    public void RowCopyToByteToSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentException>(() => image.Rows[1].CopyTo(new byte[(10 * 3) - 1]));
    }

    [TestMethod]
    public void RowCopyToColorToSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.ThrowsExactly<ArgumentException>(() => image.Rows[1].CopyTo(new ColorRGB[9]));
    }

    [TestMethod]
    public void ContentEqualsIImage()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        IImage image2 = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.IsTrue(image.ContentEquals(image2));
        Assert.IsTrue(image2.ContentEquals(image));

        Assert.IsFalse(image.ContentEquals((IImage?)null));
    }

    [TestMethod]
    public void ContentEqualsImage()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        Image<ColorRGB> image2 = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.IsTrue(image.ContentEquals(image2));

        Assert.IsFalse(image.ContentEquals((Image<ColorRGB>?)null));
    }

    [TestMethod]
    public void ContentEqualsIgnoresPadding()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Image<ColorRGB> slice = image.Slice(2, 3, 5, 7);
        Image<ColorRGB> compact = Image<ColorRGB>.Create(slice.ToArray(), 5, 7);

        Assert.AreNotEqual(slice.Stride, compact.Stride);
        Assert.IsTrue(slice.ContentEquals(compact));
        Assert.IsTrue(compact.ContentEquals(slice));
        Assert.IsTrue(compact.ContentEquals(image[2, 3, 5, 7]));
        Assert.IsTrue(image[2, 3, 5, 7].ContentEquals(compact.AsRefImage()));
        Assert.IsFalse(image[2, 3, 5, 7].ContentEquals(image[3, 3, 5, 7]));
    }

    [TestMethod]
    public void ContentEqualsIImageWrongSize()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        IImage image2 = TestDataHelper.CreateTestImage<ColorRGB>(10, 10);
        Assert.IsFalse(image.ContentEquals(image2));

        IImage image3 = TestDataHelper.CreateTestImage<ColorRGB>(20, 20);
        Assert.IsFalse(image.ContentEquals(image3));
    }

    [TestMethod]
    public void ContentEqualsImageWrongSize()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        Image<ColorRGB> image2 = TestDataHelper.CreateTestImage<ColorRGB>(10, 10);
        Assert.IsFalse(image.ContentEquals(image2));

        Image<ColorRGB> image3 = TestDataHelper.CreateTestImage<ColorRGB>(20, 20);
        Assert.IsFalse(image.ContentEquals(image3));
    }

    [TestMethod]
    public void ContentEqualsIImageWrongPixel()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        ColorRGB[] array = image.ToArray();
        array[1] = new ColorRGB(255, 255, 255);
        IImage image2 = Image<ColorRGB>.Create(array, 10, 20);

        Assert.IsFalse(image.ContentEquals(image2));
    }

    [TestMethod]
    public void ContentEqualsImageWrongPixel()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        ColorRGB[] array = image.ToArray();
        array[1] = new ColorRGB(255, 255, 255);
        Image<ColorRGB> image2 = Image<ColorRGB>.Create(array, 10, 20);

        Assert.IsFalse(image.ContentEquals(image2));
    }

    [TestMethod]
    public void ContentEqualsIImageWrongFormat()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImage image2 = TestDataHelper.CreateTestImage<ColorBGR>(10, 20);

        Assert.IsFalse(image.ContentEquals(image2));
    }

    [TestMethod]
    public void EqualsIsReferenceEquality()
    {
        ColorRGBA[] pixels = [new(1, 2, 3, 4), new(5, 6, 7, 8), new(9, 10, 11, 12), new(13, 14, 15, 16)];
        Image<ColorRGBA> a = Image<ColorRGBA>.Create(pixels, 2, 2);
        Image<ColorRGBA> b = Image<ColorRGBA>.Create(pixels, 2, 2);

        Assert.IsTrue(a.ContentEquals(b));
        Assert.IsFalse(a.Equals(b));
        Assert.IsTrue(a.Equals(a));

        // Instances with the same content stay distinct in collections.
        List<Image<ColorRGBA>> list = [a, b];
        list.Remove(b);
        Assert.AreSame(a, list[0]);
        Assert.HasCount(2, new HashSet<Image<ColorRGBA>> { a, b });
    }

    #endregion
}