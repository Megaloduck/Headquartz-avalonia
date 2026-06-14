using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Headquartz.App.Controls;

/// <summary>
/// A horizontal row of pill-shaped segments (default 8), where the first
/// <see cref="FilledCount"/> segments are rendered with <see cref="FilledBrush"/>
/// and the remainder with <see cref="EmptyBrush"/>.
///
/// Used to visualize discrete time units, e.g.:
///   - Tick status: 1 tick = 8 seconds  → SegmentCount=8, FilledCount=seconds elapsed in tick
///   - Work hours:  1 work hour = 8 ticks → SegmentCount=8, FilledCount=ticks elapsed in work hour
/// </summary>
public partial class SegmentedProgressBar : UserControl
{
    public static readonly StyledProperty<int> SegmentCountProperty =
        AvaloniaProperty.Register<SegmentedProgressBar, int>(
            nameof(SegmentCount), 8);

    public static readonly StyledProperty<int> FilledCountProperty =
        AvaloniaProperty.Register<SegmentedProgressBar, int>(
            nameof(FilledCount), 0);

    public static readonly StyledProperty<IBrush> FilledBrushProperty =
        AvaloniaProperty.Register<SegmentedProgressBar, IBrush>(
            nameof(FilledBrush), Brushes.Green);

    public static readonly StyledProperty<IBrush> EmptyBrushProperty =
        AvaloniaProperty.Register<SegmentedProgressBar, IBrush>(
            nameof(EmptyBrush), Brushes.LightGray);

    /// <summary>Total number of pill segments to render. Default 8.</summary>
    public int SegmentCount
    {
        get => GetValue(SegmentCountProperty);
        set => SetValue(SegmentCountProperty, value);
    }

    /// <summary>
    /// How many segments (from the left) should appear "filled".
    /// Automatically clamped to [0, SegmentCount].
    /// </summary>
    public int FilledCount
    {
        get => GetValue(FilledCountProperty);
        set => SetValue(FilledCountProperty, value);
    }

    /// <summary>Brush used for filled segments.</summary>
    public IBrush FilledBrush
    {
        get => GetValue(FilledBrushProperty);
        set => SetValue(FilledBrushProperty, value);
    }

    /// <summary>Brush used for empty (not-yet-reached) segments.</summary>
    public IBrush EmptyBrush
    {
        get => GetValue(EmptyBrushProperty);
        set => SetValue(EmptyBrushProperty, value);
    }

    /// <summary>Backing collection rendered by the ItemsControl in XAML.</summary>
    public ObservableCollection<SegmentItem> Segments { get; } = [];

    static SegmentedProgressBar()
    {
        SegmentCountProperty.Changed.AddClassHandler<SegmentedProgressBar>(
            (control, _) => control.Rebuild());

        FilledCountProperty.Changed.AddClassHandler<SegmentedProgressBar>(
            (control, _) => control.Rebuild());

        FilledBrushProperty.Changed.AddClassHandler<SegmentedProgressBar>(
            (control, _) => control.Rebuild());

        EmptyBrushProperty.Changed.AddClassHandler<SegmentedProgressBar>(
            (control, _) => control.Rebuild());
    }

    public SegmentedProgressBar()
    {
        InitializeComponent();
        Rebuild();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Rebuild()
    {
        int total = SegmentCount;
        int filled = FilledCount;

        if (filled < 0) filled = 0;
        if (filled > total) filled = total;

        // Reuse existing items where possible instead of clearing/rebuilding
        // the whole collection every time, so the UI doesn't flicker.
        if (Segments.Count != total)
        {
            Segments.Clear();
            for (int i = 0; i < total; i++)
                Segments.Add(new SegmentItem());
        }

        for (int i = 0; i < total; i++)
            Segments[i].Brush = i < filled ? FilledBrush : EmptyBrush;
    }
}

/// <summary>A single pill segment's visual state.</summary>
public class SegmentItem : INotifyPropertyChanged
{
    private IBrush _brush = Brushes.LightGray;

    public IBrush Brush
    {
        get => _brush;
        set
        {
            if (Equals(_brush, value)) return;
            _brush = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}