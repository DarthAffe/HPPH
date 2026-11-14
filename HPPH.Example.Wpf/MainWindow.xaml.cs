using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;

namespace HPPHExample;

public partial class MainWindow
{
    #region Constants

    private const string IMAGE_FILTER = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.webp|All files|*.*";

    #endregion

    #region Properties & Fields

    private readonly MainViewModel _viewModel = new();

    #endregion

    #region Constructors

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    #endregion

    #region Methods

    private void OnOpen(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new() { Filter = IMAGE_FILTER };
        if (dialog.ShowDialog(this) == true)
            _viewModel.Load(dialog.FileName);
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new() { Filter = "PNG|*.png", FileName = "result.png" };
        if (dialog.ShowDialog(this) == true)
            _viewModel.Save(dialog.FileName);
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
            _viewModel.Load(files[0]);
    }

    private void OnImageMouseMove(object sender, MouseEventArgs e)
    {
        Image image = (Image)sender;
        Point position = e.GetPosition(image);

        _viewModel.UpdateHover(image == SourceImage, (int)Math.Floor(position.X), (int)Math.Floor(position.Y));
    }

    private void OnImageMouseLeave(object sender, MouseEventArgs e) => _viewModel.ClearHover(sender == SourceImage);

    private void OnZoomOne(object sender, RoutedEventArgs e) => _viewModel.Zoom = 1;

    private void OnZoomFit(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SourceBitmap is not { } bitmap) return;

        _viewModel.Zoom = Math.Min(SourceScroll.ViewportWidth / bitmap.PixelWidth, SourceScroll.ViewportHeight / bitmap.PixelHeight);
    }

    // Only the pane under the mouse drives the other one, so the follow-up event of the other pane doesn't scroll back.
    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        ScrollViewer origin = (ScrollViewer)sender;
        if (!origin.IsMouseOver) return;

        ScrollViewer target = origin == SourceScroll ? ResultScroll : SourceScroll;
        target.ScrollToHorizontalOffset(e.HorizontalOffset);
        target.ScrollToVerticalOffset(e.VerticalOffset);
    }

    #endregion
}

public sealed class LogScaleConverter : IValueConverter
{
    #region Properties & Fields

    public static LogScaleConverter Instance { get; } = new();

    #endregion

    #region Methods

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value == null ? 0 : Math.Log2((double)value);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value == null ? 0 : Math.Pow(2, (double)value);

    #endregion
}
