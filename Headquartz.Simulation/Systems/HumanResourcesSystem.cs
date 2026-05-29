using System;
using System.Collections.Generic;
using System.Linq;

using Headquartz.Domain.Entities;
using Headquartz.Simulation.Events;

namespace Headquartz.Simulation.Systems;

public class HumanResourcesSystem : ISimulationSystem
{
    private const int ResignationMoraleThreshold = 10;
    private const double ResignationChancePerTick = 0.08;
    private const int LowMoraleProductivityDrain = 2;
    private const int HighMoraleProductivityGain = 1;

    public void Update(SimulationEngine engine)
    {
        UpdateMoraleAndProductivity(engine);
        ProcessResignations(engine);
        UpdateDepartmentStress(engine);
    }

    // =========================================================
    // MORALE & PRODUCTIVITY
    // =========================================================

    private static void UpdateMoraleAndProductivity(
        SimulationEngine engine)
    {
        foreach (var employee in engine.Company.Employees)
        {
            // Morale decay / recovery
            employee.Morale += employee.IsAssigned ? -1 : 1;

            // Unresolved dept events hurt morale extra
            int unresolvedEvents = engine.Company.Events
                .Count(e =>
                    e.Department == employee.Department &&
                    !e.IsResolved);

            employee.Morale -= unresolvedEvents;

            employee.Morale = Math.Clamp(employee.Morale, 0, 100);

            // Productivity follows morale
            if (employee.Morale < 20)
                employee.Productivity -= LowMoraleProductivityDrain;
            else if (employee.Morale > 60)
                employee.Productivity += HighMoraleProductivityGain;

            employee.Productivity =
                Math.Clamp(employee.Productivity, 0, 100);
        }
    }

    // =========================================================
    // RESIGNATIONS
    // =========================================================

    private static void ProcessResignations(
        SimulationEngine engine)
    {
        var resignees = engine.Company.Employees
            .Where(e =>
                e.Morale <= ResignationMoraleThreshold &&
                Random.Shared.NextDouble() < ResignationChancePerTick)
            .ToList();

        foreach (var employee in resignees)
        {
            engine.Company.Employees.Remove(employee);

            engine.Events.Publish(
                new EmployeeResignedEvent
                {
                    Employee = employee
                });
        }
    }

    // =========================================================
    // DEPARTMENT STRESS FROM UNDERSTAFFING
    // =========================================================

    private static void UpdateDepartmentStress(
        SimulationEngine engine)
    {
        foreach (var dept in engine.Company.Departments)
        {
            int staffCount = engine.Company.Employees
                .Count(e => e.Department == dept.Type);

            // Understaffed departments accumulate stress
            if (staffCount == 0)
                dept.StressLevel = Math.Min(100, dept.StressLevel + 10);
            else if (staffCount < 2)
                dept.StressLevel = Math.Min(100, dept.StressLevel + 3);
        }
    }
}