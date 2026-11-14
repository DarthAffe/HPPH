using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HPPH;

namespace HPPHExample;

public enum PaletteAlgorithm
{
    Default,
    Wu,
    MedianCut
}

public enum ResultView
{
    Pipeline,
    PaletteRemap,
    SortedPixels
}

public sealed record SampleFile(string Name, string Path);

public sealed record PaletteEntry(Brush Brush, string Text);

public sealed record AnalysisSettings(PaletteAlgorithm Algorithm, int PaletteSize, ResultView View, SortChannel SortChannel);

public sealed record ComputeResult(
    List<PipelineStep> Steps,
    IImage Result,
    IImage Display,
    BitmapSource? Bitmap,
    string Sum,
    string Average,
    Brush? AverageBrush,
    string MinMax,
    string PaletteInfo,
    List<PaletteEntry> Palette,
    List<string> Timings,
    string? Error);

public sealed class MainViewModel : INotifyPropertyChanged
{
    #region Properties & Fields

    private bool _isLoading;
    private bool _isRunning;
    private bool _isPending;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<ImageLoader> Loaders { get; } = Enum.GetValues<ImageLoader>();
    public IReadOnlyList<IColorFormat> Formats => Operations.Formats;
    public IReadOnlyList<ImageRotation> Rotations { get; } = Enum.GetValues<ImageRotation>();
    public IReadOnlyList<PaletteAlgorithm> PaletteAlgorithms { get; } = Enum.GetValues<PaletteAlgorithm>();
    public IReadOnlyList<ResultView> ResultViews { get; } = Enum.GetValues<ResultView>();
    public IReadOnlyList<SortChannel> SortChannels { get; } = Enum.GetValues<SortChannel>();
    public IReadOnlyList<SampleFile> Samples { get; } = FindSamples();
    public string RuntimeInfo { get; } = ImageInfo.DescribeRuntime();

    public ImageLoader Loader
    {
        get;
        set
        {
            if (Set(ref field, value) && (FilePath != null))
                Load(FilePath);
        }
    }

    public SampleFile? SelectedSample
    {
        get;
        set
        {
            if (Set(ref field, value) && (value != null))
                Load(value.Path);
        }
    }

    public string? FilePath
    {
        get;
        private set => Set(ref field, value);
    }

    public IImage? Source
    {
        get;
        private set => Set(ref field, value);
    }

    public BitmapSource? SourceBitmap
    {
        get;
        private set => Set(ref field, value);
    }

    public string SourceInfo
    {
        get;
        private set => Set(ref field, value);
    } = string.Empty;

    public bool Convert
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public IColorFormat Format
    {
        get;
        set => SetAndRecompute(ref field, value);
    } = IColorFormat.RGB;

    public bool Pad
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public int Padding
    {
        get;
        set => SetAndRecompute(ref field, value);
    } = 13;

    public bool Slice
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public int SliceX
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public int SliceY
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public int SliceWidth
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public int SliceHeight
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public bool FlipHorizontal
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public bool FlipVertical
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public ImageRotation Rotation
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public bool Downscale
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public int FactorX
    {
        get;
        set
        {
            SetAndRecompute(ref field, value);
            if (LinkFactors) FactorY = value;
        }
    } = 2;

    public int FactorY
    {
        get;
        set
        {
            SetAndRecompute(ref field, value);
            if (LinkFactors && (FactorX != value)) FactorX = value;
        }
    } = 2;

    public bool LinkFactors
    {
        get;
        set
        {
            if (Set(ref field, value) && value)
                FactorY = FactorX;
        }
    } = true;

    public bool UseDestination
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public int DestinationPadding
    {
        get;
        set => SetAndRecompute(ref field, value);
    } = 7;

    public PaletteAlgorithm PaletteAlgorithm
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public int PaletteSize
    {
        get;
        set => SetAndRecompute(ref field, value);
    } = 16;

