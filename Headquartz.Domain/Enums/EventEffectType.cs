using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Domain.Enums;

/// <summary>
/// The type of numerical effect a calendar event applies to a department or company metric.
/// </summary>
public enum EventEffectType
{
    /// <summary>No direct numerical effect.</summary>
    None,

    /// <summary>Direct cash inflow or outflow (flat amount).</summary>
    CashModifier,

    /// <summary>Multiplier on revenue generated during the event.</summary>
    RevenueMultiplier,

    /// <summary>Multiplier on expenses during the event.</summary>
    ExpenseMultiplier,

    /// <summary>Flat or percentage change to employee morale.</summary>
    MoraleModifier,

    /// <summary>Multiplier on employee productivity.</summary>
    ProductivityMultiplier,

    /// <summary>Multiplier on department efficiency.</summary>
    EfficiencyMultiplier,

    /// <summary>Flat budget increase or decrease for a department.</summary>
    BudgetModifier,

    /// <summary>Reputation point change.</summary>
    ReputationModifier,

    /// <summary>Accelerates inventory consumption or restocking.</summary>
    InventoryDemandModifier,

    /// <summary>Increases probability of spontaneous order generation.</summary>
    OrderGenerationRate,

    /// <summary>Generates a special task for the target department.</summary>
    TaskGeneration,

    /// <summary>Toggles department operational status (e.g., shutdown).</summary>
    OperationalToggle,

    /// <summary>Payroll-specific effect (e.g., holiday pay rates, delay).</summary>
    PayrollModifier,
}
