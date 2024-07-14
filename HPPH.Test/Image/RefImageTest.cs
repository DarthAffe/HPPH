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
    public void TestImageCreation()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();

        Assert.AreEqual(TEST_WIDTH, image.Width);
        Assert.AreEqual(TEST_HEIGHT, image.Height);

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
    }

    [TestMethod]
    public void TestImageInnerFull()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();
        image = image[0, 0, image.Width, image.Height];

        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), image[x, y]);
    }

    [TestMethod]
    public void TestImageEnumerator()
    {
        RefImage<ColorARGB> image = TestDataHelper.CreateTestImage<ColorARGB>(TEST_WIDTH, TEST_HEIGHT).AsRefImage();

        int counter = 0;
        foreach (ColorARGB color in image)
        {
            int x = counter % image.Width;
            int y = counter / image.Width;

            if(y == 1) Debugger.Break();

            Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), color);

            counter++;
        }
    }

    [TestMethod]
    public void TestImageInnerPartial()
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
    public void TestImageInnerInnerPartial()
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
    public void TestImageRowIndexer()
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
    public void TestImageRowEnumerator()
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
    public void TestImageColumnIndexer()
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
    public void TestImageColumnEnumerator()
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

    #endregion
}