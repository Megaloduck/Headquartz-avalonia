using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Headquartz.App.Models;

/// <summary>
/// Model for a single day cell in the Event Calendar grid.
/// </summary>
public partial class CalendarDayModel : ObservableObject
{
    [ObservableProperty] private int _dayNumber;

    [ObservableProperty] private bool _isCurrentMonth = true;

    [ObservableProperty] private bool _isToday;

    /// <summary>
    /// Events occurring on this day.
    /// </summary>
    public ObservableCollection<CalendarEventCardModel> Events { get; } = [];
}
