using System;
using System.Collections.Generic;
using System.Linq;

using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;

namespace Headquartz.Simulation.Systems;

/// <summary>
/// Simulation system that manages scheduled calendar events (holidays, sales, payroll, black swan).
/// Checks the current simulation date each tick, fires events, and applies department effects.
/// </summary>
public class CalendarEventSystem
{
    private readonly List<ActiveEvent> _activeEvents = [];
    private readonly HashSet<string> _firedToday = [];

    private DateTime _lastCheckedDate = DateTime.MinValue;

    /// <summary>
    /// Events currently active (for UI display and effect resolution).
    /// </summary>
    public IReadOnlyList<ActiveEvent> ActiveEvents => _activeEvents.AsReadOnly();

    // =========================================================
    // UPDATE (called once per tick by SimulationEngine)
    // =========================================================

    public void Update(SimulationEngine engine)
    {
        var today = engine.Clock.WorldTime.Date;

        // Reset daily tracking when the date changes
        if (today != _lastCheckedDate)
        {
            _firedToday.Clear();
            _lastCheckedDate = today;
        }

        // 1. Check for scheduled events that start today
        foreach (var def in CalendarEventRegistry.GetActiveForDate(today))
        {
            if (_firedToday.Contains(def.Id)) continue;

            _firedToday.Add(def.Id);
            StartEvent(engine, def, today);
        }

        // 2. Check for random black-swan events
        foreach (var def in CalendarEventRegistry.Definitions.Where(d => d.IsRandom))
        {
            if (def.RandomChancePerTick.HasValue
                && Random.Shared.NextDouble() < def.RandomChancePerTick.Value)
            {
                // Random events can fire multiple times; use a tick-based key
                string instanceKey = $"{def.Id}-{engine.Clock.Tick}";
                if (_firedToday.Contains(instanceKey)) continue;
                _firedToday.Add(instanceKey);

                StartEvent(engine, def, today);
            }
        }

        // 3. Tick down active events and apply persistent effects
        ApplyActiveEffects(engine);

        // 4. Remove expired events
        _activeEvents.RemoveAll(a => a.RemainingTicks <= 0);
    }

    // =========================================================
    // EVENT START
    // =========================================================

    private void StartEvent(SimulationEngine engine, CalendarEventDefinition def, DateTime startDate)
    {
        var endDate = startDate.AddDays(def.DurationDays);

        var active = new ActiveEvent
        {
            Definition = def,
            StartDate = startDate,
            EndDate = endDate,
            RemainingTicks = (int)(def.DurationDays * 96 * engine.Profile.EventDurationMultiplier),
        };

        _activeEvents.Add(active);

        // Publish to EventBus so other systems can react
        engine.Events.Publish(new CalendarEventTriggered
        {
            Definition = def,
            StartDate = startDate,
            EndDate = endDate,
        });

        // Create a CompanyEvent so it appears in the company events log
        engine.Company.Events.Add(new CompanyEvent
        {
            Title = def.Title,
            Description = $"{def.Campaign}: {def.Description}",
            Severity = def.Severity,
            Department = def.DepartmentEffects.FirstOrDefault()?.Department ?? DepartmentType.Management,
            RemainingTicks = active.RemainingTicks,
            CreatedAt = engine.Clock.WorldTime,
        });
    }

    // =========================================================
    // EFFECT APPLICATION
    // =========================================================

    private void ApplyActiveEffects(SimulationEngine engine)
    {
        foreach (var active in _activeEvents)
        {
            active.RemainingTicks--;

            foreach (var effect in active.Definition.DepartmentEffects)
            {
                ApplyDepartmentEffect(engine, effect);
            }
        }
    }

