namespace HPPH.Reference;

public static partial class ReferencePixelHelper
{
    #region Methods

    public static Image<T> Rotate<T>(RefImage<T> image, ImageRotation rotation)
        where T : unmanaged, IColor<T>
    {
        int w = image.Width;
        int h = image.Height;

        bool swapEdges = rotation is ImageRotation.Rotate90 or ImageRotation.Rotate270;
        int resultWidth = swapEdges ? h : w;
        int resultHeight = swapEdges ? w : h;

        T[] result = new T[w * h];

        for (int y = 0; y < resultHeight; y++)
            for (int x = 0; x < resultWidth; x++)
                result[(y * resultWidth) + x] = rotation switch
                {
                    ImageRotation.Rotate90 => image[y, h - 1 - x],
                    ImageRotation.Rotate180 => image[w - 1 - x, h - 1 - y],
                    ImageRotation.Rotate270 => image[w - 1 - y, x],
                    _ => image[x, y]
                };

        return Image<T>.Create(result, resultWidth, resultHeight);
    }

    #endregion
}
