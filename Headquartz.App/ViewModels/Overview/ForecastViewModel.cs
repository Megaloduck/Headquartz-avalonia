using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;

namespace Headquartz.App.ViewModels;

public partial class ForecastViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── Summary ───────────────────────────────────────────────

    [ObservableProperty] private decimal _currentCash;

    // ── Calendar ─────────────────────────────────────────────

    [ObservableProperty] private DateTime _currentMonth = DateTime.Now;
    [ObservableProperty] private string _upcomingEventTitle = "—";
    [ObservableProperty] private string _simulationElapsedDisplay = "00:00:00";
    [ObservableProperty] private long _ticksElapsed;
    [ObservableProperty] private bool _hasNoTodayEvents;

    public ObservableCollection<CalendarDayModel> CalendarDays { get; } = [];
    public ObservableCollection<CalendarEventCardModel> TodayEvents { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public ForecastViewModel(SimulationService simulation)
    {
        _simulation = simulation;
        _currentMonth = _simulation.Engine.Clock.WorldTime;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Navigation ────────────────────────────────────────────

    [RelayCommand]
    public void PreviousMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
        BuildCalendar();
    }

    [RelayCommand]
    public void NextMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
        BuildCalendar();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;

        CurrentCash = company.Cash;


        // Calendar & stats
        TicksElapsed = clock.Tick;

        var elapsed = clock.WorldTime - new DateTime(2026, 1, 1, 8, 0, 0);
        SimulationElapsedDisplay = $"{elapsed.Days:D2} : {elapsed.Hours:D2} : {elapsed.Minutes:D2}";

        BuildCalendar();
        RefreshTodayEvents();
        RefreshUpcomingEvent();
    }

    // =========================================================
    // CALENDAR BUILDER
    // =========================================================

    private void BuildCalendar()
    {
        CalendarDays.Clear();

        var year = CurrentMonth.Year;
        var month = CurrentMonth.Month;

        var firstOfMonth = new DateTime(year, month, 1);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        // Pad leading days from previous month
        int startDayOfWeek = (int)firstOfMonth.DayOfWeek;
        var prevMonth = firstOfMonth.AddMonths(-1);
        int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

        for (int i = startDayOfWeek - 1; i >= 0; i--)
        {
            CalendarDays.Add(new CalendarDayModel
            {
                DayNumber = daysInPrevMonth - i,
                IsCurrentMonth = false,
                IsToday = false,
            });
        }

        var today = _simulation.Engine.Clock.WorldTime.Date;

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            var dayModel = new CalendarDayModel
            {
                DayNumber = day,
                IsCurrentMonth = true,
                IsToday = date == today,
            };

            // Add events for this date from the registry
            var defs = Headquartz.Simulation.Systems.CalendarEventRegistry.GetActiveForDate(date);
            foreach (var def in defs)
            {
                var effectSummary = def.DepartmentEffects.FirstOrDefault()?.Description ?? "";
                dayModel.Events.Add(new CalendarEventCardModel
                {
                    Title = def.Title,
                    Subtitle = def.Campaign,
                    ColorTag = def.ColorTag,
                    EffectSummary = effectSummary,
                    IsActiveToday = date == today,
                });
            }

            CalendarDays.Add(dayModel);
        }

        // Pad trailing days to complete the grid (6 rows × 7 cols = 42 cells)
        int remaining = 42 - CalendarDays.Count;
        for (int i = 1; i <= remaining; i++)
        {
            CalendarDays.Add(new CalendarDayModel
            {
                DayNumber = i,
                IsCurrentMonth = false,
                IsToday = false,
            });
        }
    }

    private void RefreshTodayEvents()
    {
        TodayEvents.Clear();

        var today = _simulation.Engine.Clock.WorldTime.Date;
        var activeEvents = _simulation.Engine.CalendarEvents.ActiveEvents
            .Where(a => a.StartDate <= today && a.EndDate > today)
            .ToList();

        foreach (var active in activeEvents)
        {
            var def = active.Definition;
            var effectSummary = def.DepartmentEffects.FirstOrDefault()?.Description ?? "";
            TodayEvents.Add(new CalendarEventCardModel
            {
                Title = def.Title,
                Subtitle = def.Campaign,
                ColorTag = def.ColorTag,
                EffectSummary = effectSummary,
                IsActiveToday = true,
            });
        }

        // If nothing is active today, show the next upcoming event
        if (TodayEvents.Count == 0)
        {
            var nextEvent = GetNextUpcomingEvent();
            if (nextEvent != null)
            {
                var effectSummary = nextEvent.DepartmentEffects.FirstOrDefault()?.Description ?? "";
                TodayEvents.Add(new CalendarEventCardModel
                {
                    Title = nextEvent.Title,
                    Subtitle = $"Upcoming: {nextEvent.Campaign}",
                    ColorTag = nextEvent.ColorTag,
                    EffectSummary = effectSummary,
                    IsActiveToday = false,
                });
            }
        }

        HasNoTodayEvents = TodayEvents.Count == 0;
    }

    private void RefreshUpcomingEvent()
    {
        var next = GetNextUpcomingEvent();
        UpcomingEventTitle = next?.Title ?? "—";
    }

    private Headquartz.Domain.Entities.CalendarEventDefinition? GetNextUpcomingEvent()
    {
        var today = _simulation.Engine.Clock.WorldTime.Date;
        var year = today.Year;

        var allEvents = Headquartz.Simulation.Systems.CalendarEventRegistry.Definitions
            .Where(d => !d.IsRandom)
            .ToList();

        Headquartz.Domain.Entities.CalendarEventDefinition? next = null;
        DateTime? nextDate = null;

        foreach (var def in allEvents)
        {
            DateTime? candidate = null;

            if (def.IsRecurring && def.RecurringDayOfMonth.HasValue)
            {
                int m = def.RecurringMonth ?? today.Month;
                int d = Math.Min(def.RecurringDayOfMonth.Value, DateTime.DaysInMonth(year, m));
                candidate = new DateTime(year, m, d);
                if (candidate < today)
                {
                    // Try next month (or next year for month-specific events)
                    if (def.RecurringMonth.HasValue)
                        candidate = new DateTime(year + 1, m, d);
                    else
                        candidate = candidate.Value.AddMonths(1);
                }
            }
            else if (def.FixedDate.HasValue)
            {
                var fd = def.FixedDate.Value;
                candidate = new DateTime(year, fd.Month, fd.Day);
                if (candidate < today)
                    candidate = new DateTime(year + 1, fd.Month, fd.Day);
            }

            if (candidate.HasValue && candidate >= today)
            {
                if (nextDate == null || candidate < nextDate)
                {
                    nextDate = candidate;
                    next = def;
                }
            }
        }

        return next;
    }
}