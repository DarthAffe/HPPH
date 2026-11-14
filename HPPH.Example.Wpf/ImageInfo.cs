using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HPPH;
using MediaColor = System.Windows.Media.Color;
using WpfImageExtension = HPPH.WPF.ImageExtension;

namespace HPPHExample;

internal static class ImageInfo
{
    #region Methods

    public static string Describe(IImage image)
        => $"{image.ColorFormat.Name} {image.Width}x{image.Height}, stride {image.Stride}, {image.SizeInBytes:N0} bytes";

    public static string DescribePixel(IImage image, int x, int y)
    {
        if ((x < 0) || (y < 0) || (x >= image.Width) || (y >= image.Height)) return string.Empty;

        IColor color = image[x, y];
        return $"({x}, {y})  {color.GetType().Name}  R {color.R}  G {color.G}  B {color.B}  A {color.A}";
    }

    public static string DescribeColor(IColor color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}  (R {color.R}, G {color.G}, B {color.B}, A {color.A})";

    public static SolidColorBrush ToBrush(IColor color)
    {
        SolidColorBrush brush = new(MediaColor.FromArgb(color.A, color.R, color.G, color.B));
        brush.Freeze();
        return brush;
    }

    public static BitmapSource? ToBitmapSource(IImage image)
        => (image.Width == 0) || (image.Height == 0) ? null : WpfImageExtension.ToBitmapSource(image);

    public static string DescribeRuntime()
    {
        string environment = string.Join(", ", Environment.GetEnvironmentVariables()
                                                          .Cast<DictionaryEntry>()
                                                          .Select(x => $"{x.Key}={x.Value}")
                                                          .Where(x => x.StartsWith("DOTNET_Enable", StringComparison.OrdinalIgnoreCase))
                                                          .Order());

        return $"{RuntimeInformation.FrameworkDescription}"
             + $"  |  Vector512 {Vector512.IsHardwareAccelerated}, Vector256 {Vector256.IsHardwareAccelerated}, Vector128 {Vector128.IsHardwareAccelerated}"
             + $"  |  AVX-512BW {Avx512BW.IsSupported}, AVX2 {Avx2.IsSupported}, SSSE3 {Ssse3.IsSupported}"
             + (environment.Length > 0 ? $"  |  {environment}" : string.Empty);
    }

    #endregion
}
