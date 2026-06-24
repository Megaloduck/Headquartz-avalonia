using System;
using System.Collections.Generic;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Headquartz.App.Models;

/// <summary>
/// Model for a single event card shown inside a calendar day cell.
/// Maps to the colored blocks in the Forecast Overview calendar.
/// </summary>
public partial class CalendarEventCardModel : ObservableObject
{
    [ObservableProperty] private string _title = string.Empty;

    [ObservableProperty] private string _subtitle = string.Empty;

    /// <summary>
    /// UI color token (e.g., "Green", "Yellow", "Red", "Pink", "Blue", "Purple").
    /// The view maps this to Avalonia dynamic resources.
    /// </summary>
    [ObservableProperty] private string _colorTag = "Blue";

    /// <summary>
    /// The first department-effect description to show as the subtitle.
    /// E.g., "Marketing +10%", "Discount 20%", "Labour pays +20%".
    /// </summary>
    [ObservableProperty] private string _effectSummary = string.Empty;

    [ObservableProperty] private bool _isActiveToday;
}
