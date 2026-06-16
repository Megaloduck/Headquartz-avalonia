using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Headquartz.App.Controls;

/// <summary>
/// A horizontal row of pill-shaped segments (default 8).
///
/// Supports two fill modes via <see cref="FilledCount"/> (whole pills)
/// and <see cref="PartialFillRatio"/> (0.0–1.0 fill width on the next pill),
/// so callers can animate a smooth in-progress indicator:
///
///   FilledCount       = number of fully completed pills
///   PartialFillRatio  = how far the NEXT pill is filled (0 = empty, 1 = full)
///
/// The pill at index FilledCount gets a fill width equal to PartialFillRatio;
/// everything past that index uses EmptyBrush as background.
/// </summary>
public partial class SegmentedProgressBar : UserControl
{
    public static readonly StyledProperty<int> SegmentCountProperty =
        AvaloniaProperty.Register<SegmentedProgressBar, int>(nameof(SegmentCount), 8);

    public static readonly StyledProperty<int> FilledCountProperty =
        AvaloniaProperty.Register<SegmentedProgressBar, int>(nameof(FilledCount), 0);

    /// <summary>
    /// 0.0–1.0. How filled the pill immediately after FilledCount is.
    /// 0 = completely empty, 1 = fully lit (same as FilledBrush).
    /// </summary>
    public static readonly StyledProperty<double> PartialFillRatioProperty =
        AvaloniaProperty.Register<SegmentedProgressBar, double>(nameof(PartialFillRatio), 0.0);

    public static readonly StyledProperty<IBrush> FilledBrushProperty =
        AvaloniaProperty.Register<SegmentedProgressBar, IBrush>(nameof(FilledBrush), Brushes.Green);

    public static readonly StyledProperty<IBrush> EmptyBrushProperty =
        AvaloniaProperty.Register<SegmentedProgressBar, IBrush>(nameof(EmptyBrush), Brushes.LightGray);

    public int SegmentCount
    {
        get => GetValue(SegmentCountProperty);
        set => SetValue(SegmentCountProperty, value);
    }

    /// <summary>Number of fully completed (fully lit) pills.</summary>
    public int FilledCount
    {
        get => GetValue(FilledCountProperty);
        set => SetValue(FilledCountProperty, value);
    }

    /// <summary>
    /// Progress of the current (next) pill: 0.0 = empty, 1.0 = fully lit.
    /// Ignored when FilledCount >= SegmentCount.
    /// </summary>
    public double PartialFillRatio
    {
        get => GetValue(PartialFillRatioProperty);
        set => SetValue(PartialFillRatioProperty, value);
    }

    public IBrush FilledBrush
    {
        get => GetValue(FilledBrushProperty);
        set => SetValue(FilledBrushProperty, value);
    }

    public IBrush EmptyBrush
    {
        get => GetValue(EmptyBrushProperty);
        set => SetValue(EmptyBrushProperty, value);
    }

    public ObservableCollection<SegmentItem> Segments { get; } = [];

    static SegmentedProgressBar()
    {
        SegmentCountProperty.Changed.AddClassHandler<SegmentedProgressBar>((c, _) => c.Rebuild());
        FilledCountProperty.Changed.AddClassHandler<SegmentedProgressBar>((c, _) => c.Rebuild());
        PartialFillRatioProperty.Changed.AddClassHandler<SegmentedProgressBar>((c, _) => c.Rebuild());
        FilledBrushProperty.Changed.AddClassHandler<SegmentedProgressBar>((c, _) => c.Rebuild());
        EmptyBrushProperty.Changed.AddClassHandler<SegmentedProgressBar>((c, _) => c.Rebuild());
    }

    public SegmentedProgressBar()
    {
        InitializeComponent();
        Rebuild();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void Rebuild()
    {
        int total = SegmentCount;
        int filled = Math.Clamp(FilledCount, 0, total);
        double partial = Math.Clamp(PartialFillRatio, 0.0, 1.0);

        if (Segments.Count != total)
        {
            Segments.Clear();
            for (int i = 0; i < total; i++)
                Segments.Add(new SegmentItem());
        }

        for (int i = 0; i < total; i++)
        {
            if (i < filled)
            {
                // Fully lit pill
                Segments[i].FilledBrush = FilledBrush;
                Segments[i].EmptyBrush = EmptyBrush;
                Segments[i].FillWidth = 100.0; // 100% filled
            }
            else if (i == filled && partial > 0.0 && filled < total)
            {
                // In-progress pill — fill width equals partial ratio
                Segments[i].FilledBrush = FilledBrush;
                Segments[i].EmptyBrush = EmptyBrush;
                Segments[i].FillWidth = partial * 100.0; // Percentage filled
            }
            else
            {
                // Empty pill
                Segments[i].FilledBrush = FilledBrush;
                Segments[i].EmptyBrush = EmptyBrush;
                Segments[i].FillWidth = 0.0; // 0% filled
            }
        }
    }
}

/// <summary>A single pill segment's visual state.</summary>
public class SegmentItem : INotifyPropertyChanged
{
    private IBrush _filledBrush = Brushes.Green;
    private IBrush _emptyBrush = Brushes.LightGray;
    private double _fillWidth = 0.0;

    public IBrush FilledBrush
    {
        get => _filledBrush;
        set { if (!Equals(_filledBrush, value)) { _filledBrush = value; OnPropertyChanged(); } }
    }

    public IBrush EmptyBrush
    {
        get => _emptyBrush;
        set { if (!Equals(_emptyBrush, value)) { _emptyBrush = value; OnPropertyChanged(); } }
    }

    public double FillWidth
    {
        get => _fillWidth;
        set { if (Math.Abs(_fillWidth - value) > 0.001) { _fillWidth = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}