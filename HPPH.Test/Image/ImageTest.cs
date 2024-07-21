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
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int counter = 0;
            foreach (IColor color in image)
            {
                int x = counter % image.Width;
                int y = counter / image.Width;

                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), color);

                counter++;
            }
        }
    }

    [TestMethod]
    public void ImageInnerPartial()
    {
        (int width, int height) = SIZES[0];

        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
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

        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
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
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            Assert.AreEqual(image.Height, image.Rows.Count);

            for (int y = 0; y < image.Height; y++)
            {
                IImageRow row = image.Rows[y];
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

        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
        image = image[163, 280, 720, 13];

        Assert.AreEqual(image.Height, image.Rows.Count);

        for (int y = 0; y < image.Height; y++)
        {
            IImageRow row = image.Rows[y];
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
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int y = 0;
            foreach (IImageRow row in image.Rows)
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
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            Assert.AreEqual(image.Width, image.Columns.Count);

            for (int x = 0; x < image.Width; x++)
            {
                IImageColumn column = image.Columns[x];
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

        IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
        image = image[163, 280, 720, 13];

        Assert.AreEqual(image.Width, image.Columns.Count);

        for (int x = 0; x < image.Width; x++)
        {
            IImageColumn column = image.Columns[x];
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
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int x = 0;
            foreach (IImageColumn column in image.Columns)
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
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

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
    }

    [TestMethod]
    public void ConvertToCopy()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

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
    public void InterfaceRowsIterate()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

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
    }

    [TestMethod]
    public void InterfaceColumnsIterate()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

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
    }

    [TestMethod]
    public void InterfaceEnumerate()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);
            ColorARGB[] reference = TestDataHelper.GetPixelData<ColorARGB>(width, height);

            int i = 0;
            foreach (IColor color in image)
                Assert.AreEqual(reference[i++], color);

            IEnumerable enumerable = image;
            i = 0;
            foreach (IColor color in enumerable)
                Assert.AreEqual(reference[i++], color);
        }
    }

    [TestMethod]
    public void InterfaceColumnsCopyTo()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int x = 0;
            foreach (IImageColumn column in image.Columns)
            {
                IColor[] colors = new IColor[height];
                byte[] bytes = new byte[height * 4];
                column.CopyTo(colors);
                column.CopyTo(bytes);

                int y = 0;
                foreach (IColor color in colors)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y++), color);

                for (y = 0; y < height; y++)
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
    }

    [TestMethod]
    public void InterfaceColumnsToArray()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int x = 0;
            foreach (IImageColumn column in image.Columns)
            {
                IColor[] data = column.ToArray();

                for (int y = 0; y < height; y++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), data[y]);

                x++;
            }
        }
    }

    [TestMethod]
    public void InterfaceRowsCopyTo()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int y = 0;
            foreach (IImageRow row in image.Rows)
            {
                IColor[] colors = new IColor[width];
                byte[] bytes = new byte[width * 4];
                row.CopyTo(colors);
                row.CopyTo(bytes);

                int x = 0;
                foreach (IColor color in colors)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x++, y), color);

                for (x = 0; x < width; x++)
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
    }

    [TestMethod]
    public void InterfaceRowsToArray()
    {
        foreach ((int width, int height) in SIZES)
        {
            IImage image = TestDataHelper.CreateTestImage<ColorARGB>(width, height);

            int y = 0;
            foreach (IImageRow row in image.Rows)
            {
                IColor[] data = row.ToArray();

                for (int x = 0; x < width; x++)
                    Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), data[x]);

                y++;
            }
        }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateWrongStride()
    {
        Image<ColorRGB>.Create([], 10, 20, 9);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateWrongBufferSize()
    {
        Image<ColorRGB>.Create(new byte[299], 10, 10, 30);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CopyToByteNull()
    {
        TestDataHelper.CreateTestImage<ColorRGB>(10, 10).CopyTo((Span<byte>)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CopyToColorNull()
    {
        TestDataHelper.CreateTestImage<ColorRGB>(10, 10).CopyTo((Span<ColorRGB>)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CopyToByteWrongSize()
    {
        TestDataHelper.CreateTestImage<ColorRGB>(10, 10).CopyTo(new byte[(10 * 10 * 3) - 1]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CopyToColorWrongSize()
    {
        TestDataHelper.CreateTestImage<ColorRGB>(10, 10).CopyTo(new ColorRGB[(10 * 10) - 1]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void AsRefImageWrongType()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 10);
        RefImage<ColorBGR> refImage = image.AsRefImage<ColorBGR>();
    }

    [TestMethod]
    public void PinEmpty()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(0, 0);
        Assert.IsTrue(Unsafe.IsNullRef(in image.GetPinnableReference()));
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void IndexerWrongXBig()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        ColorRGB test = image[10, 19];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void IndexerWrongYBig()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        ColorRGB test = image[9, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void IndexerWrongXSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        ColorRGB test = image[-1, 19];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void IndexerWrongYSmall()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        ColorRGB test = image[9, -1];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageWrongX()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        RefImage<ColorRGB> test = image[-1, 0, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageWrongY()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        RefImage<ColorRGB> test = image[0, -1, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageWrongWidth()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        RefImage<ColorRGB> test = image[0, 0, 0, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageWrongHeight()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        RefImage<ColorRGB> test = image[0, 0, 10, 0];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageInvalidSizeWidth()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        RefImage<ColorRGB> test = image[1, 0, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageInvalidSizeHeight()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        RefImage<ColorRGB> test = image[0, 1, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageInterfaceWrongX()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImage test = image[-1, 0, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageInterfaceWrongY()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImage test = image[0, -1, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageInterfaceWrongWidth()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImage test = image[0, 0, 0, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageInterfaceWrongHeight()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImage test = image[0, 0, 10, 0];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageInterfaceInvalidSizeWidth()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImage test = image[1, 0, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SubImageInterfaceInvalidSizeHeight()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImage test = image[0, 1, 10, 20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void ColumnsIndexerToBig()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImageColumn test = image.Columns[20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void ColumnsIndexerToSmall()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImageColumn test = image.Columns[-1];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void RowsIndexerToBig()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImageRow test = image.Rows[20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void RowsIndexerToSmall()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImageRow test = image.Rows[-1];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void ColumnIndexerToBig()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IColor test = image.Columns[1][20];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void ColumnIndexerToSmall()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IColor test = image.Columns[1][-1];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void RowIndexerToBig()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IColor test = image.Rows[1][10];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void RowIndexerToSmall()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IColor test = image.Rows[1][-1];
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ColumnCopyToByteNull()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        image.Columns[1].CopyTo((Span<byte>)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ColumnCopyToColorNull()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        image.Columns[1].CopyTo((Span<IColor>)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ColumnCopyToByteToSmall()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        image.Columns[1].CopyTo(new byte[(20 * 3) - 1]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ColumnCopyToColorToSmall()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        image.Columns[1].CopyTo(new IColor[19]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RowCopyToByteNull()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        image.Rows[1].CopyTo((Span<byte>)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RowCopyToColorNull()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        image.Rows[1].CopyTo((Span<IColor>)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void RowCopyToByteToSmall()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        image.Rows[1].CopyTo(new byte[(10 * 3) - 1]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void RowCopyToColorToSmall()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        image.Rows[1].CopyTo(new IColor[9]);
    }

    [TestMethod]
    public void EqualsIImage()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        IImage image2 = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.AreEqual(image, image2);

        Assert.IsFalse(image.Equals((IImage?)null));
    }

    [TestMethod]
    public void EqualsGenericIImage()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        IImage<ColorRGB> image2 = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.AreEqual(image, image2);

        Assert.IsFalse(image.Equals((IImage<ColorRGB>?)null));
    }

    [TestMethod]
    public void EqualsImage()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        Image<ColorRGB> image2 = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        Assert.AreEqual(image, image2);

        Assert.IsFalse(image.Equals((Image<ColorRGB>?)null));
    }

    [TestMethod]
    public void EqualsIImageWrongSize()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        IImage image2 = TestDataHelper.CreateTestImage<ColorRGB>(10, 10);
        Assert.AreNotEqual(image, image2);

        IImage image3 = TestDataHelper.CreateTestImage<ColorRGB>(20, 20);
        Assert.AreNotEqual(image, image3);
    }

    [TestMethod]
    public void EqualsGenericIImageWrongSize()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        IImage<ColorRGB> image2 = TestDataHelper.CreateTestImage<ColorRGB>(10, 10);
        Assert.AreNotEqual(image, image2);

        IImage<ColorRGB> image3 = TestDataHelper.CreateTestImage<ColorRGB>(20, 20);
        Assert.AreNotEqual(image, image3);
    }

    [TestMethod]
    public void EqualsImageWrongSize()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        Image<ColorRGB> image2 = TestDataHelper.CreateTestImage<ColorRGB>(10, 10);
        Assert.AreNotEqual(image, image2);

        Image<ColorRGB> image3 = TestDataHelper.CreateTestImage<ColorRGB>(20, 20);
        Assert.AreNotEqual(image, image3);
    }

    [TestMethod]
    public void EqualsIImageWrongPixel()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        ColorRGB[] array = image.ToArray();
        array[1] = new ColorRGB(255, 255, 255);
        IImage image2 = Image<ColorRGB>.Create(array, 10, 20);

        Assert.AreNotEqual(image, image2);
    }

    [TestMethod]
    public void EqualsGenericIImageWrongPixel()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        ColorRGB[] array = image.ToArray();
        array[1] = new ColorRGB(255, 255, 255);
        IImage<ColorRGB> image2 = Image<ColorRGB>.Create(array, 10, 20);

        Assert.AreNotEqual(image, image2);
    }

    [TestMethod]
    public void EqualsImageWrongPixel()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);

        ColorRGB[] array = image.ToArray();
        array[1] = new ColorRGB(255, 255, 255);
        Image<ColorRGB> image2 = Image<ColorRGB>.Create(array, 10, 20);

        Assert.AreNotEqual(image, image2);
    }

    [TestMethod]
    public void EqualsIImageWrongFormat()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(10, 20);
        IImage image2 = TestDataHelper.CreateTestImage<ColorBGR>(10, 20);

        Assert.AreNotEqual(image, image2);
    }

    #endregion
}