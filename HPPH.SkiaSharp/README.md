# HPPH.SkiaSharp

[SkiaSharp](https://github.com/mono/SkiaSharp) extensions for [HPPH](https://github.com/DarthAffe/HPPH).

All methods copy the pixels, the resulting images don't depend on the source.   
Premultiplied sources are converted to straight alpha.

## Loading

```csharp
using HPPH.SkiaSharp;

Image<ColorBGRA> image = ImageHelper.LoadImage("image.png");
Image<ColorRGB> image2 = ImageHelper.LoadImage<ColorRGB>(stream);
```

Throws an `InvalidDataException` if SkiaSharp can't decode the data.

## Converting

```csharp
// SkiaSharp -> HPPH
Image<ColorBGRA> image = skImage.ToImage();
Image<ColorRGB> image2 = skBitmap.ToImage<ColorRGB>();

// HPPH -> SkiaSharp
using SKImage newImage = image.ToSKImage();
using SKBitmap newBitmap = image.ToSKBitmap();
using SKBitmap regionBitmap = image[10, 10, 100, 50].ToSKBitmap();

byte[] png = image.ToPng();
```

`ToSKImage` and `ToSKBitmap` create `Rgba8888` for `ColorRGBA`-images and `Bgra8888` for everything else.   
They take an `Image<T>`, `RefImage<T>` or `IImage`, so they work with every color format.
