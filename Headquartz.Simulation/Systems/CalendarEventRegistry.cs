using System;
using System.Collections.Generic;
using System.Linq;

using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Systems;

/// <summary>
/// Static registry of all scheduled calendar events used in the Headquartz simulation.
/// Events are defined once and reused across playthroughs.
/// </summary>
public static class CalendarEventRegistry
{
    private static readonly List<CalendarEventDefinition> _definitions = [];

    static CalendarEventRegistry()
    {
        BuildDefinitions();
    }

    public static IReadOnlyList<CalendarEventDefinition> Definitions => _definitions.AsReadOnly();

    public static CalendarEventDefinition? GetById(string id) =>
        _definitions.FirstOrDefault(d => d.Id == id);

    public static IEnumerable<CalendarEventDefinition> GetActiveForDate(DateTime date) =>
        _definitions.Where(d => d.IsActiveOn(date));

    // =========================================================
    // BUILDER
    // =========================================================

    private static void BuildDefinitions()
    {
        // ── Q1 January ─────────────────────────────────────────

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "grand-opening",
            Title = "Grand Opening",
            Campaign = "Fresh Start",
            Description = "Launch of the company in the simulator market.",
            ColorTag = "Green",
            Severity = EventSeverity.Medium,
            FixedDate = new DateTime(2026, 1, 1),
            DurationDays = 1,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources,  EffectType = EventEffectType.TaskGeneration,       Magnitude = 1,   Description = "Hiring surge; onboarding overload", DurationTicks = 96 },
                new() { Department = DepartmentType.Finance,          EffectType = EventEffectType.CashModifier,           Magnitude = -1,  Description = "High initial CAPEX; cash-flow negative", DurationTicks = 96 },
                new() { Department = DepartmentType.Sales,            EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.3, Description = "Strong opening interest; early adopters", DurationTicks = 96 },
                new() { Department = DepartmentType.Marketing,       EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.5, Description = "Maximum visibility; brand awareness push", DurationTicks = 96 },
                new() { Department = DepartmentType.Production,       EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.8, Description = "Ramp-up phase; setup lines", DurationTicks = 96 },
                new() { Department = DepartmentType.Warehouse,       EffectType = EventEffectType.InventoryDemandModifier, Magnitude = 1.5, Description = "Initial stock buildup; receiving surge", DurationTicks = 96 },
                new() { Department = DepartmentType.Logistics,        EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.7, Description = "New route establishment; onboarding delays", DurationTicks = 96 },
            ]
        });

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "new-year",
            Title = "New Year",
            Campaign = "New Beginnings",
            Description = "Celebration of the new calendar year.",
            ColorTag = "Blue",
            Severity = EventSeverity.Low,
            FixedDate = new DateTime(2026, 1, 1),
            DurationDays = 1,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.MoraleModifier,       Magnitude = 5,   Description = "Annual review cycle; bonus payouts", DurationTicks = 96 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.CashModifier,           Magnitude = -1,  Description = "Fiscal-year budget allocation; reset counters", DurationTicks = 96 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 0.6, Description = "Post-holiday slump; slow restart", DurationTicks = 96 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.TaskGeneration,         Magnitude = 1,   Description = "New Year campaign launch", DurationTicks = 96 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.OperationalToggle,    Magnitude = 0,   Description = "Resume after holiday shutdown", DurationTicks = 48 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.TaskGeneration,         Magnitude = 1,   Description = "Year-end inventory reconciliation", DurationTicks = 96 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.7, Description = "Holiday backlog clearing", DurationTicks = 96 },
            ]
        });

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "payroll-jan",
            Title = "Payroll Cycle",
            Campaign = "Payroll Run",
            Description = "Monthly salary, wages, and tax disbursement.",
            ColorTag = "Pink",
            Severity = EventSeverity.Low,
            IsRecurring = true,
            RecurringDayOfMonth = 25,
            RecurringExceptMonths = [12],
            DurationDays = 1,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.TaskGeneration,       Magnitude = 1,   Description = "Timesheet validation; payroll processing", DurationTicks = 48 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.CashModifier,           Magnitude = -1,  Description = "Cash outflow; tax withholding; working capital impact", DurationTicks = 48 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.MoraleModifier,         Magnitude = 3,   Description = "Commission payout; morale boost", DurationTicks = 48 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.None,                   Magnitude = 0,   Description = "Minimal direct impact", DurationTicks = 48 },
                new() { Department = DepartmentType.Production,   EffectType = EventEffectType.PayrollModifier,        Magnitude = 1,   Description = "Labor cost accrual; overtime pay", DurationTicks = 48 },
                new() { Department = DepartmentType.Warehouse,      EffectType = EventEffectType.PayrollModifier,        Magnitude = 1,   Description = "Overtime pay for ops staff", DurationTicks = 48 },
                new() { Department = DepartmentType.Logistics,     EffectType = EventEffectType.PayrollModifier,        Magnitude = 1,   Description = "Driver payroll; fuel reimbursements", DurationTicks = 48 },
            ]
        });

        // ── Q1 February ────────────────────────────────────────

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "chinese-new-year",
            Title = "Chinese New Year",
            Campaign = "Prosperity",
            Description = "Major holiday in manufacturing hubs.",
            ColorTag = "Red",
            Severity = EventSeverity.Medium,
            FixedDate = new DateTime(2026, 2, 10), // Approximate for 2026
            DurationDays = 3,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.MoraleModifier,       Magnitude = 8,   Description = "Cultural leave; holiday staffing", DurationTicks = 288 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.CashModifier,           Magnitude = -1,  Description = "Red-envelope budget; bonus accrual", DurationTicks = 288 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.4, Description = "Pre-holiday buying surge", DurationTicks = 96 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.2, Description = "Cultural/red-theme campaigns", DurationTicks = 288 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.OperationalToggle,    Magnitude = 0,   Description = "Shutdown / reduced output in hubs", DurationTicks = 192 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.InventoryDemandModifier, Magnitude = 1.8, Description = "Pre-shipping rush; outbound spike", DurationTicks = 96 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.4, Description = "Severe Asia-route delays; port congestion", DurationTicks = 288 },
            ]
        });

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "valentines-day",
            Title = "Valentine's Day",
            Campaign = "Spread the Love",
            Description = "Seasonal gift-giving peak.",
            ColorTag = "Pink",
            Severity = EventSeverity.Low,
            FixedDate = new DateTime(2026, 2, 14),
            DurationDays = 1,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.TaskGeneration,       Magnitude = 1,   Description = "Temporary staffing for seasonal surge", DurationTicks = 96 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.BudgetModifier,         Magnitude = -1,  Description = "Seasonal marketing budget release", DurationTicks = 96 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.5, Description = "Gift category spike", DurationTicks = 96 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.3, Description = "Romance / couples targeting", DurationTicks = 96 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.TaskGeneration,         Magnitude = 1,   Description = "Custom packaging demand", DurationTicks = 96 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.4, Description = "High-velocity picking", DurationTicks = 96 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.2, Description = "Same-day delivery pressure", DurationTicks = 96 },
            ]
        });

        // Payroll for Feb is covered by recurring definition above (IsRecurring = true, RecurringDayOfMonth = 25)

        // ── Q1 March ───────────────────────────────────────────
        // Only Payroll (recurring)

        // ── Q2 April ───────────────────────────────────────────

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "ramadan-fest",
            Title = "Ramadan Fest",
            Campaign = "Blessing Month",
            Description = "Extended period of community consumption.",
            ColorTag = "Green",
            Severity = EventSeverity.Medium,
            FixedDate = new DateTime(2026, 4, 1), // Approximate start
            DurationDays = 30,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.MoraleModifier,       Magnitude = 5,   Description = "Flexible hours; cultural observance", DurationTicks = 2880 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.CashModifier,           Magnitude = -1,  Description = "Charity contributions; zakat outflow", DurationTicks = 2880 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.2, Description = "Food / beverage pattern shift; night sales", DurationTicks = 2880 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.2, Description = "Community / family-focused campaigns", DurationTicks = 2880 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.1, Description = "Pre-dawn meal prep production", DurationTicks = 2880 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.8, Description = "Night-shift operations; fasting-hour adjustments", DurationTicks = 2880 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.9, Description = "Iftar delivery timing windows", DurationTicks = 2880 },
            ]
        });

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "happy-easter",
            Title = "Happy Easter",
            Campaign = "Spring Renewal",
            Description = "Family gathering and travel period.",
            ColorTag = "Green",
            Severity = EventSeverity.Low,
            FixedDate = new DateTime(2026, 4, 3), // Approximate for 2026
            DurationDays = 3,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.MoraleModifier,       Magnitude = 5,   Description = "Spring break schedules; time-off spike", DurationTicks = 288 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.TaskGeneration,       Magnitude = 1,   Description = "Q1 close / quarterly review", DurationTicks = 288 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.2, Description = "Family gathering products; travel goods", DurationTicks = 288 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.2, Description = "Spring / renewal themes", DurationTicks = 288 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.TaskGeneration,         Magnitude = 1,   Description = "Seasonal SKU build", DurationTicks = 288 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.InventoryDemandModifier, Magnitude = 1.2, Description = "Holiday stock rotation", DurationTicks = 288 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.2, Description = "Travel-route volume", DurationTicks = 288 },
            ]
        });

        // ── Q2 May ─────────────────────────────────────────────

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "labor-day",
            Title = "Labor's Day",
            Campaign = "Worker's Pride",
            Description = "Recognition of the workforce.",
            ColorTag = "Yellow",
            Severity = EventSeverity.Low,
            FixedDate = new DateTime(2026, 5, 1),
            DurationDays = 1,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.MoraleModifier,       Magnitude = 10,  Description = "Employee recognition events; morale ops", DurationTicks = 96 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.BudgetModifier,         Magnitude = -1,  Description = "Wage / benefit audit; labor cost review", DurationTicks = 96 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 0.8, Description = "Holiday promotions", DurationTicks = 96 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.2, Description = "Brand-values storytelling", DurationTicks = 96 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.OperationalToggle,    Magnitude = 0,   Description = "May pause / reduced shifts", DurationTicks = 48 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.7, Description = "Labor-shortage risk", DurationTicks = 96 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.6, Description = "Holiday delivery delays", DurationTicks = 96 },
            ]
        });

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "vesak-day",
            Title = "Vesak Day",
            Campaign = "Inner Peace",
            Description = "Spiritual observance in various regions.",
            ColorTag = "Blue",
            Severity = EventSeverity.Low,
            FixedDate = new DateTime(2026, 5, 3), // Approximate (full moon)
            DurationDays = 1,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.MoraleModifier,       Magnitude = 5,   Description = "Cultural leave; meditation events", DurationTicks = 96 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.CashModifier,           Magnitude = -1,  Description = "Minor impact; local donation", DurationTicks = 96 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 0.8, Description = "Southeast Asia affected", DurationTicks = 96 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.1, Description = "Mindful / peace-themed campaigns", DurationTicks = 96 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.8, Description = "Regional slowdown", DurationTicks = 96 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.8, Description = "Regional delay", DurationTicks = 96 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.7, Description = "Regional congestion", DurationTicks = 96 },
            ]
        });

        // ── Q2 June ────────────────────────────────────────────

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "mid-year-sale",
            Title = "Mid-Year Sale",
            Campaign = "6.6 Mega Sale",
            Description = "Aggressive mid-year inventory clearing.",
            ColorTag = "Red",
            Severity = EventSeverity.High,
            FixedDate = new DateTime(2026, 6, 10),
            DurationDays = 6,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.TaskGeneration,       Magnitude = 2,   Description = "Customer-service surge; overtime approval", DurationTicks = 576 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.5, Description = "Revenue spike; margin squeeze", DurationTicks = 576 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 2.0, Description = "Volume spike; discount-driven", DurationTicks = 576 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.5, Description = "Discount / urgency campaigns", DurationTicks = 576 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.4, Description = "Pre-sale ramp-up", DurationTicks = 576 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.8, Description = "Picking / packing surge", DurationTicks = 576 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.6, Description = "High-volume dispatch", DurationTicks = 576 },
            ]
        });

        // ── Q3 August ──────────────────────────────────────────

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "back-to-school",
            Title = "Back to School",
            Campaign = "Study Hard",
            Description = "Education sector demand surge.",
            ColorTag = "Blue",
            Severity = EventSeverity.Medium,
            FixedDate = new DateTime(2026, 8, 15),
            DurationDays = 16,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.TaskGeneration,       Magnitude = 1,   Description = "Student / temp hiring", DurationTicks = 1536 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.BudgetModifier,         Magnitude = -1,  Description = "Seasonal budget drawdown", DurationTicks = 1536 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.5, Description = "Education / stationery surge", DurationTicks = 1536 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.3, Description = "Parent / student targeting", DurationTicks = 1536 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.4, Description = "Bulk supply production", DurationTicks = 1536 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.InventoryDemandModifier, Magnitude = 1.6, Description = "Back-to-school inventory peak", DurationTicks = 1536 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.3, Description = "Campus / bulk delivery", DurationTicks = 1536 },
            ]
        });

        // ── Q3 September ───────────────────────────────────────

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "supply-chain-disrupt",
            Title = "Supply Chain Disrupt",
            Campaign = "Port Strike",
            Description = "A random 'black swan' simulation event.",
            ColorTag = "Red",
            Severity = EventSeverity.Critical,
            IsRandom = true,
            RandomChancePerTick = 0.0005, // Very rare
            DurationDays = 5,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.TaskGeneration,       Magnitude = 2,   Description = "Crisis-management team activation", DurationTicks = 480 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.CashModifier,           Magnitude = -1,  Description = "Cost overrun; contingency spend", DurationTicks = 480 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 0.3, Description = "Stockout risk; lost sales", DurationTicks = 480 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.6, Description = "Damage control; stock alerts", DurationTicks = 480 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.OperationalToggle,    Magnitude = 0,   Description = "Material shortage; line stops", DurationTicks = 480 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.InventoryDemandModifier, Magnitude = 0.2, Description = "Depleted stock; safety-stock draw", DurationTicks = 480 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.2, Description = "Bottleneck; reroute costs", DurationTicks = 480 },
            ]
        });

        // ── Q4 October ─────────────────────────────────────────

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "halloween-fest",
            Title = "Halloween Fest",
            Campaign = "Spooky Deals",
            Description = "Themed seasonal event.",
            ColorTag = "Purple",
            Severity = EventSeverity.Low,
            FixedDate = new DateTime(2026, 10, 29),
            DurationDays = 3,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.MoraleModifier,       Magnitude = 5,   Description = "Costume events; morale boost", DurationTicks = 288 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.BudgetModifier,         Magnitude = -1,  Description = "Seasonal P&L tracking", DurationTicks = 288 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.3, Description = "Candy / costume spike", DurationTicks = 288 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.4, Description = "Viral / spooky campaigns", DurationTicks = 288 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.TaskGeneration,         Magnitude = 1,   Description = "Seasonal SKU build", DurationTicks = 288 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.3, Description = "Halloween zone layout", DurationTicks = 288 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.2, Description = "Last-mile rush", DurationTicks = 288 },
            ]
        });

        // ── Q4 November ────────────────────────────────────────

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "double-11",
            Title = "Double 11 (11.11)",
            Campaign = "Singles Day",
            Description = "The world's largest e-commerce day.",
            ColorTag = "Red",
            Severity = EventSeverity.Critical,
            FixedDate = new DateTime(2026, 11, 11),
            DurationDays = 1,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.TaskGeneration,       Magnitude = 3,   Description = "All-hands support; 24/7 staffing", DurationTicks = 96 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 3.0, Description = "Cash-flow explosion", DurationTicks = 96 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 4.0, Description = "Massive volume; peak transactions", DurationTicks = 96 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 2.0, Description = "FOMO / flash-sale campaigns", DurationTicks = 96 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.8, Description = "Full capacity; overtime", DurationTicks = 96 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 2.5, Description = "Peak throughput", DurationTicks = 96 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 2.0, Description = "National emergency logistics", DurationTicks = 96 },
            ]
        });

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "black-friday",
            Title = "Black Friday",
            Campaign = "The Big Blitz",
            Description = "Mass-market discount period.",
            ColorTag = "Red",
            Severity = EventSeverity.High,
            FixedDate = new DateTime(2026, 11, 27),
            DurationDays = 4,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.TaskGeneration,       Magnitude = 2,   Description = "Extended shifts; temp surge", DurationTicks = 384 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.ExpenseMultiplier,      Magnitude = 1.5, Description = "Discount margin impact", DurationTicks = 384 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 2.5, Description = "Record volume", DurationTicks = 384 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.6, Description = "Urgency / scarcity campaigns", DurationTicks = 384 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.5, Description = "Fulfillment sprint", DurationTicks = 384 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 2.0, Description = "Maximum throughput", DurationTicks = 384 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.ExpenseMultiplier,      Magnitude = 1.8, Description = "Carrier surge pricing", DurationTicks = 384 },
            ]
        });

        // ── Q4 December ────────────────────────────────────────

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "cyber-monday",
            Title = "Cyber Monday",
            Campaign = "Digital Wave",
            Description = "Post-Black Friday online focus.",
            ColorTag = "Pink",
            Severity = EventSeverity.Medium,
            FixedDate = new DateTime(2026, 12, 2),
            DurationDays = 1,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.TaskGeneration,       Magnitude = 1,   Description = "IT / support desk surge", DurationTicks = 96 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.5, Description = "Online revenue spike", DurationTicks = 96 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.8, Description = "Digital-only surge", DurationTicks = 96 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.4, Description = "Retargeting / email blasts", DurationTicks = 96 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.2, Description = "Print-on-demand sprint", DurationTicks = 96 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.5, Description = "Pick / pack overload", DurationTicks = 96 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.4, Description = "Parcel density surge", DurationTicks = 96 },
            ]
        });

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "christmas",
            Title = "Christmas",
            Campaign = "Holiday Magic",
            Description = "Major global gift-giving peak.",
            ColorTag = "Green",
            Severity = EventSeverity.Medium,
            FixedDate = new DateTime(2026, 12, 25),
            DurationDays = 1,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.PayrollModifier,        Magnitude = 1.5, Description = "Holiday pay scheduling", DurationTicks = 96 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.TaskGeneration,       Magnitude = 1,   Description = "Year-end accounting close", DurationTicks = 96 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 1.8, Description = "Gift peak", DurationTicks = 96 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.3, Description = "Emotional / holiday campaigns", DurationTicks = 96 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.2, Description = "Final gift production", DurationTicks = 96 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.InventoryDemandModifier, Magnitude = 1.8, Description = "Christmas stock peak", DurationTicks = 96 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.5, Description = "Deadline pressure; cutoff dates", DurationTicks = 96 },
            ]
        });

        _definitions.Add(new CalendarEventDefinition
        {
            Id = "year-end",
            Title = "Year End",
            Campaign = "Clearance 202X",
            Description = "Final stock liquidation before taxes.",
            ColorTag = "Yellow",
            Severity = EventSeverity.Medium,
            FixedDate = new DateTime(2026, 12, 26),
            DurationDays = 6,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.TaskGeneration,       Magnitude = 1,   Description = "Performance reviews; comp planning", DurationTicks = 576 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.TaskGeneration,       Magnitude = 2,   Description = "Tax planning; depreciation booking", DurationTicks = 576 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.RevenueMultiplier,      Magnitude = 0.5, Description = "Clearance / markdown", DurationTicks = 576 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 1.2, Description = "Final push campaigns", DurationTicks = 576 },
                new() { Department = DepartmentType.Production,     EffectType = EventEffectType.OperationalToggle,    Magnitude = 0,   Description = "Shutdown / maintenance", DurationTicks = 288 },
                new() { Department = DepartmentType.Warehouse,     EffectType = EventEffectType.TaskGeneration,         Magnitude = 1,   Description = "Inventory write-offs; count", DurationTicks = 576 },
                new() { Department = DepartmentType.Logistics,      EffectType = EventEffectType.EfficiencyMultiplier,   Magnitude = 0.6, Description = "Last shipments of year", DurationTicks = 96 },
            ]
        });

        // December Payroll override (moved to Dec 24 to avoid Christmas conflict)
        _definitions.Add(new CalendarEventDefinition
        {
            Id = "payroll-dec",
            Title = "Payroll Cycle",
            Campaign = "Payroll Run",
            Description = "Monthly salary, wages, and tax disbursement. (Holiday-adjusted to Dec 24.)",
            ColorTag = "Pink",
            Severity = EventSeverity.Low,
            FixedDate = new DateTime(2026, 12, 24),
            DurationDays = 1,
            DepartmentEffects =
            [
                new() { Department = DepartmentType.HumanResources, EffectType = EventEffectType.TaskGeneration,       Magnitude = 1,   Description = "Timesheet validation; payroll processing", DurationTicks = 48 },
                new() { Department = DepartmentType.Finance,        EffectType = EventEffectType.CashModifier,           Magnitude = -1,  Description = "Cash outflow; tax withholding; working capital impact", DurationTicks = 48 },
                new() { Department = DepartmentType.Sales,          EffectType = EventEffectType.MoraleModifier,         Magnitude = 3,   Description = "Commission payout; morale boost", DurationTicks = 48 },
                new() { Department = DepartmentType.Marketing,      EffectType = EventEffectType.None,                   Magnitude = 0,   Description = "Minimal direct impact", DurationTicks = 48 },
                new() { Department = DepartmentType.Production,   EffectType = EventEffectType.PayrollModifier,        Magnitude = 1,   Description = "Labor cost accrual; overtime pay", DurationTicks = 48 },
                new() { Department = DepartmentType.Warehouse,      EffectType = EventEffectType.PayrollModifier,        Magnitude = 1,   Description = "Overtime pay for ops staff", DurationTicks = 48 },
                new() { Department = DepartmentType.Logistics,     EffectType = EventEffectType.PayrollModifier,        Magnitude = 1,   Description = "Driver payroll; fuel reimbursements", DurationTicks = 48 },
            ]
        });
    }
}
