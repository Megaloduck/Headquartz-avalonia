using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Schedulers
{
    /// <summary>
    /// Single source of truth for "how many ticks make up one payroll /
    /// loan-interest cycle." Previously this was the literal magic number
    /// 10, duplicated independently in four places (SimulationEngine.
    /// ProcessPayroll, FinanceSystem.ProcessLoanInterest, HRPayrollViewModel,
    /// FinanceAuditsViewModel) — changing one without the others would
    /// desync when payroll actually fires from what the UI tells the
    /// player "next payroll in N ticks."
    ///
    /// ⚠️ TicksPerCycle currently preserves existing behavior (10 ticks)
    /// unchanged. It is NOT yet derived from the design doc's "4 weeks/
    /// month" payroll cadence — that requires resolving an open conflict
    /// first: the design doc states both "8 work hours/day" and "40 work
    /// hours/week (Mon–Thu)," which don't multiply out (8×4=32, not 40).
    /// See Build Guide. Once resolved, recompute this from
    /// TicksPerWorkHour × WorkHoursPerDay × WorkDaysPerWeek × 4 weeks —
    /// this becomes the only line that needs to change.
    /// </summary>
    public static class PayrollSchedule
    {
        public const long TicksPerCycle = 10;

        public static bool IsPayrollTick(long tick) => tick % TicksPerCycle == 0;

        public static long NextPayrollTick(long currentTick) =>
            ((currentTick / TicksPerCycle) + 1) * TicksPerCycle;

        public static long TicksUntilNextPayroll(long currentTick) =>
            NextPayrollTick(currentTick) - currentTick;
    }   
}
