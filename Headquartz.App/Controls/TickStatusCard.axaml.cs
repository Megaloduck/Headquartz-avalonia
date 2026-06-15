using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Headquartz.App.Controls;

/// <summary>
/// Displays the current in-game date plus two segmented pill bars:
///
///   Tick bar (green)  — SegmentCount = TicksPerWorkHour (4/5/6/7 by difficulty)
///                       1 pill fills per completed simulation tick (5 real seconds)
///                       Resets when a work hour completes.
///
///   Work hours (cyan) — SegmentCount always 8 (WorkHoursPerDay)
///                       1 pill fills per completed work hour
///                       Resets when a day completes.
///
/// All three "Elapsed" properties are driven by ShellViewModel which reads
/// SimulationProfile.TicksPerWorkHour and SimulationClock.Tick.
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

    // ── TicksPerWorkHour (drives tick bar segment count) ─────

    public static readonly StyledProperty<int> TicksPerWorkHourProperty =
        AvaloniaProperty.Register<TickStatusCard, int>(nameof(TicksPerWorkHour), 5);

    /// <summary>
    /// Number of ticks that make one work hour: 4/5/6/7 by difficulty.
    /// Controls how many pills the tick bar has.
    /// </summary>
    public int TicksPerWorkHour
    {
        get => GetValue(TicksPerWorkHourProperty);
        set => SetValue(TicksPerWorkHourProperty, value);
    }

    // ── TicksElapsedInWorkHour (tick bar fill) ───────────────

    public static readonly StyledProperty<int> TicksElapsedInWorkHourProperty =
        AvaloniaProperty.Register<TickStatusCard, int>(nameof(TicksElapsedInWorkHour), 0);

    /// <summary>
    /// How many ticks have elapsed in the current work hour (0 .. TicksPerWorkHour).
    /// Each completed tick lights up one green pill.
    /// </summary>
    public int TicksElapsedInWorkHour
    {
        get => GetValue(TicksElapsedInWorkHourProperty);
        set => SetValue(TicksElapsedInWorkHourProperty, value);
    }

    // ── WorkHoursElapsedInDay (work hours bar fill) ──────────

    public static readonly StyledProperty<int> WorkHoursElapsedInDayProperty =
        AvaloniaProperty.Register<TickStatusCard, int>(nameof(WorkHoursElapsedInDay), 0);

    /// <summary>
    /// How many work hours have elapsed in the current day (0 .. 8).
    /// Each completed work hour lights up one cyan pill.
    /// </summary>
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}