    public ResultView ResultView
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public SortChannel SortChannel
    {
        get;
        set => SetAndRecompute(ref field, value);
    }

    public ComputeResult? Result
    {
        get;
        private set
        {
            if (Set(ref field, value))
                OnPropertyChanged(nameof(ResultInfo));
        }
    }

    public string ResultInfo => Result == null ? string.Empty : ImageInfo.Describe(Result.Display);

    public string? Error
    {
        get;
        private set => Set(ref field, value);
    }

    public bool IsBusy
    {
        get;
        private set => Set(ref field, value);
    }

    public string SourceHover
    {
        get;
        private set => Set(ref field, value);
    } = string.Empty;

    public string ResultHover
    {
        get;
        private set => Set(ref field, value);
    } = string.Empty;

    public double Zoom
    {
        get;
        set => Set(ref field, Math.Clamp(value, 0.05, 64));
    } = 1;

    #endregion

    #region Constructors

    public MainViewModel()
    {
        SelectedSample = Samples.FirstOrDefault();
    }

    #endregion

    #region Methods

    public void Load(string path)
    {
        try
        {
            IImage image = ImageIO.Load(path, Loader);

            _isLoading = true;
            try
            {
                FilePath = path;
                Source = image;
                SourceBitmap = ImageInfo.ToBitmapSource(image);
                SourceInfo = $"{Loader}: {ImageInfo.Describe(image)}";
                SourceHover = string.Empty;

                SliceX = image.Width / 4;
                SliceY = image.Height / 4;
                SliceWidth = Math.Max(1, image.Width / 2);
                SliceHeight = Math.Max(1, image.Height / 2);
            }
            finally
            {
                _isLoading = false;
            }

            Error = null;
            Recompute();
        }
        catch (Exception ex)
        {
            Error = $"Loading '{path}' failed: {ex.GetType().Name}: {ex.Message}";
        }
    }

    public void Save(string path)
    {
        if (Result == null) return;

        try
        {
            ImageIO.SavePng(Result.Display, path, Loader);
        }
        catch (Exception ex)
        {
            Error = $"Saving failed: {ex.GetType().Name}: {ex.Message}";
        }
    }

    public void UpdateHover(bool source, int x, int y)
    {
        if (source)
            SourceHover = Source == null ? string.Empty : ImageInfo.DescribePixel(Source, x, y);
        else
            ResultHover = Result == null ? string.Empty : ImageInfo.DescribePixel(Result.Display, x, y);
    }

    public void ClearHover(bool source)
    {
        if (source)
            SourceHover = string.Empty;
        else
            ResultHover = string.Empty;
    }

    private async void Recompute()
    {
        if (_isLoading || (Source == null)) return;

        if (_isRunning)
        {
            _isPending = true;
            return;
        }

        _isRunning = true;
        IsBusy = true;
        try
        {
            do
            {
                _isPending = false;

                IImage source = Source;
                PipelineSettings pipeline = CreatePipelineSettings();
                AnalysisSettings analysis = new(PaletteAlgorithm, PaletteSize, ResultView, SortChannel);

                try
                {
                    ComputeResult result = await Task.Run(() => Compute(source, pipeline, analysis));
                    Result = result;
                    Error = result.Error;
                }
                catch (Exception ex)
                {
                    Result = null;
                    ResultHover = string.Empty;
                    Error = $"{ex.GetType().Name}: {ex.Message}";
                }
            } while (_isPending);
        }
        finally
        {
            _isRunning = false;
            IsBusy = false;
        }
    }

    private PipelineSettings CreatePipelineSettings()
    {
        ImageFlip flip = (FlipHorizontal ? ImageFlip.Horizontal : ImageFlip.None) | (FlipVertical ? ImageFlip.Vertical : ImageFlip.None);

        return new PipelineSettings(Convert, Format,
                                    Pad, Padding,
                                    Slice, SliceX, SliceY, SliceWidth, SliceHeight,
                                    flip, Rotation,
                                    Downscale, FactorX, FactorY,
                                    UseDestination, DestinationPadding);
    }

