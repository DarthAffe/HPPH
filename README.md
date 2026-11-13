# HPPH

C# High Performance Pixel Handling

## Summary

Helpers to work with 24 and 32 bit color data, optimized for performance and written fully in C#.   
Most operations are allocation free and use SIMD (up to AVX-512) where available.

| Package                                                                   | Description                                                                          |
|---------------------------------------------------------------------------|--------------------------------------------------------------------------------------|
| [HPPH](https://www.nuget.org/packages/HPPH)                               | Core package, contains everything described below                                    |
| [HPPH.SkiaSharp](https://www.nuget.org/packages/HPPH.SkiaSharp)           | Loading of images and conversion from and to SkiaSharp images and bitmaps            |
| [HPPH.System.Drawing](https://www.nuget.org/packages/HPPH.System.Drawing) | Loading of images and conversion from and to System.Drawing bitmaps (Windows only)  |

Supports .NET 10 and .NET 11.

## Colors

| Type                                                 | Size   |
|------------------------------------------------------|--------|
| `ColorRGB`, `ColorBGR`                               | 24 bit |
| `ColorARGB`, `ColorABGR`, `ColorRGBA`, `ColorBGRA`   | 32 bit |

All colors implement `IColor<T>` and expose `R`, `G`, `B` and `A`. 24 bit colors always return 255 for `A`.   
The format of a color type is available through `T.ColorFormat` or the instances on `IColorFormat`.

```csharp
ColorRGBA color = new(255, 128, 0, 255);
IColorFormat format = ColorRGBA.ColorFormat; // same as IColorFormat.RGBA
```

## Images

`Image<T>` is a class holding its pixels in a byte array.   
`RefImage<T>` is a ref struct that works on top of any span (arrays, `stackalloc`, native memory).   
Both can be a view on a region of a bigger buffer, so `Stride` (bytes per row) is not always `Width * BytesPerPixel`.

`IImage` is used where the color format is only known at runtime. `Image<T>` is the only implementation.

### Creating

```csharp
// copies the data
Image<ColorRGB> image = Image<ColorRGB>.Create(colors, width, height);
Image<ColorBGRA> image2 = Image<ColorBGRA>.Create(bytes, width, height, stride);

// uses the buffer without copying, changes to it show up in the image
Image<ColorBGRA> wrapped = Image<ColorBGRA>.Wrap(bytes, width, height, stride);
RefImage<ColorBGRA> refImage = RefImage<ColorBGRA>.Wrap(span, width, height, stride);

// color format only known at runtime
IImage image3 = format.CreateImage(bytes, width, height, stride);
IImage wrapped2 = format.WrapImage(bytes, width, height, stride);
```

### Accessing pixels

```csharp
ColorRGB pixel = image[x, y];

foreach (ColorRGB color in image) { }                       // row by row
foreach (ImageRow<ColorRGB> row in image.Rows) { }          // row.AsSpan() for a ReadOnlySpan<ColorRGB>
foreach (ImageColumn<ColorRGB> column in image.Columns) { }

ColorRGB[] firstRow = image.Rows[0].ToArray();
```

### Regions

Regions share the data of the image, nothing is copied.   
A slice keeps the whole buffer of its image alive. Use `ToImage` to get an independent copy, for example before storing a small region of a big image.

```csharp
RefImage<ColorRGB> region = image[10, 10, 100, 50];   // allocation free
Image<ColorRGB> slice = image.Slice(10, 10, 100, 50); // can be stored in a field
Image<ColorRGB> copy = region.ToImage();              // independent copy
Image<ColorRGB> copy2 = slice.ToImage();              // same for slices
```

### Copying and converting

```csharp
ColorRGB[] pixels = image.ToArray();
byte[] raw = image.ToRawArray();      // rows without padding
image.CopyTo(buffer);
image.CopyTo(buffer, stride);         // rows are written stride bytes apart

Image<ColorBGRA> bgra = image.ConvertTo<ColorBGRA>();
image.ConvertTo<ColorBGRA>(buffer, stride);
```

### Comparing

`Equals` checks for reference equality. Use `ContentEquals` to compare format, size and pixels. Row padding is ignored.

```csharp
bool equal = image.ContentEquals(other);
```

### Custom operations

To work with an `IImage` without knowing its color format, implement `IImageOperation<TResult>` and pass it to `Execute`.

```csharp
readonly struct MaxRed : IImageOperation<byte>
{
    public byte Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T>
    {
        byte max = 0;
        foreach (T color in image)
            max = Math.Max(max, color.R);

        return max;
    }
}

byte maxRed = image.Execute<MaxRed, byte>(default); // no allocations
byte maxRed2 = image.Execute(new MaxRed());         // shorter, but boxes struct operations
```

`Image<T>` and `RefImage<T>` offer the same `Execute` methods.

## Operations

All operations are extension methods in `PixelHelper`.   
Sum, average, min-max and palettes work on `Image<T>`, `RefImage<T>`, `IImage` and `ReadOnlySpan<T>`. Sorting and conversion work on spans.

### Sum

Sums up each channel into a `long`.

```csharp
ColorSum sum = image.Sum();
long red = sum.R;
```

### Average

```csharp
ColorRGB average = image.Average();
IColor average2 = someIImage.Average();
```

### Min-Max

Gets the minimum and maximum of each channel.

```csharp
ColorMinMax minMax = image.MinMax();
byte redRange = minMax.RedRange; // RedMax - RedMin
```

### Sort

Sorts in place by a single channel. The sort is stable.

```csharp
Span<ColorRGB> colors = image.ToArray();
colors.SortByRed();
colors.SortByGreen();
colors.SortByBlue();
colors.SortByAlpha();
```

### Color palettes

Reduces the colors to a palette of the given size.

```csharp
ColorRGB[] palette = image.CreateColorPalette(16);

// pixelCounts[i] is the number of pixels represented by palette[i]
Span<long> pixelCounts = stackalloc long[16];
ColorRGB[] palette2 = image.CreateColorPalette(16, pixelCounts);
```

`CreateColorPalette` uses Wu's algorithm. It's higher quality and in most cases faster than median cut.   
To pick one explicitly use `CreateWuColorPalette` or `CreateMedianCutColorPalette` (palette size has to be a power of 2).

- Splits are only done on red, green and blue, alpha is averaged.
- Palettes are ordered by the number of pixels each color represents (most common first).
- Palettes never contain duplicates. If there aren't enough distinct colors, the palette is smaller than requested.

### Conversion

All formats can be converted into each other.

```csharp
Span<ColorRGB> rgb = image.ToArray();

ColorBGRA[] bgra = rgb.Convert<ColorRGB, ColorBGRA>();
rgb.Convert(bgraBuffer);

// only for formats of the same size
Span<ColorBGR> bgr = rgb.ConvertInPlace<ColorRGB, ColorBGR>();
```

Conversion is mostly limited by memory bandwidth. Writing to freshly allocated memory often costs more than the conversion itself, so reuse buffers where possible.

## SkiaSharp and System.Drawing

Loading files and converting from and to their image types:

```csharp
Image<ColorBGRA> image = ImageHelper.LoadImage("image.png");
using SKImage skImage = image.ToSKImage();
```

See [HPPH.SkiaSharp](https://github.com/DarthAffe/HPPH/blob/master/HPPH.SkiaSharp/README.md) and [HPPH.System.Drawing](https://github.com/DarthAffe/HPPH/blob/master/HPPH.System.Drawing/README.md) for details.
