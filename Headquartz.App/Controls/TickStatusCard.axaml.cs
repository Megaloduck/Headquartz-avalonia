using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Headquartz.App.Controls;

/// <summary>
/// Displays the current in-game date plus two segmented pill bars:
///
///   Tick bar (green)
///     SegmentCount     = TicksPerWorkHour  (4/5/6/7 by difficulty)
///     FilledCount      = whole completed ticks within current work hour
///     PartialFillRatio = 0.0–1.0, ms elapsed in current tick / TickDelayMs
///                        → the next pill animates smoothly in real-time
///
///   Work hours (cyan)
///     SegmentCount     = 8 (always — WorkHoursPerDay)
///     FilledCount      = whole completed work hours within current day
///     No partial fill — snaps per completed work hour
/// </summary>
public partial class TickStatusCard : UserControl
{
    // ── DateLabel ─────────────────────────────────────────────

    public static readonly StyledProperty<string> DateLabelProperty =
        AvaloniaProperty.Register<TickStatusCard, string>(nameof(DateLabel), string.Empty);

    public string DateLabel
    {
        get => GetValue(DateLabelProperty);
        set => SetValue(DateLabelProperty, value);
    }

    // ── TicksPerWorkHour ──────────────────────────────────────

    public static readonly StyledProperty<int> TicksPerWorkHourProperty =
        AvaloniaProperty.Register<TickStatusCard, int>(nameof(TicksPerWorkHour), 5);

    /// <summary>4/5/6/7 by difficulty — controls tick bar segment count.</summary>
    public int TicksPerWorkHour
    {
        get => GetValue(TicksPerWorkHourProperty);
        set => SetValue(TicksPerWorkHourProperty, value);
    }

    // ── TicksElapsedInWorkHour ────────────────────────────────

    public static readonly StyledProperty<int> TicksElapsedInWorkHourProperty =
        AvaloniaProperty.Register<TickStatusCard, int>(nameof(TicksElapsedInWorkHour), 0);

    /// <summary>
    /// Whole ticks completed in the current work hour (0 .. TicksPerWorkHour).
    /// These pills are fully lit.
    /// </summary>
    public int TicksElapsedInWorkHour
    {
        get => GetValue(TicksElapsedInWorkHourProperty);
        set => SetValue(TicksElapsedInWorkHourProperty, value);
    }

    // ── TickPartialFillRatio ──────────────────────────────────

    public static readonly StyledProperty<double> TickPartialFillRatioProperty =
        AvaloniaProperty.Register<TickStatusCard, double>(nameof(TickPartialFillRatio), 0.0);

    /// <summary>
    /// 0.0–1.0. How far the in-progress tick pill is filled.
    /// Computed as: msElapsedInCurrentTick / TickDelayMs.
    /// Updated every ~100 ms by ShellViewModel's sub-tick timer.
    /// </summary>
    public double TickPartialFillRatio
    {
        get => GetValue(TickPartialFillRatioProperty);
        set => SetValue(TickPartialFillRatioProperty, value);
    }

    // ── WorkHoursElapsedInDay ─────────────────────────────────

    public static readonly StyledProperty<int> WorkHoursElapsedInDayProperty =
        AvaloniaProperty.Register<TickStatusCard, int>(nameof(WorkHoursElapsedInDay), 0);

    /// <summary>Whole work hours completed in the current day (0 .. 8).</summary>
    public int WorkHoursElapsedInDay
    {
        get => GetValue(WorkHoursElapsedInDayProperty);
        set => SetValue(WorkHoursElapsedInDayProperty, value);
    }

    // ─────────────────────────────────────────────────────────

    public TickStatusCard()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}