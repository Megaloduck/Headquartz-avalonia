using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Headquartz.App.Controls;

/// <summary>
/// Displays the current date plus two 8-segment "pill" bars representing
/// the simulation's time hierarchy:
///
///     1 tick      = 8 seconds   → TickSecondsElapsed   (0-8)
///     1 work hour = 8 ticks      → WorkHourTicksElapsed (0-8)
///     1 day       = 8 work hours
///
/// This control is purely presentational — drive the two "elapsed" values
/// from your SimulationClock (see <see cref="FromTickProgress"/> below for
/// a quick way to compute them).
/// </summary>
public partial class TickStatusCard : UserControl
{
    public static readonly StyledProperty<string> DateLabelProperty =
        AvaloniaProperty.Register<TickStatusCard, string>(
            nameof(DateLabel), string.Empty);

    public static readonly StyledProperty<int> TickSecondsElapsedProperty =
        AvaloniaProperty.Register<TickStatusCard, int>(
            nameof(TickSecondsElapsed), 0);

    public static readonly StyledProperty<int> WorkHourTicksElapsedProperty =
        AvaloniaProperty.Register<TickStatusCard, int>(
            nameof(WorkHourTicksElapsed), 0);

    /// <summary>Header text, e.g. "22 September 2026".</summary>
    public string DateLabel
    {
        get => GetValue(DateLabelProperty);
        set => SetValue(DateLabelProperty, value);
    }

    /// <summary>
    /// 0-8. How many of the 8 "second" pills in the current tick are filled.
    /// 8 = the tick just completed / is about to roll over.
    /// </summary>
    public int TickSecondsElapsed
    {
        get => GetValue(TickSecondsElapsedProperty);
        set => SetValue(TickSecondsElapsedProperty, value);
    }

    /// <summary>
    /// 0-8. How many of the 8 "tick" pills in the current work hour are filled.
    /// </summary>
    public int WorkHourTicksElapsed
    {
        get => GetValue(WorkHourTicksElapsedProperty);
        set => SetValue(WorkHourTicksElapsedProperty, value);
    }

    public TickStatusCard()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Convenience helper for computing both pill values from a tick counter
    /// and a sub-tick second counter, given the 8/8 hierarchy described above.
    ///
    /// Example (inside your ViewModel's refresh):
    ///   var (secs, hourTicks) = TickStatusCard.FromTickProgress(
    ///       Clock.Tick, secondsIntoCurrentTick);
    ///   TickSecondsElapsed = secs;
    ///   WorkHourTicksElapsed = hourTicks;
    /// </summary>
    public static (int TickSecondsElapsed, int WorkHourTicksElapsed) FromTickProgress(
        long totalTicks,
        int secondsIntoCurrentTick)
    {
        int secs = secondsIntoCurrentTick % 8;
        int hourTicks = (int)(totalTicks % 8);

        return (secs, hourTicks);
    }
}