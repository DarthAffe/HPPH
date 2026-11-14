namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static Image<T> Flip<T>(RefImage<T> image, ImageFlip flip)
        where T : unmanaged, IColor<T>
    {
        int w = image.Width;
        int h = image.Height;

        bool horizontal = (flip & ImageFlip.Horizontal) != 0;
        bool vertical = (flip & ImageFlip.Vertical) != 0;

        T[] result = new T[w * h];

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                result[(y * w) + x] = image[horizontal ? w - 1 - x : x, vertical ? h - 1 - y : y];

        return Image<T>.Create(result, w, h);
    }

    #endregion
}
