using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Systems;

namespace Headquartz.Simulation.Commands;

/// <summary>
/// Shared spend-guard so department-funded commands (hiring, task
/// creation, equipment, stock, routes, etc.) draw against the requesting
/// department's own Budget, not just the company's global Cash. Modeled
/// on the existing pattern of pulling shared logic into one place (see
/// SeedInventory / SeedDepartments delegation) rather than duplicating
/// the department lookup in every command.
///
/// This is what makes Finance's allocation actually mean something: a
/// department that has spent its Budget down can't just keep drawing on
/// Cash — it has to go through RequestBudgetIncreaseCommand and get
/// Finance to approve a top-up via ReviewBudgetRequestCommand.
/// </summary>
public static class DepartmentBudgetGuard
{
    public static Department? Find(SimulationEngine engine, DepartmentType department) =>
        engine.Company.Departments.FirstOrDefault(d => d.Type == department);

    /// <summary>
    /// True only if the department exists and its remaining Budget covers
    /// the cost. Callers are responsible for checking company-wide Cash
    /// separately where relevant — this guard only speaks to the
    /// department's own pool.
    /// </summary>
    public static bool CanAfford(SimulationEngine engine, DepartmentType department, decimal cost)
    {
        var dept = Find(engine, department);
        return dept != null && dept.Budget >= cost;
    }

    /// <summary>
    /// Deducts cost from the department's Budget. Call from Execute()
    /// only, after Validate() has already confirmed CanAfford — this does
    /// not re-check, and floors at zero rather than going negative.
    /// </summary>
    public static void Spend(SimulationEngine engine, DepartmentType department, decimal cost)
    {
        var dept = Find(engine, department);
        if (dept != null)
            dept.Budget = Math.Max(0, dept.Budget - cost);
    }
}