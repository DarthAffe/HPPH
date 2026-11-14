namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static Image<T> Downscale<T>(RefImage<T> image, int factorX, int factorY)
        where T : unmanaged, IColor<T>
    {
        int w = image.Width;
        int h = image.Height;

        int resultWidth = w == 0 ? 0 : Math.Max(1, w / factorX);
        int resultHeight = h == 0 ? 0 : Math.Max(1, h / factorY);

        int blockWidth = Math.Min(factorX, w);
        int blockHeight = Math.Min(factorY, h);
        long count = (long)blockWidth * blockHeight;

        T[] result = new T[resultWidth * resultHeight];

        for (int y = 0; y < resultHeight; y++)
            for (int x = 0; x < resultWidth; x++)
            {
                long r = 0;
                long g = 0;
                long b = 0;
                long a = 0;

                for (int blockY = 0; blockY < blockHeight; blockY++)
                    for (int blockX = 0; blockX < blockWidth; blockX++)
                    {
                        T color = image[(x * blockWidth) + blockX, (y * blockHeight) + blockY];
                        r += color.R;
                        g += color.G;
                        b += color.B;
                        a += color.A;
                    }

                result[(y * resultWidth) + x] = T.Create(Round(r, count), Round(g, count), Round(b, count), Round(a, count));
            }

        return Image<T>.Create(result, resultWidth, resultHeight);
    }

    #endregion
}
