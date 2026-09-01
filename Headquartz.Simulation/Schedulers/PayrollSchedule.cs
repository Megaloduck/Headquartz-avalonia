using System;
using System.Collections.Generic;
using System.Text;

using System;

namespace Headquartz.Simulation.Schedulers
{
    /// <summary>
    /// Single source of truth for "how many ticks make up one payroll /
    /// loan-interest cycle."
    ///
    /// Resolved: the work week is Mon–Fri, 8 hours/day, 40 hours/week
    /// (8 × 5 = 40 — the design doc's earlier "Mon–Thu" wording was a
    /// typo; Mon–Thu can't reach 40 at 8 hrs/day). A payroll cycle is
    /// 4 weeks.
    ///
    /// TicksPerWorkHour is difficulty-dependent (Trainee=4, Manager=5,
    /// Director=6, Chairman=7 — see SimulationProfile), so the payroll
    /// cycle length is difficulty-dependent too. Every method here takes
    /// the active profile's TicksPerWorkHour explicitly rather than
    /// hardcoding a tick count, so a session's SimulationEngine and every
    /// ViewModel reading its Clock always agree on the same cycle length.
    ///
    /// WorldTime (SimulationClock) is a deliberately separate calendar
    /// clock — flat 15 min/tick, 96 ticks/day — used purely for
    /// calendar-date matching (CalendarEventRegistry, order deadlines).
    /// It does NOT skip weekends and does NOT scale with TicksPerWorkHour.
    /// Fixed-date events (e.g. Christmas) always land on their real
    /// calendar date even if that date is a "weekend" under the work-hour
    /// model — accepted, since calendar events are moving to
    /// session-based randomization rather than fixed dates.
    /// </summary>
    public static class PayrollSchedule
    {
        public const int WorkHoursPerDay = 8;

        /// <summary>Mon–Fri.</summary>
        public const int WorkDaysPerWeek = 5;

        public const int WeeksPerPayrollCycle = 4;

        /// <summary>
        /// Ticks in one payroll cycle for a given difficulty's
        /// TicksPerWorkHour: TicksPerWorkHour × 8 × 5 × 4.
        /// </summary>
        public static long GetTicksPerCycle(int ticksPerWorkHour) =>
            (long)ticksPerWorkHour * WorkHoursPerDay * WorkDaysPerWeek * WeeksPerPayrollCycle;

        public static bool IsPayrollTick(long tick, int ticksPerWorkHour)
        {
            long cycle = GetTicksPerCycle(ticksPerWorkHour);
            return cycle > 0 && tick % cycle == 0;
        }

        public static long NextPayrollTick(long currentTick, int ticksPerWorkHour)
        {
            long cycle = GetTicksPerCycle(ticksPerWorkHour);
            return ((currentTick / cycle) + 1) * cycle;
        }

        public static long TicksUntilNextPayroll(long currentTick, int ticksPerWorkHour) =>
            NextPayrollTick(currentTick, ticksPerWorkHour) - currentTick;
    }
}
