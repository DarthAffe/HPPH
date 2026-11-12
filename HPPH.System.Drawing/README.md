# HPPH.System.Drawing

System.Drawing extensions for [HPPH](https://github.com/DarthAffe/HPPH).

System.Drawing is only supported on Windows.   
All methods copy the pixels, the resulting images don't depend on the source.

## Loading

```csharp
using HPPH.System.Drawing;

IImage image = ImageHelper.LoadImage("image.png");
Image<ColorRGB> image2 = ImageHelper.LoadImage<ColorRGB>(stream);
```

Without a color format the image is `ColorBGR` for 24 bit files and `ColorBGRA` for everything else.

## Converting

```csharp
// System.Drawing -> HPPH
IImage image = bitmap.ToImage();
Image<ColorRGB> image2 = bitmap.ToImage<ColorRGB>();

// HPPH -> System.Drawing
using Bitmap newBitmap = image.ToBitmap();

byte[] png = image.ToPng();
```

`ToBitmap` creates `Format24bppRgb` for 24 bit and `Format32bppArgb` for 32 bit color formats.   
Bottom-up bitmaps (negative stride) are not supported.