    private void ApplyDepartmentEffect(SimulationEngine engine, DepartmentEffect effect)
    {
        var company = engine.Company;

        switch (effect.EffectType)
        {
            case EventEffectType.CashModifier:
                // Magnitude is a flat amount (negative for outflow)
                // We apply it per tick, scaled down so it's not instant bankruptcy
                company.Cash += (decimal)(effect.Magnitude * 100);
                break;

            case EventEffectType.RevenueMultiplier:
                // Applied during revenue generation; store in a temporary state
                // For now, apply a small cash boost proportional to the multiplier
                if (effect.Magnitude > 1.0)
                {
                    company.Cash += (decimal)((effect.Magnitude - 1.0) * 50);
                    company.Revenue += (decimal)((effect.Magnitude - 1.0) * 50);
                }
                else if (effect.Magnitude < 1.0)
                {
                    company.Cash -= (decimal)((1.0 - effect.Magnitude) * 50);
                }
                break;

            case EventEffectType.ExpenseMultiplier:
                if (effect.Magnitude > 1.0)
                {
                    company.Cash -= (decimal)((effect.Magnitude - 1.0) * 30);
                    company.Expenses += (decimal)((effect.Magnitude - 1.0) * 30);
                }
                break;

            case EventEffectType.MoraleModifier:
                foreach (var emp in company.Employees
                    .Where(e => e.Department == effect.Department))
                {
                    emp.Morale = Math.Clamp(
                        emp.Morale + (int)effect.Magnitude,
                        0, 100);
                }
                break;

            case EventEffectType.ProductivityMultiplier:
                foreach (var emp in company.Employees
                    .Where(e => e.Department == effect.Department))
                {
                    int delta = (int)((effect.Magnitude - 1.0) * 20);
                    emp.Productivity = Math.Clamp(emp.Productivity + delta, 0, 100);
                }
                break;

            case EventEffectType.EfficiencyMultiplier:
                var dept = company.Departments
                    .FirstOrDefault(d => d.Type == effect.Department);
                if (dept != null)
                {
                    int delta = (int)((effect.Magnitude - 1.0) * 20);
                    dept.Efficiency = Math.Clamp(dept.Efficiency + delta, 0, 100);
                }
                break;

            case EventEffectType.BudgetModifier:
                var budgetDept = company.Departments
                    .FirstOrDefault(d => d.Type == effect.Department);
                if (budgetDept != null)
                {
                    budgetDept.Budget += (decimal)effect.Magnitude * 100;
                    budgetDept.Budget = Math.Max(0, budgetDept.Budget);
                }
                break;

            case EventEffectType.ReputationModifier:
                company.Reputation = Math.Clamp(
                    company.Reputation + (int)effect.Magnitude,
                    0, 100);
                break;

            case EventEffectType.OperationalToggle:
                var opDept = company.Departments
                    .FirstOrDefault(d => d.Type == effect.Department);
                if (opDept != null)
                {
                    opDept.IsOperational = effect.Magnitude >= 1.0;
                }
                break;

            case EventEffectType.PayrollModifier:
                // Payroll effects are handled by the main payroll logic in SimulationEngine.
                // For now, apply a small morale bump or cash adjustment.
                foreach (var emp in company.Employees
                    .Where(e => e.Department == effect.Department))
                {
                    emp.Morale = Math.Clamp(emp.Morale + 1, 0, 100);
                }
                break;

            case EventEffectType.TaskGeneration:
                // Tasks are generated by the main task system; this is a hint.
                // We add a small budget adjustment to represent preparation costs.
                company.Expenses += (decimal)effect.Magnitude * 200;
                break;

            case EventEffectType.InventoryDemandModifier:
                // Consume or restock inventory based on demand
                foreach (var item in company.Inventory)
                {
                    int change = (int)((effect.Magnitude - 1.0) * 5);
                    item.Quantity = Math.Max(0, item.Quantity + change);
                }
                break;

            case EventEffectType.OrderGenerationRate:
                // Increase spontaneous order chance
                if (Random.Shared.NextDouble() < (effect.Magnitude - 1.0) * 0.1)
                {
                    // Triggered in the main loop; just prepare reputation
                    company.Reputation = Math.Min(100, company.Reputation + 1);
                }
                break;

            case EventEffectType.None:
            default:
                break;
        }
    }

    // =========================================================
    // NESTED: ActiveEvent
    // =========================================================

    public class ActiveEvent
    {
        public CalendarEventDefinition Definition { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RemainingTicks { get; set; }

        public bool IsActive => RemainingTicks > 0;
    }
}