    private static ComputeResult Compute(IImage source, PipelineSettings pipeline, AnalysisSettings analysis)
    {
        List<PipelineStep> steps = ImagePipeline.Run(source, pipeline);
        IImage result = steps.Count > 0 ? steps[^1].Image : source;

        List<string> timings = [];
        List<string> errors = [];

        string sum = Measure("Sum", timings, errors, () => result.Sum().ToString());

        Brush? averageBrush = null;
        string average = Measure("Average", timings, errors, () =>
        {
            IColor color = result.Average();
            averageBrush = ImageInfo.ToBrush(color);
            return ImageInfo.DescribeColor(color);
        });

        string minMax = Measure("MinMax", timings, errors, () =>
        {
            ColorMinMax value = result.MinMax();
            return $"{value}{Environment.NewLine}Ranges [A: {value.AlphaRange}, R: {value.RedRange}, G: {value.GreenRange}, B: {value.BlueRange}]";
        });

        IColor[] palette = [];
        long[] pixelCounts = new long[Math.Max(0, analysis.PaletteSize)];
        string paletteInfo = Measure($"Palette ({analysis.Algorithm})", timings, errors, () =>
        {
            palette = analysis.Algorithm switch
            {
                PaletteAlgorithm.Wu => result.CreateWuColorPalette(analysis.PaletteSize, pixelCounts),
                PaletteAlgorithm.MedianCut => result.CreateMedianCutColorPalette(analysis.PaletteSize, pixelCounts),
                _ => result.CreateColorPalette(analysis.PaletteSize, pixelCounts)
            };

            return $"{palette.Length} colors, {pixelCounts.Take(palette.Length).Sum():N0} of {(long)result.Width * result.Height:N0} pixels";
        });

        List<PaletteEntry> entries = palette.Select((color, i) => new PaletteEntry(ImageInfo.ToBrush(color), $"{ImageInfo.DescribeColor(color)}  {pixelCounts[i]:N0} px")).ToList();

        IImage display = result;
        switch (analysis.View)
        {
            case ResultView.PaletteRemap:
                Measure("Remap to palette (app-side)", timings, errors, () =>
                {
                    display = result.Execute<RemapOperation, IImage>(new RemapOperation(palette));
                    return string.Empty;
                });
                break;

            case ResultView.SortedPixels:
                Measure($"SortBy{analysis.SortChannel} (incl. copy)", timings, errors, () =>
                {
                    display = result.Execute<SortOperation, IImage>(new SortOperation(analysis.SortChannel));
                    return string.Empty;
                });
                break;
        }

        BitmapSource? bitmap = ImageInfo.ToBitmapSource(display);

        return new ComputeResult(steps, result, display, bitmap, sum, average, averageBrush, minMax, paletteInfo, entries, timings,
                                 errors.Count > 0 ? string.Join(Environment.NewLine, errors) : null);
    }

    private static string Measure(string name, List<string> timings, List<string> errors, Func<string> action)
    {
        long start = Stopwatch.GetTimestamp();
        try
        {
            string result = action();
            timings.Add($"{name}: {Stopwatch.GetElapsedTime(start).TotalMilliseconds:0.###} ms");
            return result;
        }
        catch (Exception ex)
        {
            string error = $"{name} failed: {ex.GetType().Name}: {ex.Message}";
            errors.Add(error);
            return error;
        }
    }

    private static List<SampleFile> FindSamples()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            string path = Path.Combine(directory.FullName, "sample_data");
            if (Directory.Exists(path))
                return Directory.EnumerateFiles(path)
                                .Order()
                                .Select(x => new SampleFile(Path.GetFileName(x), x))
                                .ToList();
        }

        return [];
    }

    private void SetAndRecompute<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Set(ref field, value, propertyName))
            Recompute();
    }

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}
