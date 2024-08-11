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
    [ExpectedException(typeof(ArgumentNullException))]
    public void CopyToColorNull()
    {
        TestDataHelper.CreateTestImage<ColorRGB>(10, 10).AsRefImage().CopyTo(null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CopyToColorWrongSize()
    {
        TestDataHelper.CreateTestImage<ColorRGB>(10, 10).AsRefImage().CopyTo(new ColorRGB[(10 * 10) - 1]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWrongXBig()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        ColorRGB test = image[10, 19];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWrongYBig()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        ColorRGB test = image[9, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWrongXSmall()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        ColorRGB test = image[-1, 19];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWrongYSmall()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        ColorRGB test = image[9, -1];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SubImageWrongX()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        RefImage<ColorRGB> test = image[-1, 0, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SubImageWrongY()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        RefImage<ColorRGB> test = image[0, -1, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SubImageWrongWidth()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        RefImage<ColorRGB> test = image[0, 0, 0, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SubImageWrongHeight()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        RefImage<ColorRGB> test = image[0, 0, 10, 0];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SubImageInvalidSizeWidth()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        RefImage<ColorRGB> test = image[1, 0, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SubImageInvalidSizeHeight()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        RefImage<ColorRGB> test = image[0, 1, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ColumnsIndexerToBig()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        ImageColumn<ColorRGB> test = image.Columns[20];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ColumnsIndexerToSmall()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        ImageColumn<ColorRGB> test = image.Columns[-1];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void RowsIndexerToBig()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        ImageRow<ColorRGB> test = image.Rows[20];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void RowsIndexerToSmall()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        ImageRow<ColorRGB> test = image.Rows[-1];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ColumnIndexerToBig()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        IColor test = image.Columns[1][20];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ColumnIndexerToSmall()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        IColor test = image.Columns[1][-1];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void RowIndexerToBig()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        IColor test = image.Rows[1][10];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void RowIndexerToSmall()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        IColor test = image.Rows[1][-1];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ColumnCopyToByteNull()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        image.Columns[1].CopyTo((Span<byte>)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ColumnCopyToColorNull()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        image.Rows[1].CopyTo((Span<ColorRGB>)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ColumnCopyToByteToSmall()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        image.Columns[1].CopyTo(new byte[(20 * 3) - 1]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ColumnCopyToColorToSmall()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        image.Columns[1].CopyTo(new ColorRGB[19]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RowCopyToByteNull()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        image.Rows[1].CopyTo((Span<byte>)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RowCopyToColorNull()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        image.Rows[1].CopyTo((Span<ColorRGB>)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void RowCopyToByteToSmall()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        image.Rows[1].CopyTo(new byte[(10 * 3) - 1]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void RowCopyToColorToSmall()
    {
        RefImage<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20).AsRefImage();
        image.Rows[1].CopyTo(new ColorRGB[9]);
    }

    #endregion
}