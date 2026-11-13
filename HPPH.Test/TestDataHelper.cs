namespace HPPH.Test;

internal static class TestDataHelper
{
    public static T GetColorFromLocation<T>(int x, int y)
        where T : unmanaged, IColor<T>
    {
        byte[] xBytes = BitConverter.GetBytes((short)x);
        byte[] yBytes = BitConverter.GetBytes((short)y);
        return T.Create(xBytes[0], xBytes[1], yBytes[0], yBytes[1]);
    }

    public static T[] GetPixelData<T>(int width, int height)
        where T : unmanaged, IColor<T>
    {
        T[] buffer = new T[width * height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                buffer[(y * width) + x] = GetColorFromLocation<T>(x, y);

        return buffer;
    }

    public static Image<T> CreateTestImage<T>(int width, int height)
        where T : unmanaged, IColor<T>
        => Image<T>.Create(GetPixelData<T>(width, height), width, height);
}