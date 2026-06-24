using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Domain.Entities;

/// <summary>
/// Static blueprint for a scheduled calendar event (e.g., Grand Opening, Payroll, Black Friday).
/// Defines when it occurs, how long it lasts, and what effects it has on each department.
/// </summary>
public class CalendarEventDefinition
{
    /// <summary>Stable identifier used to link instances back to their template.</summary>
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Campaign { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// UI color token. Maps to Avalonia brushes in the view layer.
    /// Suggested: "Green", "Yellow", "Red", "Pink", "Blue", "Purple", "Orange"
    /// </summary>
    public string ColorTag { get; set; } = "Blue";

    public EventSeverity Severity { get; set; } = EventSeverity.Low;

    // ── Scheduling ───────────────────────────────────────────

    /// <summary>
    /// For fixed-date events (e.g., New Year on Jan 1). Ignored if <see cref="IsRecurring"/> is true.
    /// </summary>
    public DateTime? FixedDate { get; set; }

    /// <summary>
    /// For recurring events (e.g., Payroll on the 25th of every month).
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// For monthly recurring events: the day of the month (1-31).
    /// </summary>
    public int? RecurringDayOfMonth { get; set; }

    /// <summary>
    /// For recurring events locked to a specific month (1-12). Null means every month.
    /// </summary>
    public int? RecurringMonth { get; set; }

    /// <summary>
    /// For recurring events: months to skip (e.g., December payroll moved to 24th).
    /// </summary>
    public List<int> RecurringExceptMonths { get; set; } = [];

    /// <summary>
    /// For date-range events: how many days the event spans (e.g., Black Friday = 4 days).
    /// </summary>
    public int DurationDays { get; set; } = 1;

    /// <summary>
    /// True for random "black swan" events that are not on the calendar.
    /// </summary>
    public bool IsRandom { get; set; }

    /// <summary>
    /// Probability per tick that a random event fires. Only used when <see cref="IsRandom"/> is true.
    /// </summary>
    public double? RandomChancePerTick { get; set; }

    // ── Effects ──────────────────────────────────────────────

    /// <summary>
    /// Per-department effects applied when the event is active.
    /// </summary>
    public List<DepartmentEffect> DepartmentEffects { get; set; } = [];

    /// <summary>
    /// Convenience helper to check if a given simulation date falls on this event.
    /// </summary>
    public bool IsActiveOn(DateTime date)
    {
        if (IsRandom) return false;

        if (IsRecurring && RecurringDayOfMonth.HasValue)
        {
            if (date.Day != RecurringDayOfMonth.Value) return false;
            if (RecurringMonth.HasValue && date.Month != RecurringMonth.Value) return false;
            if (RecurringExceptMonths.Contains(date.Month)) return false;
            return true;
        }

        if (FixedDate.HasValue)
        {
            var fd = FixedDate.Value;
            var eventStart = new DateTime(date.Year, fd.Month, fd.Day);
            var checkDate = date.Date;
            return checkDate >= eventStart && checkDate < eventStart.AddDays(DurationDays);
        }

        return false;
    }

    /// <summary>
    /// Returns the start date for this definition relative to a given year.
    /// For fixed-date events, uses the same month/day in the current year.
    /// For recurring events, returns the matching date in the current year.
    /// </summary>
    public DateTime? GetStartDateForYear(int year)
    {
        if (IsRandom) return null;

        if (IsRecurring && RecurringDayOfMonth.HasValue)
        {
            int month = RecurringMonth ?? 1;
            int day = Math.Min(RecurringDayOfMonth.Value, DateTime.DaysInMonth(year, month));
            return new DateTime(year, month, day);
        }

        if (FixedDate.HasValue)
        {
            var d = FixedDate.Value;
            return new DateTime(year, d.Month, d.Day);
        }

        return null;
    }
}
