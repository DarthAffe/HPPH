using System.Collections;

namespace HPPH.Test.Image;

[TestClass]
public class ImageTest
{
    #region Constants

    private const int TEST_WIDTH = 1920;
    private const int TEST_HEIGHT = 1080;

    #endregion

    #region Methods

    [TestMethod]
    public void ImageCreation()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        Assert.AreEqual(TEST_WIDTH, image.Width);
        Assert.AreEqual(TEST_HEIGHT, image.Height);

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
    }

    [TestMethod]
    public void ImageInnerFull()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
        image = image[0, 0, image.Width, image.Height];

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
    }

    [TestMethod]
    public void ImageEnumerator()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int counter = 0;
        foreach (IColor color in image)
        {
            int x = counter % image.Width;
            int y = counter / image.Width;

            Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), color);

            counter++;
        }
    }

    [TestMethod]
    public void ImageInnerPartial()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
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
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
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
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        Assert.AreEqual(image.Height, image.Rows.Count);

        for (int y = 0; y < image.Height; y++)
        {
            IImageRow row = image.Rows[y];
            Assert.AreEqual(image.Width, row.Length);
            for (int x = 0; x < row.Length; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), row[x]);
        }
    }

    [TestMethod]
    public void ImageRowEnumerator()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int y = 0;
        foreach (IImageRow row in image.Rows)
        {
            for (int x = 0; x < row.Length; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), row[x]);

            y++;
        }
    }

    [TestMethod]
    public void ImageColumnIndexer()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        Assert.AreEqual(image.Width, image.Columns.Count);

        for (int x = 0; x < image.Width; x++)
        {
            IImageColumn column = image.Columns[x];
            Assert.AreEqual(image.Height, column.Length);
            for (int y = 0; y < column.Length; y++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), column[y]);
        }
    }

    [TestMethod]
    public void ImageColumnEnumerator()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int x = 0;
        foreach (IImageColumn column in image.Columns)
        {
            for (int y = 0; y < column.Length; y++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), column[y]);

            x++;
        }
    }

    [TestMethod]
    public void AsRefImage()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
        image = image[163, 280, 720, 13];
        image = image[15, 2, 47, 8];

        RefImage<ColorARGB> refImage = image.AsRefImage<ColorARGB>();

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(image[x, y], refImage[x, y]);
    }

    [TestMethod]
    public void ConvertToInPlace()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        IColor[] referenceData = image.ToArray();

        image.ConvertTo<ColorBGRA>();
        IColor[] testData = image.ToArray();

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

    [TestMethod]
    public void ConvertToCopy()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        IColor[] referenceData = image.ToArray();

        image.ConvertTo<ColorBGR>();
        IColor[] testData = image.ToArray();

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

    [TestMethod]
    public void ToArray()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
        ColorARGB[] testData = image.ToArray();

        for (int y = 0; y < TEST_HEIGHT; y++)
            for (int x = 0; x < TEST_WIDTH; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), testData[(y * TEST_WIDTH) + x]);
    }

    [TestMethod]
    public void CopyTo()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
        ColorARGB[] testData = new ColorARGB[TEST_WIDTH * TEST_HEIGHT];
        image.CopyTo(testData);

        for (int y = 0; y < TEST_HEIGHT; y++)
            for (int x = 0; x < TEST_WIDTH; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), testData[(y * TEST_WIDTH) + x]);
    }

    [TestMethod]
    public void SubImage()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
        RefImage<ColorARGB> subImage = image[10, 20, 100, 200];

        for (int y = 0; y < 200; y++)
            for (int x = 0; x < 100; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(10 + x, 20 + y), subImage[x, y]);
    }

    [TestMethod]
    public void GenericRowsIterate()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int y = 0;
        foreach (ImageRow<ColorARGB> row in image.Rows)
        {
            int x = 0;
            foreach (ColorARGB color in row)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x++, y), color);

            y++;
        }
    }

    [TestMethod]
    public void GenericRowsCopyTo()
    {

        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int y = 0;
        foreach (ImageRow<ColorARGB> row in image.Rows)
        {
            ColorARGB[] colors = new ColorARGB[TEST_WIDTH];
            byte[] bytes = new byte[TEST_WIDTH * 4];
            row.CopyTo(colors);
            row.CopyTo(bytes);

            int x = 0;
            foreach (ColorARGB color in colors)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x++, y), color);

            for (x = 0; x < TEST_WIDTH; x++)
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

    [TestMethod]
    public void GenericRowsToArray()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int y = 0;
        foreach (ImageRow<ColorARGB> row in image.Rows)
        {
            ColorARGB[] data = row.ToArray();

            for (int x = 0; x < TEST_WIDTH; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), data[x]);

            y++;
        }
    }

    [TestMethod]
    public void GenericColumnsIterate()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int x = 0;
        foreach (ImageColumn<ColorARGB> column in image.Columns)
        {
            int y = 0;
            foreach (ColorARGB color in column)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y++), color);

            x++;
        }
    }

    [TestMethod]
    public void GenericColumnsCopyTo()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int x = 0;
        foreach (ImageColumn<ColorARGB> column in image.Columns)
        {
            ColorARGB[] colors = new ColorARGB[TEST_HEIGHT];
            byte[] bytes = new byte[TEST_HEIGHT * 4];
            column.CopyTo(colors);
            column.CopyTo(bytes);

            int y = 0;
            foreach (ColorARGB color in colors)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y++), color);

            for (y = 0; y < TEST_HEIGHT; y++)
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

    [TestMethod]
    public void GenericColumnsToArray()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int x = 0;
        foreach (ImageColumn<ColorARGB> column in image.Columns)
        {
            ColorARGB[] data = column.ToArray();

            for (int y = 0; y < TEST_HEIGHT; y++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), data[y]);

            x++;
        }
    }

    [TestMethod]
    public void GenericEnumerate()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
        ColorARGB[] reference = TestDataHelper.GetPixelData<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int i = 0;
        foreach (ColorARGB color in image)
            Assert.AreEqual(reference[i++], color);

        IEnumerable enumerable = image;
        i = 0;
        foreach (ColorARGB color in enumerable)
            Assert.AreEqual(reference[i++], color);
    }

    [TestMethod]
    public unsafe void Pin()
    {
        Image<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
        ColorARGB[] reference = TestDataHelper.GetPixelData<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

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

    [TestMethod]
    public void InterfaceRowsIterate()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int y = 0;
        foreach (IImageRow row in image.Rows)
        {
            int x = 0;
            foreach (IColor color in row)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x++, y), color);

            y++;
        }

        y = 0;
        foreach (IImageRow row in (IEnumerable)image.Rows)
        {
            int x = 0;
            foreach (IColor color in (IEnumerable)row)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x++, y), color);

            y++;
        }
    }

    [TestMethod]
    public void InterfaceColumnsIterate()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int x = 0;
        foreach (IImageColumn column in image.Columns)
        {
            int y = 0;
            foreach (IColor color in column)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y++), color);

            x++;
        }

        x = 0;
        foreach (IImageColumn column in (IEnumerable)image.Columns)
        {
            int y = 0;
            foreach (IColor color in (IEnumerable)column)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y++), color);

            x++;
        }
    }

    [TestMethod]
    public void InterfaceEnumerate()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
        ColorARGB[] reference = TestDataHelper.GetPixelData<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int i = 0;
        foreach (IColor color in image)
            Assert.AreEqual(reference[i++], color);

        IEnumerable enumerable = image;
        i = 0;
        foreach (IColor color in enumerable)
            Assert.AreEqual(reference[i++], color);
    }

    [TestMethod]
    public void InterfaceColumnsCopyTo()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int x = 0;
        foreach (IImageColumn column in image.Columns)
        {
            IColor[] colors = new IColor[TEST_HEIGHT];
            byte[] bytes = new byte[TEST_HEIGHT * 4];
            column.CopyTo(colors);
            column.CopyTo(bytes);

            int y = 0;
            foreach (IColor color in colors)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y++), color);

            for (y = 0; y < TEST_HEIGHT; y++)
            {
                IColor reference = TestDataHelper.GetColorFromLocation<ColorARGB>(x, y);

                Assert.AreEqual(reference.A, bytes[(y * 4) + 0]);
                Assert.AreEqual(reference.R, bytes[(y * 4) + 1]);
                Assert.AreEqual(reference.G, bytes[(y * 4) + 2]);
                Assert.AreEqual(reference.B, bytes[(y * 4) + 3]);
            }

            x++;
        }
    }

    [TestMethod]
    public void InterfaceColumnsToArray()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int x = 0;
        foreach (IImageColumn column in image.Columns)
        {
            IColor[] data = column.ToArray();

            for (int y = 0; y < TEST_HEIGHT; y++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), data[y]);

            x++;
        }
    }

    [TestMethod]
    public void InterfaceRowsCopyTo()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int y = 0;
        foreach (IImageRow row in image.Rows)
        {
            IColor[] colors = new IColor[TEST_WIDTH];
            byte[] bytes = new byte[TEST_WIDTH * 4];
            row.CopyTo(colors);
            row.CopyTo(bytes);

            int x = 0;
            foreach (IColor color in colors)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x++, y), color);

            for (x = 0; x < TEST_WIDTH; x++)
            {
                IColor reference = TestDataHelper.GetColorFromLocation<ColorARGB>(x, y);

                Assert.AreEqual(reference.A, bytes[(x * 4) + 0]);
                Assert.AreEqual(reference.R, bytes[(x * 4) + 1]);
                Assert.AreEqual(reference.G, bytes[(x * 4) + 2]);
                Assert.AreEqual(reference.B, bytes[(x * 4) + 3]);
            }

            y++;
        }
    }

    [TestMethod]
    public void InterfaceRowsToArray()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        int y = 0;
        foreach (IImageRow row in image.Rows)
        {
            IColor[] data = row.ToArray();

            for (int x = 0; x < TEST_WIDTH; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), data[x]);

            y++;
        }
    }

    #endregion
}