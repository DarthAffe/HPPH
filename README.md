# HPPH

C# High Performance Pixel Handling 


## Summary

This library contains helpers to work with color data (currently only 24 and 32 bit byte data) in a somewhat performance-optimized fully in C#.   

It consists of the following packages:

| Package                                                                   | Description                                                                    |
|---------------------------------------------------------------------------|--------------------------------------------------------------------------------|
| [HPPH](https://www.nuget.org/packages/HPPH)                               | Core-package, containg all the features described below                        |
| [HPPH.System.Drawing](https://www.nuget.org/packages/HPPH.System.Drawing) | Contains extensions to convert Images from and to System.Drawing.Bitmaps       |
| [HPPH.SkiaSharp](https://www.nuget.org/packages/HPPH.SkiaSharp)           | Contains extensions to convert Images from and to SkiaSharp images and bitmaps |

## Supported Operations

All of the currently supported operations are briefly described below. More are to come.   

Benchmarks are all run on an Ryzen 9 5900X. Reference is always a simple approach like looping over the data performing the operation.    
The data used is always the full set of sample_data to cover a variaty of images.    


### Image
An abstraction layer for handling pixel-grids.   

Supports all of the operations below and things like allocation-free region selection, iteration and copying of rows, colums or whole images/regions.


### Sum

Optimized summarization of colors into 4 longs (one for each channel).

| Method           | Mean       | Error    | StdDev   | Allocated |
|----------------- |-----------:|---------:|---------:|----------:|
| PixelHelper_3BPP |   107.3 μs |  0.21 μs |  0.20 μs |     528 B |
| PixelHelper_4BPP |   167.7 μs |  0.85 μs |  0.71 μs |     528 B |
| Reference_3BPP   | 1,683.3 μs | 18.87 μs | 17.65 μs |     529 B |
| Reference_4BPP   | 1,619.5 μs |  9.08 μs |  7.58 μs |     529 B |


### Average

Averages some colors into a single color of the same format.

| Method           | Mean       | Error    | StdDev   | Allocated |
|----------------- |-----------:|---------:|---------:|----------:|
| PixelHelper_3BPP |   108.2 μs |  0.41 μs |  0.34 μs |      56 B |
| PixelHelper_4BPP |   169.0 μs |  2.29 μs |  2.14 μs |      64 B |
| Reference_3BPP   | 1,654.9 μs | 12.06 μs | 11.28 μs |     705 B |
| Reference_4BPP   | 1,613.4 μs | 21.11 μs | 18.71 μs |     713 B |


### Min-Max

Gets the minimum and maximum value for each channel of the given color data.

| Method           | Mean       | Error    | StdDev   | Allocated |
|----------------- |-----------:|---------:|---------:|----------:|
| PixelHelper_3BPP |   106.2 μs |  0.38 μs |  0.35 μs |     312 B |
| PixelHelper_4BPP |   139.2 μs |  1.94 μs |  1.82 μs |     312 B |
| Reference_3BPP   | 3,838.1 μs | 35.40 μs | 31.38 μs |     314 B |
| Reference_4BPP   | 4,456.9 μs | 21.06 μs | 19.70 μs |     315 B |


### Sort

Sorts color data by a single channel (Red, Green, Blue or Alpha).   
The algorithm is stable.

> This benchmark is somewhat flawed as it's effectivly running on pre-sorted data, which does not affect the PixelHelper, but might affect the Reference.   
>
> It's also using Linq.OrderBy which is not ideal performance-wise and requires some unneccessary allocations to fit the API, but Array/Span.Sort is not stable, which the PixelHelper is so that wouldn't be a fair comparison.)

| Method           | Mean      | Error     | StdDev    | Allocated  |
|----------------- |----------:|----------:|----------:|-----------:|
| PixelHelper_3BPP |  4.084 ms | 0.0084 ms | 0.0075 ms |        6 B |
| PixelHelper_4BPP |  2.687 ms | 0.0102 ms | 0.0091 ms |        3 B |
| Reference_3BPP   | 59.883 ms | 0.5693 ms | 0.5325 ms | 43118222 B |
| Reference_4BPP   | 59.599 ms | 0.4866 ms | 0.4551 ms | 52355952 B |


### Quantization

Creates a color-palette of a given size for some color-data.   
Currently only a simple but fast variation using the median-cut algorithm is implemented. It's also limited to sizes being a power of 2.   

A more quality focused implementation without the size limitation is planned.


### Conversion

Converts from one color format to another.   
All of the included formats can freely be converted between each other.   

Allocation-free in-place conversion is only supported for formats of same size (both 24 or 32 bit).

| Method             | Mean     | Error     | StdDev    | Allocated   |
|------------------- |---------:|----------:|----------:|------------:|
| RGBToBGR           | 1.487 ms | 0.0221 ms | 0.0196 ms |  9073.58 KB |
| RGBToBGRA          | 1.676 ms | 0.0330 ms | 0.0353 ms | 12064.76 KB |
| RGBAToABGR         | 1.766 ms | 0.0348 ms | 0.0476 ms | 12084.93 KB |
| ARGBToBGR          | 1.533 ms | 0.0072 ms | 0.0064 ms |  9085.36 KB |
| RGBToBGR_InPlace   | 1.025 ms | 0.0021 ms | 0.0017 ms |    34.47 KB |
| RGBAToABGR_InPlace | 1.054 ms | 0.0023 ms | 0.0020 ms |    34.16 KB |
