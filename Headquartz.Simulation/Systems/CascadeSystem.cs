using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;

namespace Headquartz.Simulation.Systems;

/// <summary>
/// Resolves cross-department stress propagation and
/// cascade failures each simulation tick.
///
/// Cascade chain:
///   HR stress      → spreads to ALL departments
///   Finance stress → spreads to ALL departments
///   Production     → Warehouse
///   Warehouse      → Logistics
///   Logistics      → Reputation + Order failures
///   Marketing eff  → Reputation (low marketing = less demand)
/// </summary>
public class CascadeSystem : ISimulationSystem
{
    public void Update(SimulationEngine engine) =>
        Update(engine, cascadeMultiplier: 1.0);

    /// <summary>
    /// Profile-aware update. Pass SimulationProfile.CascadeMultiplier
    /// to scale all stress propagation amounts.
    /// </summary>
    public void Update(SimulationEngine engine, double cascadeMultiplier)
    {
        UpdateDepartmentStress(engine);
        PropagateCrossChain(engine, cascadeMultiplier);
        ApplyEfficiencyFromStress(engine);
        EmitCrisisEvents(engine);
        PruneResolvedEvents(engine);
    }

    // =========================================================
    // STRESS ACCUMULATION PER DEPARTMENT
    // =========================================================

    private static void UpdateDepartmentStress(SimulationEngine engine)
    {
        foreach (var dept in engine.Company.Departments)
        {
            int blockedTasks = engine.Company.Tasks
                .Count(t =>
                    t.Department == dept.Type &&
                    t.IsBlocked);

            int unresolvedEvents = engine.Company.Events
                .Count(e =>
                    e.Department == dept.Type &&
                    !e.IsResolved);

            int stressDelta =
                (blockedTasks * 4) +
                (unresolvedEvents * 2);

            if (stressDelta > 0)
                dept.StressLevel =
                    Math.Min(100, dept.StressLevel + stressDelta);
            else
                dept.StressLevel =
                    Math.Max(0, dept.StressLevel - 1);
        }
    }

    // =========================================================
    // CROSS-DEPARTMENT PROPAGATION
    // =========================================================

    private static void PropagateCrossChain(
        SimulationEngine engine,
        double multiplier)
    {
        var company = engine.Company;

        var hr = Dept(company, DepartmentType.HumanResources);
        var finance = Dept(company, DepartmentType.Finance);
        var production = Dept(company, DepartmentType.Production);
        var warehouse = Dept(company, DepartmentType.Warehouse);
        var logistics = Dept(company, DepartmentType.Logistics);
        var marketing = Dept(company, DepartmentType.Marketing);

        // HR crisis spreads to all departments
        if (hr?.StressLevel > 60)
        {
            int spread = Scale((hr.StressLevel - 60) / 10, multiplier);
            foreach (var dept in company.Departments
                         .Where(d => d.Type != DepartmentType.HumanResources))
                dept.StressLevel =
                    Math.Min(100, dept.StressLevel + spread);
        }

        // Finance crisis spreads to all departments
        if (finance?.StressLevel > 70)
        {
            int spread = Scale((finance.StressLevel - 70) / 10, multiplier);
            foreach (var dept in company.Departments
                         .Where(d => d.Type != DepartmentType.Finance))
                dept.StressLevel =
                    Math.Min(100, dept.StressLevel + spread);
        }

        // Production → Warehouse
        if (production?.StressLevel > 60 && warehouse != null)
            warehouse.StressLevel =
                Math.Min(100, warehouse.StressLevel + Scale(5, multiplier));

        // Warehouse → Logistics
        if (warehouse?.StressLevel > 60 && logistics != null)
            logistics.StressLevel =
                Math.Min(100, logistics.StressLevel + Scale(5, multiplier));

        // Logistics failure → reputation drain
        if (logistics?.StressLevel > 75)
            company.Reputation =
                Math.Max(0, company.Reputation - Scale(1, multiplier));

        // Low Marketing efficiency → reputation decay
        if (marketing?.Efficiency < 30)
            company.Reputation =
                Math.Max(0, company.Reputation - Scale(1, multiplier));
    }

    // =========================================================
    // EFFICIENCY FROM STRESS
    // =========================================================

    private static void ApplyEfficiencyFromStress(SimulationEngine engine)
    {
        foreach (var dept in engine.Company.Departments)
        {
            if (dept.StressLevel > 80)
                dept.Efficiency = Math.Max(10, dept.Efficiency - 3);
            else if (dept.StressLevel > 50)
                dept.Efficiency = Math.Max(20, dept.Efficiency - 1);
            else if (dept.StressLevel < 30)
                dept.Efficiency = Math.Min(100, dept.Efficiency + 1);

            dept.IsOperational = dept.StressLevel < 95;
        }
    }

    // =========================================================
    // CRISIS EVENT EMISSION
    // =========================================================

    private static void EmitCrisisEvents(SimulationEngine engine)
    {
        foreach (var dept in engine.Company.Departments)
        {
            if (dept.StressLevel >= 90 &&
                Random.Shared.NextDouble() < 0.3)
            {
                engine.Events.Publish(
                    new DepartmentCrisisEvent
                    {
                        Department = dept.Type,
                        StressLevel = dept.StressLevel
                    });
            }
        }
    }

    // =========================================================
    // CLEANUP OLD EVENTS
    // =========================================================

    private static void PruneResolvedEvents(SimulationEngine engine)
    {
        foreach (var ev in engine.Company.Events
                     .Where(e => !e.IsResolved))
        {
            ev.RemainingTicks--;
            if (ev.RemainingTicks <= 0)
                ev.IsResolved = true;
        }

        if (engine.Company.Events.Count > 100)
            engine.Company.Events
                .RemoveAll(e => e.IsResolved);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static Department? Dept(Company company, DepartmentType type) =>
        company.Departments.FirstOrDefault(d => d.Type == type);

    /// <summary>
    /// Applies the difficulty multiplier to a stress amount,
    /// ensuring a minimum of 1 so propagation never disappears.
    /// </summary>
    private static int Scale(int amount, double multiplier) =>
        Math.Max(1, (int)Math.Round(amount * multiplier));
}