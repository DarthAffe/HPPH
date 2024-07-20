using System.Diagnostics;

namespace HPPH.Test.Image;

[TestClass]
public class RefImageTest
{
    #region Constants

    private const int TEST_WIDTH = 1920;
    private const int TEST_HEIGHT = 1080;

    #endregion

    #region Methods

    [TestMethod]
    public void ImageCreation()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();

        Assert.AreEqual(TEST_WIDTH, image.Width);
        Assert.AreEqual(TEST_HEIGHT, image.Height);

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
    }

    [TestMethod]
    public void ImageInnerFull()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();
        image = image[0, 0, image.Width, image.Height];

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
    }

    [TestMethod]
    public void ImageEnumerator()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();

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

    [TestMethod]
    public void ImageInnerPartial()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();
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
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();
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
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();

        Assert.AreEqual(image.Height, image.Rows.Count);

        for (int y = 0; y < image.Height; y++)
        {
            ImageRow<ColorARGB> row = image.Rows[y];
            Assert.AreEqual(image.Width, row.Length);
            for (int x = 0; x < row.Length; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), row[x]);
        }
    }

    [TestMethod]
    public void ImageRowEnumerator()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();

        int y = 0;
        foreach (ImageRow<ColorARGB> row in image.Rows)
        {
            for (int x = 0; x < row.Length; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), row[x]);

            y++;
        }
    }

    [TestMethod]
    public void ImageColumnIndexer()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();

        Assert.AreEqual(image.Width, image.Columns.Count);

        for (int x = 0; x < image.Width; x++)
        {
            ImageColumn<ColorARGB> column = image.Columns[x];
            Assert.AreEqual(image.Height, column.Length);
            for (int y = 0; y < column.Length; y++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), column[y]);
        }
    }

    [TestMethod]
    public void ImageColumnEnumerator()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();

        int x = 0;
        foreach (ImageColumn<ColorARGB> column in image.Columns)
        {
            for (int y = 0; y < column.Length; y++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), column[y]);

            x++;
        }
    }

    [TestMethod]
    public void ToArray()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();
        ColorARGB[] testData = image.ToArray();

        for (int y = 0; y < TEST_HEIGHT; y++)
            for (int x = 0; x < TEST_WIDTH; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), testData[(y * TEST_WIDTH) + x]);
    }

    [TestMethod]
    public void CopyTo()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();
        ColorARGB[] testData = new ColorARGB[TEST_WIDTH * TEST_HEIGHT];
        image.CopyTo(testData);

        for (int y = 0; y < TEST_HEIGHT; y++)
            for (int x = 0; x < TEST_WIDTH; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), testData[(y * TEST_WIDTH) + x]);
    }

    [TestMethod]
    public void SubImage()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();
        RefImage<ColorARGB> subImage = image[10, 20, 100, 200];

        for (int y = 0; y < 200; y++)
            for (int x = 0; x < 100; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(10 + x, 20 + y), subImage[x, y]);
    }

    [TestMethod]
    public unsafe void Pin()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();
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

    #endregion
}