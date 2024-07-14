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
    public void TestImageCreation()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);

        Assert.AreEqual(TEST_WIDTH, image.Width);
        Assert.AreEqual(TEST_HEIGHT, image.Height);

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
    }

    [TestMethod]
    public void TestImageInnerFull()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
        image = image[0, 0, image.Width, image.Height];

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
    }

    [TestMethod]
    public void TestImageEnumerator()
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
    public void TestImageInnerPartial()
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
    public void TestImageInnerInnerPartial()
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
    public void TestImageRowIndexer()
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
    public void TestImageRowEnumerator()
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
    public void TestImageColumnIndexer()
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
    public void TestImageColumnEnumerator()
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
    public void TestAsRefImage()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT);
        image = image[163, 280, 720, 13];
        image = image[15, 2, 47, 8];

        RefImage<ColorARGB> refImage = image.AsRefImage<ColorARGB>();

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(image[x, y], refImage[x, y]);
    }

    #endregion
}