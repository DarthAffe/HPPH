# HPPH.WPF

WPF extensions for [HPPH](https://github.com/DarthAffe/HPPH).

Only uses what WPF already contains, so no further dependencies are needed in WPF applications.   
All methods copy the pixels, the resulting images don't depend on the source.

## Loading

```csharp
using HPPH.WPF;

IImage image = ImageHelper.LoadImage("image.png");
Image<ColorRGB> image2 = ImageHelper.LoadImage<ColorRGB>(stream);
```

Without a color format the image is `ColorBGR` or `ColorRGB` if WPF decodes the file to 24 bit and `ColorBGRA` for everything else.   
Embedded color profiles are ignored.

## Converting

```csharp
// WPF -> HPPH
IImage image = bitmapSource.ToImage();
Image<ColorRGB> image2 = bitmapSource.ToImage<ColorRGB>();

// HPPH -> WPF
BitmapSource bitmap = image.ToBitmapSource();             // frozen, usable on every thread
BitmapSource regionBitmap = image[10, 10, 100, 50].ToBitmapSource();
WriteableBitmap writeable = image.ToWriteableBitmap();

byte[] png = image.ToPng();
```

`ToBitmapSource` and `ToWriteableBitmap` create `Rgb24` for `ColorRGB`, `Bgr24` for `ColorBGR` and `Bgra32` for 4-byte color formats.   
Other pixel formats, premultiplied ones included, are converted to `Bgra32` by WPF before they are read.

## Updating a WriteableBitmap

`CopyTo` writes into an existing `WriteableBitmap` without allocating, for example to show a frequently changing image.

```csharp
image.CopyTo(writeable);
```

The bitmap needs the size of the image and one of the pixel formats `Bgra32`, `Bgr32`, `Bgr24` or `Rgb24`.   
Like every access to a `WriteableBitmap` it has to happen on the thread the bitmap belongs to.
