namespace HPPH.Test.PixelHelper;

[TestClass]
public class ConvertTests
{
    private static IEnumerable<string> GetTestImages() => Directory.EnumerateFiles(@"..\..\..\..\sample_data", "*.png", SearchOption.AllDirectories);

    [TestMethod]
    public void Convert3ByteSameBppRGBToBGR()
    {
        foreach (string image in GetTestImages())
        {
            for (int skip = 0; skip < 4; skip++)
            {
                ColorRGB[] data = ImageHelper.GetColorsFromImage<ColorRGB>(image).SkipLast(skip).ToArray();
                ReadOnlySpan<ColorRGB> referenceData = data;

                Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
                referenceData.CopyTo(sourceData);

                Span<ColorBGR> result = sourceData.Convert<ColorRGB, ColorBGR>();

                Assert.AreEqual(referenceData.Length, result.Length);
                for (int i = 0; i < referenceData.Length; i++)
                {
                    ColorRGB reference = referenceData[i];
                    ColorBGR test = result[i];

                    Assert.AreEqual(reference.R, test.R, $"R differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.G, test.G, $"G differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.B, test.B, $"B differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.A, test.A, $"A differs at index {i}. Image: {image}, skip: {skip}");
                }
            }
        }

        IImage<ColorBGR> converted = TestDataHelper.CreateTestImage<ColorRGB>(1920, 1080).ConvertTo<ColorBGR>();

        for (int y = 0; y < converted.Height; y++)
            for (int x = 0; x < converted.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorBGR>(x, y), converted[x, y], $"Wrong color at x: {x}, y: {y}");
    }

    [TestMethod]
    public void Convert4ByteSameBppRGBAToARGB()
    {
        foreach (string image in GetTestImages())
        {
            for (int skip = 0; skip < 4; skip++)
            {
                ColorRGBA[] data = ImageHelper.GetColorsFromImage<ColorRGBA>(image).SkipLast(skip).ToArray();
                ReadOnlySpan<ColorRGBA> referenceData = data;

                Span<ColorRGBA> sourceData = new ColorRGBA[referenceData.Length];
                referenceData.CopyTo(sourceData);

                Span<ColorARGB> result = sourceData.Convert<ColorRGBA, ColorARGB>();

                Assert.AreEqual(referenceData.Length, result.Length);
                for (int i = 0; i < referenceData.Length; i++)
                {
                    ColorRGBA reference = referenceData[i];
                    ColorARGB test = result[i];

                    Assert.AreEqual(reference.R, test.R, $"R differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.G, test.G, $"G differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.B, test.B, $"B differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.A, test.A, $"A differs at index {i}. Image: {image}, skip: {skip}");
                }
            }
        }

        IImage<ColorARGB> converted = TestDataHelper.CreateTestImage<ColorRGBA>(1920, 1080).ConvertTo<ColorARGB>();

        for (int y = 0; y < converted.Height; y++)
            for (int x = 0; x < converted.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorARGB>(x, y), converted[x, y], $"Wrong color at x: {x}, y: {y}");
    }

    [TestMethod]
    public void Convert4ByteSameBppRGBAToBGRA()
    {
        foreach (string image in GetTestImages())
        {
            for (int skip = 0; skip < 4; skip++)
            {
                ColorRGBA[] data = ImageHelper.GetColorsFromImage<ColorRGBA>(image).SkipLast(skip).ToArray();
                ReadOnlySpan<ColorRGBA> referenceData = data;

                Span<ColorRGBA> sourceData = new ColorRGBA[referenceData.Length];
                referenceData.CopyTo(sourceData);

                Span<ColorBGRA> result = sourceData.Convert<ColorRGBA, ColorBGRA>();

                Assert.AreEqual(referenceData.Length, result.Length);
                for (int i = 0; i < referenceData.Length; i++)
                {
                    ColorRGBA reference = referenceData[i];
                    ColorBGRA test = result[i];

                    Assert.AreEqual(reference.R, test.R, $"R differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.G, test.G, $"G differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.B, test.B, $"B differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.A, test.A, $"A differs at index {i}. Image: {image}, skip: {skip}");
                }
            }
        }

        IImage<ColorBGRA> converted = TestDataHelper.CreateTestImage<ColorRGBA>(1920, 1080).ConvertTo<ColorBGRA>();

        for (int y = 0; y < converted.Height; y++)
            for (int x = 0; x < converted.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorBGRA>(x, y), converted[x, y], $"Wrong color at x: {x}, y: {y}");
    }

    [TestMethod]
    public void ConvertNarrow4ByteRGBAToRGB()
    {
        foreach (string image in GetTestImages())
        {
            for (int skip = 0; skip < 4; skip++)
            {
                ColorRGBA[] data = ImageHelper.GetColorsFromImage<ColorRGBA>(image).SkipLast(skip).ToArray();
                ReadOnlySpan<ColorRGBA> referenceData = data;

                Span<ColorRGBA> sourceData = new ColorRGBA[referenceData.Length];
                referenceData.CopyTo(sourceData);

                Span<ColorRGB> result = sourceData.Convert<ColorRGBA, ColorRGB>();

                Assert.AreEqual(referenceData.Length, result.Length);
                for (int i = 0; i < referenceData.Length; i++)
                {
                    ColorRGBA reference = referenceData[i];
                    ColorRGB test = result[i];

                    Assert.AreEqual(reference.R, test.R, $"R differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.G, test.G, $"G differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.B, test.B, $"B differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(255, test.A, $"A differs at index {i}. Image: {image}, skip: {skip}");
                }
            }
        }

        IImage<ColorRGB> converted = TestDataHelper.CreateTestImage<ColorRGBA>(1920, 1080).ConvertTo<ColorRGB>();

        for (int y = 0; y < converted.Height; y++)
            for (int x = 0; x < converted.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorRGB>(x, y), converted[x, y], $"Wrong color at x: {x}, y: {y}");
    }

    [TestMethod]
    public void ConvertNarrow4ByteRGBAToBGR()
    {
        foreach (string image in GetTestImages())
        {
            for (int skip = 0; skip < 4; skip++)
            {
                ColorRGBA[] data = ImageHelper.GetColorsFromImage<ColorRGBA>(image).SkipLast(skip).ToArray();
                ReadOnlySpan<ColorRGBA> referenceData = data;

                Span<ColorRGBA> sourceData = new ColorRGBA[referenceData.Length];
                referenceData.CopyTo(sourceData);

                Span<ColorBGR> result = sourceData.Convert<ColorRGBA, ColorBGR>();

                Assert.AreEqual(referenceData.Length, result.Length);
                for (int i = 0; i < referenceData.Length; i++)
                {
                    ColorRGBA reference = referenceData[i];
                    ColorBGR test = result[i];

                    Assert.AreEqual(reference.R, test.R, $"R differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.G, test.G, $"G differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.B, test.B, $"B differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(255, test.A, $"A differs at index {i}. Image: {image}, skip: {skip}");
                }
            }
        }

        IImage<ColorBGR> converted = TestDataHelper.CreateTestImage<ColorRGBA>(1920, 1080).ConvertTo<ColorBGR>();

        for (int y = 0; y < converted.Height; y++)
            for (int x = 0; x < converted.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorBGR>(x, y), converted[x, y], $"Wrong color at x: {x}, y: {y}");
    }

    [TestMethod]
    public void ConvertWiden3ByteRGBToRGBA()
    {
        foreach (string image in GetTestImages())
        {
            for (int skip = 0; skip < 4; skip++)
            {
                ColorRGB[] data = ImageHelper.GetColorsFromImage<ColorRGB>(image).SkipLast(skip).ToArray();
                ReadOnlySpan<ColorRGB> referenceData = data;

                Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
                referenceData.CopyTo(sourceData);

                Span<ColorRGBA> result = sourceData.Convert<ColorRGB, ColorRGBA>();

                Assert.AreEqual(referenceData.Length, result.Length);
                for (int i = 0; i < referenceData.Length; i++)
                {
                    ColorRGB reference = referenceData[i];
                    ColorRGBA test = result[i];

                    Assert.AreEqual(reference.R, test.R, $"R differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.G, test.G, $"G differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.B, test.B, $"B differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.A, test.A, $"A differs at index {i}. Image: {image}, skip: {skip}");
                }
            }
        }

        IImage<ColorRGBA> converted = TestDataHelper.CreateTestImage<ColorRGB>(1920, 1080).ConvertTo<ColorRGBA>();

        for (int y = 0; y < converted.Height; y++)
            for (int x = 0; x < converted.Width; x++)
            {
                ColorABGR refColor = TestDataHelper.GetColorFromLocation<ColorABGR>(x, y);
                ColorRGBA color = converted[x, y];

                Assert.AreEqual(255, color.A, $"Wrong A at x: {x}, y: {y}");
                Assert.AreEqual(refColor.R, color.R, $"Wrong R at x: {x}, y: {y}");
                Assert.AreEqual(refColor.G, color.G, $"Wrong G at x: {x}, y: {y}");
                Assert.AreEqual(refColor.B, color.B, $"Wrong B at x: {x}, y: {y}");
            }
    }

    [TestMethod]
    public void ConvertWiden3ByteRGBToARGB()
    {
        foreach (string image in GetTestImages())
        {
            for (int skip = 0; skip < 4; skip++)
            {
                ColorRGB[] data = ImageHelper.GetColorsFromImage<ColorRGB>(image).SkipLast(skip).ToArray();
                ReadOnlySpan<ColorRGB> referenceData = data;

                Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
                referenceData.CopyTo(sourceData);

                Span<ColorARGB> result = sourceData.Convert<ColorRGB, ColorARGB>();

                Assert.AreEqual(referenceData.Length, result.Length);
                for (int i = 0; i < referenceData.Length; i++)
                {
                    ColorRGB reference = referenceData[i];
                    ColorARGB test = result[i];

                    Assert.AreEqual(reference.R, test.R, $"R differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.G, test.G, $"G differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.B, test.B, $"B differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.A, test.A, $"A differs at index {i}. Image: {image}, skip: {skip}");
                }
            }
        }

        IImage<ColorARGB> converted = TestDataHelper.CreateTestImage<ColorRGB>(1920, 1080).ConvertTo<ColorARGB>();

        for (int y = 0; y < converted.Height; y++)
            for (int x = 0; x < converted.Width; x++)
            {
                ColorABGR refColor = TestDataHelper.GetColorFromLocation<ColorABGR>(x, y);
                ColorARGB color = converted[x, y];

                Assert.AreEqual(255, color.A, $"Wrong A at x: {x}, y: {y}");
                Assert.AreEqual(refColor.R, color.R, $"Wrong R at x: {x}, y: {y}");
                Assert.AreEqual(refColor.G, color.G, $"Wrong G at x: {x}, y: {y}");
                Assert.AreEqual(refColor.B, color.B, $"Wrong B at x: {x}, y: {y}");
            }
    }

    [TestMethod]
    public void ConvertWiden3ByteRGBToBGRA()
    {
        foreach (string image in GetTestImages())
        {
            for (int skip = 0; skip < 4; skip++)
            {
                ColorRGB[] data = ImageHelper.GetColorsFromImage<ColorRGB>(image).SkipLast(skip).ToArray();
                ReadOnlySpan<ColorRGB> referenceData = data;

                Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
                referenceData.CopyTo(sourceData);

                Span<ColorBGRA> result = sourceData.Convert<ColorRGB, ColorBGRA>();

                Assert.AreEqual(referenceData.Length, result.Length);
                for (int i = 0; i < referenceData.Length; i++)
                {
                    ColorRGB reference = referenceData[i];
                    ColorBGRA test = result[i];

                    Assert.AreEqual(reference.R, test.R, $"R differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.G, test.G, $"G differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.B, test.B, $"B differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.A, test.A, $"A differs at index {i}. Image: {image}, skip: {skip}");
                }
            }
        }

        IImage<ColorBGRA> converted = TestDataHelper.CreateTestImage<ColorRGB>(1920, 1080).ConvertTo<ColorBGRA>();

        for (int y = 0; y < converted.Height; y++)
            for (int x = 0; x < converted.Width; x++)
            {
                ColorABGR refColor = TestDataHelper.GetColorFromLocation<ColorABGR>(x, y);
                ColorBGRA color = converted[x, y];

                Assert.AreEqual(255, color.A, $"Wrong A at x: {x}, y: {y}");
                Assert.AreEqual(refColor.R, color.R, $"Wrong R at x: {x}, y: {y}");
                Assert.AreEqual(refColor.G, color.G, $"Wrong G at x: {x}, y: {y}");
                Assert.AreEqual(refColor.B, color.B, $"Wrong B at x: {x}, y: {y}");
            }
    }

    [TestMethod]
    public void ConvertWiden3ByteRGBToABGR()
    {
        foreach (string image in GetTestImages())
        {
            for (int skip = 0; skip < 4; skip++)
            {
                ColorRGB[] data = ImageHelper.GetColorsFromImage<ColorRGB>(image).SkipLast(skip).ToArray();
                ReadOnlySpan<ColorRGB> referenceData = data;

                Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
                referenceData.CopyTo(sourceData);

                Span<ColorABGR> result = sourceData.Convert<ColorRGB, ColorABGR>();

                Assert.AreEqual(referenceData.Length, result.Length);
                for (int i = 0; i < referenceData.Length; i++)
                {
                    ColorRGB reference = referenceData[i];
                    ColorABGR test = result[i];

                    Assert.AreEqual(reference.R, test.R, $"R differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.G, test.G, $"G differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.B, test.B, $"B differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.A, test.A, $"A differs at index {i}. Image: {image}, skip: {skip}");
                }
            }
        }

        IImage<ColorABGR> converted = TestDataHelper.CreateTestImage<ColorRGB>(1920, 1080).ConvertTo<ColorABGR>();

        for (int y = 0; y < converted.Height; y++)
            for (int x = 0; x < converted.Width; x++)
            {
                ColorABGR refColor = TestDataHelper.GetColorFromLocation<ColorABGR>(x, y);
                ColorABGR color = converted[x, y];

                Assert.AreEqual(255, color.A, $"Wrong A at x: {x}, y: {y}");
                Assert.AreEqual(refColor.R, color.R, $"Wrong R at x: {x}, y: {y}");
                Assert.AreEqual(refColor.G, color.G, $"Wrong G at x: {x}, y: {y}");
                Assert.AreEqual(refColor.B, color.B, $"Wrong B at x: {x}, y: {y}");
            }
    }

    [TestMethod]
    public void Convert3ByteSameBppRGBToBGRReadOnlySpan()
    {
        foreach (string image in GetTestImages())
        {
            for (int skip = 0; skip < 4; skip++)
            {
                ColorRGB[] data = ImageHelper.GetColorsFromImage<ColorRGB>(image).SkipLast(skip).ToArray();
                ReadOnlySpan<ColorRGB> referenceData = data;

                Span<ColorRGB> sourceData = new ColorRGB[referenceData.Length];
                referenceData.CopyTo(sourceData);

                Span<ColorBGR> result = ((ReadOnlySpan<ColorRGB>)sourceData).Convert<ColorRGB, ColorBGR>();

                Assert.AreEqual(referenceData.Length, result.Length);
                for (int i = 0; i < referenceData.Length; i++)
                {
                    ColorRGB reference = referenceData[i];
                    ColorBGR test = result[i];

                    Assert.AreEqual(reference.R, test.R, $"R differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.G, test.G, $"G differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.B, test.B, $"B differs at index {i}. Image: {image}, skip: {skip}");
                    Assert.AreEqual(reference.A, test.A, $"A differs at index {i}. Image: {image}, skip: {skip}");
                }
            }
        }

        IImage<ColorBGR> converted = TestDataHelper.CreateTestImage<ColorRGB>(1920, 1080).ConvertTo<ColorBGR>();

        for (int y = 0; y < converted.Height; y++)
            for (int x = 0; x < converted.Width; x++)
                Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorBGR>(x, y), converted[x, y], $"Wrong color at x: {x}, y: {y}");
    }
}