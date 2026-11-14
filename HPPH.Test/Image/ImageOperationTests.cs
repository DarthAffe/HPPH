namespace HPPH.Test.Image;

[TestClass]
public class ImageOperationTests
{
    #region Operations

    private sealed class ColorTypeOperation : IImageOperation<Type>
    {
        public Type Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => typeof(T);
    }

    private readonly struct FirstPixelOperation : IImageOperation<IColor>
    {
        public IColor Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T> => image[0, 0];
    }

    // A ref struct operation can carry a span, e.g. a destination buffer.
    private readonly ref struct RedChannelOperation(Span<byte> destination) : IImageOperation<int>
    {
        private readonly Span<byte> _destination = destination;

        public int Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T>
        {
            int i = 0;
            foreach (T color in image)
                _destination[i++] = color.R;

            return i;
        }
    }

    #endregion

    #region Methods

    [TestMethod]
    public void OperationGetsActualColorType()
    {
        Assert.AreEqual(typeof(ColorRGB), Execute<ColorRGB>());
        Assert.AreEqual(typeof(ColorBGR), Execute<ColorBGR>());
        Assert.AreEqual(typeof(ColorRGBA), Execute<ColorRGBA>());
        Assert.AreEqual(typeof(ColorBGRA), Execute<ColorBGRA>());
        Assert.AreEqual(typeof(ColorARGB), Execute<ColorARGB>());
        Assert.AreEqual(typeof(ColorABGR), Execute<ColorABGR>());

        static Type Execute<T>()
            where T : unmanaged, IColor<T>
        {
            IImage image = TestDataHelper.CreateTestImage<T>(4, 3);
            return image.Execute<ColorTypeOperation, Type>(new ColorTypeOperation());
        }
    }

    [TestMethod]
    public void OperationSeesSlice()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorBGR>(20, 10).Slice(3, 4, 5, 6);

        IColor first = image.Execute<FirstPixelOperation, IColor>(default);

        Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorBGR>(3, 4), first);
    }

    [TestMethod]
    public void RefStructOperation()
    {
        Image<ColorARGB> typed = TestDataHelper.CreateTestImage<ColorARGB>(20, 10);
        IImage image = typed.Slice(2, 1, 7, 5);
        byte[] reds = new byte[7 * 5];

        int count = image.Execute<RedChannelOperation, int>(new RedChannelOperation(reds));

        Assert.AreEqual(reds.Length, count);
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 7; x++)
                Assert.AreEqual(typed[2 + x, 1 + y].R, reds[(y * 7) + x]);
    }

    [TestMethod]
    public void InferredOverload()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGBA>(20, 10).Slice(3, 4, 5, 6);

        Type type = image.Execute(new ColorTypeOperation());
        IColor first = image.Execute(new FirstPixelOperation());

        Assert.AreEqual(typeof(ColorRGBA), type);
        Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorRGBA>(3, 4), first);
    }

    [TestMethod]
    public void NullOperation()
    {
        IImage image = TestDataHelper.CreateTestImage<ColorRGB>(4, 3);

        Assert.ThrowsExactly<ArgumentNullException>(() => image.Execute<ColorTypeOperation, Type>(null!));
        // ReSharper disable once CanReplaceCastWithShorterTypeArgument - that's intentional here
        Assert.ThrowsExactly<ArgumentNullException>(() => image.Execute((IImageOperation<Type>)null!));
    }

    [TestMethod]
    public void RefImageOperation()
    {
        Image<ColorBGRA> typed = TestDataHelper.CreateTestImage<ColorBGRA>(20, 10);
        RefImage<ColorBGRA> image = typed.AsRefImage()[2, 1, 7, 5];
        byte[] reds = new byte[7 * 5];

        int count = image.Execute<RedChannelOperation, int>(new RedChannelOperation(reds));
        IColor first = image.Execute<FirstPixelOperation, IColor>(default);

        Assert.AreEqual(reds.Length, count);
        Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorBGRA>(2, 1), first);
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 7; x++)
                Assert.AreEqual(typed[2 + x, 1 + y].R, reds[(y * 7) + x]);
    }

    [TestMethod]
    public void RefImageInferredOverload()
    {
        RefImage<ColorABGR> image = TestDataHelper.CreateTestImage<ColorABGR>(20, 10).AsRefImage()[3, 4, 5, 6];

        Type type = image.Execute(new ColorTypeOperation());
        IColor first = image.Execute(new FirstPixelOperation());

        Assert.AreEqual(typeof(ColorABGR), type);
        Assert.AreEqual(TestDataHelper.GetColorFromLocation<ColorABGR>(3, 4), first);
    }

    [TestMethod]
    public void RefImageNullOperation()
    {
        Image<ColorRGB> image = TestDataHelper.CreateTestImage<ColorRGB>(4, 3);

        Assert.ThrowsExactly<ArgumentNullException>(() => image.AsRefImage().Execute<ColorTypeOperation, Type>(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => image.AsRefImage().Execute((IImageOperation<Type>)null!));
    }

    #endregion
}
