using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HPPH.Test.Image;

[TestClass]
public class RefImageTest
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
            RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();

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
            RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();
            image = image[0, 0, image.Width, image.Height];

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
        }
    }

    [TestMethod]
    public void ImageEnumerator()
    {
        foreach ((int width, int height) in SIZES)
        {
            RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();

            int counter = 0;
            foreach (ColorARGB color in image)
            {
                int x = counter % image.Width;
                int y = counter / image.Width;

                if (y == 1) Debugger.Break();

                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), color);

                counter++;
            }
        }
    }

    [TestMethod]
    public void ImageInnerPartial()
    {
        (int width, int height) = SIZES[0];

        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();
        image = image[163, 280, 720, 13];

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

        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();
        image = image[163, 280, 720, 13];
        image = image[15, 2, 47, 8];

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
            RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();

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

        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();
        image = image[163, 280, 720, 13];

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
            RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();

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
            RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();

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

        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();
        image = image[163, 280, 720, 13];

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
            RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();

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
    public void ToArray()
    {
        foreach ((int width, int height) in SIZES)
        {
            RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();
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
            RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();
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

        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();
        RefImage<ColorARGB> subImage = image[10, 20, 100, 200];

        for (int y = 0; y < 200; y++)
            for (int x = 0; x < 100; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(10 + x, 20 + y), subImage[x, y]);
    }

    [TestMethod]
    public void Slice()
    {
        (int width, int height) = SIZES[0];

        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();
        RefImage<ColorARGB> slice = image.Slice(10, 20, 100, 200).Slice(5, 7, 30, 40);

        Assert.AreEqual(30, slice.Width);
        Assert.AreEqual(40, slice.Height);
        Assert.AreEqual(image.Stride, slice.Stride);
        Assert.IsTrue(image[15, 27, 30, 40].ToArray().AsSpan().SequenceEqual(slice.ToArray()));

        for (int y = 0; y < 40; y++)
            for (int x = 0; x < 30; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(15 + x, 27 + y), slice[x, y]);
    }

    [TestMethod]
    public void SliceOutOfRange()
    {
        AssertThrows(-1, 0, 10, 20);
        AssertThrows(0, -1, 10, 20);
        AssertThrows(0, 0, 0, 20);
        AssertThrows(0, 0, 10, 0);
        AssertThrows(1, 0, 10, 20);
        AssertThrows(0, 1, 10, 20);
        AssertThrows(5, 0, int.MaxValue, 20);
        AssertThrows(0, 5, 10, int.MaxValue);

        static void AssertThrows(int x, int y, int width, int height)
            => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage().Slice(x, y, width, height));
    }

    [TestMethod]
    public void SliceOutOfRangeDescribesTheProblem()
    {
        AssertParameter(-1, 0, 5, 5, "x", -1);
        AssertParameter(0, -1, 5, 5, "y", -1);
        AssertParameter(0, 0, 0, 5, "width", 0);
        AssertParameter(0, 0, 5, 0, "height", 0);
        AssertParameter(6, 0, 5, 5, "width", 5);
        AssertParameter(0, 16, 5, 5, "height", 5);

        static void AssertParameter(int x, int y, int width, int height, string expectedParamName, int expectedValue)
        {
            ArgumentOutOfRangeException ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage().Slice(x, y, width, height));
            Assert.AreEqual(expectedParamName, ex.ParamName, $"Slice({x}, {y}, {width}, {height})");
            Assert.AreEqual(expectedValue, ex.ActualValue, $"Slice({x}, {y}, {width}, {height})");
        }
    }

    [TestMethod]
    public void IndexerOutOfRangeDescribesTheProblem()
    {
        AssertParameter(10, 0, "x", 10);
        AssertParameter(0, -1, "y", -1);

        static void AssertParameter(int x, int y, string expectedParamName, int expectedValue)
        {
            ArgumentOutOfRangeException ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage()[x, y]);
            Assert.AreEqual(expectedParamName, ex.ParamName, $"[{x}, {y}]");
            Assert.AreEqual(expectedValue, ex.ActualValue, $"[{x}, {y}]");
        }
    }

    [TestMethod]
    public unsafe void Pin()
    {
        foreach ((int width, int height) in SIZES)
        {
            RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(width, height).AsRefImage();
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
    public void PinEmpty()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(0, 0).AsRefImage();
        Assert.IsTrue(Unsafe.IsNullRef(in image.GetPinnableReference()));
    }

    [TestMethod]
    public void CopyToColorEmpty()
    {
        Assert.ThrowsExactly<ArgumentException>(() => TestDataHelper.CreateTestImage<ColorRGB>(10, 10).AsRefImage().CopyTo(default(Span<ColorRGB>)));
    }

    [TestMethod]
    public void CopyToColorWrongSize()
    {
        Assert.ThrowsExactly<ArgumentException>(() => TestDataHelper.CreateTestImage<ColorRGB>(10, 10).AsRefImage().CopyTo(new ColorRGB[(10 * 10) - 1]));
    }

    [TestMethod]
    public void IndexerWrongXBig()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            ColorRGB test = image[10, 19];
        });
    }

    [TestMethod]
    public void IndexerWrongYBig()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            ColorRGB test = image[9, 20];
        });
    }

    [TestMethod]
    public void IndexerWrongXSmall()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            ColorRGB test = image[-1, 19];
        });
    }

    [TestMethod]
    public void IndexerWrongYSmall()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            ColorRGB test = image[9, -1];
        });
    }

    [TestMethod]
    public void SubImageWrongX()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            RefImage<ColorRGB> test = image[-1, 0, 10, 20];
        });
    }

    [TestMethod]
    public void SubImageWrongY()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            RefImage<ColorRGB> test = image[0, -1, 10, 20];
        });
    }

    [TestMethod]
    public void SubImageWrongWidth()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            RefImage<ColorRGB> test = image[0, 0, 0, 20];
        });
    }

    [TestMethod]
    public void SubImageWrongHeight()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            RefImage<ColorRGB> test = image[0, 0, 10, 0];
        });
    }

    [TestMethod]
    public void SubImageInvalidSizeWidth()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            RefImage<ColorRGB> test = image[1, 0, 10, 20];
        });
    }

    [TestMethod]
    public void SubImageInvalidSizeHeight()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            RefImage<ColorRGB> test = image[0, 1, 10, 20];
        });
    }

    [TestMethod]
    public void ColumnsIndexerToBig()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            ImageColumn<ColorRGB> test = image.Columns[20];
        });
    }

    [TestMethod]
    public void ColumnsIndexerToSmall()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            ImageColumn<ColorRGB> test = image.Columns[-1];
        });
    }

    [TestMethod]
    public void RowsIndexerToBig()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            ImageRow<ColorRGB> test = image.Rows[20];
        });
    }

    [TestMethod]
    public void RowsIndexerToSmall()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            ImageRow<ColorRGB> test = image.Rows[-1];
        });
    }

    [TestMethod]
    public void ColumnIndexerToBig()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            IColor test = image.Columns[1][20];
        });
    }

    [TestMethod]
    public void ColumnIndexerToSmall()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            IColor test = image.Columns[1][-1];
        });
    }

    [TestMethod]
    public void RowIndexerToBig()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            IColor test = image.Rows[1][10];
        });
    }

    [TestMethod]
    public void RowIndexerToSmall()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            IColor test = image.Rows[1][-1];
        });
    }

    [TestMethod]
    public void ColumnCopyToByteEmpty()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            image.Columns[1].CopyTo((Span<byte>)default);
        });
    }

    [TestMethod]
    public void ColumnCopyToColorEmpty()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            image.Rows[1].CopyTo((Span<ColorRGB>)default);
        });
    }

    [TestMethod]
    public void ColumnCopyToByteToSmall()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            image.Columns[1].CopyTo(new byte[(20 * 3) - 1]);
        });
    }

    [TestMethod]
    public void ColumnCopyToColorToSmall()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            image.Columns[1].CopyTo(new ColorRGB[19]);
        });
    }

    [TestMethod]
    public void RowCopyToByteEmpty()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            image.Rows[1].CopyTo((Span<byte>)default);
        });
    }

    [TestMethod]
    public void RowCopyToColorEmpty()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            image.Rows[1].CopyTo((Span<ColorRGB>)default);
        });
    }

    [TestMethod]
    public void RowCopyToByteToSmall()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            image.Rows[1].CopyTo(new byte[(10 * 3) - 1]);
        });
    }

    [TestMethod]
    public void RowCopyToColorToSmall()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
            image.Rows[1].CopyTo(new ColorRGB[9]);
        });
    }

    [TestMethod]
    public void ToImage()
    {
        foreach ((int width, int height) in SIZES)
        {
            Image<ColorRGB> source = TestDataHelper.CreateTestImage<ColorRGB>(width, height);
            RefImage<ColorRGB> region = source[width / 4, height / 4, Math.Max(1, width / 2), Math.Max(1, height / 2)];

            Image<ColorRGB> image = region.ToImage();

            Assert.AreEqual(region.Width, image.Width);
            Assert.AreEqual(region.Height, image.Height);
            Assert.AreEqual(region.Width * 3, image.Stride);
            Assert.IsTrue(image.ContentEquals(region));
        }
    }

    [TestMethod]
    public void ToImageIsIndependentOfSource()
    {
        const int WIDTH = 4;
        const int HEIGHT = 3;
        const int STRIDE = (WIDTH * 4) + 8;

        byte[] buffer = new byte[STRIDE * HEIGHT];
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = (byte)i;

        Image<ColorRGBA> source = Image<ColorRGBA>.Wrap(buffer, WIDTH, HEIGHT, STRIDE);
        Image<ColorRGBA> copy = source[1, 1, 2, 2].ToImage();
        ColorRGBA[] expected = copy.ToArray();

        buffer.AsSpan().Clear();

        Assert.AreSequenceEqual(expected, copy.ToArray());
        Assert.IsFalse(copy.ContentEquals(source[1, 1, 2, 2]));
    }

    [TestMethod]
    public void ToImageEmpty()
    {
        Image<ColorRGB> image = RefImage<ColorRGB>.Wrap([], 0, 0, 0).ToImage();

        Assert.AreEqual(0, image.Width);
        Assert.AreEqual(0, image.Height);
        Assert.AreEqual(0, image.SizeInBytes);
    }

    #endregion
}