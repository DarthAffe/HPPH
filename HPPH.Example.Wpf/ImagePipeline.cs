using System.Diagnostics;
using HPPH;

namespace HPPHExample;

internal sealed record PipelineSettings(
    bool Convert,
    IColorFormat Format,
    bool Pad,
    int Padding,
    bool Slice,
    int SliceX,
    int SliceY,
    int SliceWidth,
    int SliceHeight,
    ImageFlip Flip,
    ImageRotation Rotation,
    bool Downscale,
    int FactorX,
    int FactorY,
    bool UseDestination,
    int DestinationPadding);

public sealed record PipelineStep(string Name, IImage Image, double Milliseconds)
{
    public override string ToString() => $"{Name}: {ImageInfo.Describe(Image)} - {Milliseconds:0.###} ms";
}

internal static class ImagePipeline
{
    #region Methods

    public static List<PipelineStep> Run(IImage source, PipelineSettings settings)
    {
        List<PipelineStep> steps = [];
        IImage image = source;

        if (settings.Convert)
            image = Step(steps, $"Convert to {settings.Format.Name}", () => Operations.ConvertTo(image, settings.Format));

        if (settings.Pad)
            image = Step(steps, $"Pad rows by {settings.Padding} bytes", () => Operations.AddPadding(image, settings.Padding));

        if (settings.Slice)
        {
            int x = Math.Clamp(settings.SliceX, 0, image.Width - 1);
            int y = Math.Clamp(settings.SliceY, 0, image.Height - 1);
            int width = Math.Clamp(settings.SliceWidth, 1, image.Width - x);
            int height = Math.Clamp(settings.SliceHeight, 1, image.Height - y);

            image = Step(steps, $"Slice ({x}, {y}, {width}, {height})", () => image.Slice(x, y, width, height));
        }

        if (settings.Flip != ImageFlip.None)
            image = Step(steps, $"Flip {settings.Flip}", () => Flip(image, settings));

        if (settings.Rotation != ImageRotation.None)
            image = Step(steps, $"Rotate {settings.Rotation}", () => Rotate(image, settings));

        if (settings.Downscale)
            image = Step(steps, $"Downscale {settings.FactorX}x{settings.FactorY}", () => Downscale(image, settings));

        return steps;
    }

    private static IImage Flip(IImage image, PipelineSettings settings)
    {
        if (!settings.UseDestination) return image.Flip(settings.Flip);

        return Operations.WriteInto(image.ColorFormat, image.Width, image.Height, settings.DestinationPadding,
                                    (destination, stride) => image.Flip(settings.Flip, destination, stride));
    }

    private static IImage Rotate(IImage image, PipelineSettings settings)
    {
        if (!settings.UseDestination) return image.Rotate(settings.Rotation);

        (int width, int height) = PixelHelper.GetRotatedSize(image.Width, image.Height, settings.Rotation);
        return Operations.WriteInto(image.ColorFormat, width, height, settings.DestinationPadding,
                                    (destination, stride) => image.Rotate(settings.Rotation, destination, stride));
    }

    private static IImage Downscale(IImage image, PipelineSettings settings)
    {
        if (!settings.UseDestination) return image.Downscale(settings.FactorX, settings.FactorY);

        (int width, int height) = PixelHelper.GetDownscaledSize(image.Width, image.Height, settings.FactorX, settings.FactorY);
        return Operations.WriteInto(image.ColorFormat, width, height, settings.DestinationPadding,
                                    (destination, stride) => image.Downscale(settings.FactorX, settings.FactorY, destination, stride));
    }

    private static IImage Step(List<PipelineStep> steps, string name, Func<IImage> operation)
    {
        long start = Stopwatch.GetTimestamp();
        IImage result = operation();
        double milliseconds = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

        steps.Add(new PipelineStep(name, result, milliseconds));
        return result;
    }

    #endregion
}
