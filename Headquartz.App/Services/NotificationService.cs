using Headquartz.App.Models;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Phase;
using Headquartz.Simulation.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Services;

public class NotificationService
{
    public event Action<NotificationModel>? NotificationFired;

    public NotificationService(SimulationEngine engine)
    {
        engine.Events.Subscribe<PayrollFailedEvent>(e =>
            Fire("💸 Payroll Failed",
                 $"Shortfall: ${e.Shortfall:N0}. Employee morale will crash.",
                 "critical", "BrushAlertCriticalBg", "BrushDanger",
                 DepartmentType.Finance));

        engine.Events.Subscribe<EmployeeResignedEvent>(e =>
            Fire("👤 Employee Resigned",
                 $"{e.Employee.Name} ({e.Employee.Department}) left due to low morale.",
                 "high", "BrushAlertWarningBg", "BrushWarning",
                 e.Employee.Department));

        engine.Events.Subscribe<OrderFailedEvent>(e =>
            Fire("📦 Order Cancelled",
                 $"Missed deadline for {e.Order.ClientName}. Reputation −3.",
                 "medium", "BrushAlertInfoBg", "BrushInfo",
                 DepartmentType.Sales));

        engine.Events.Subscribe<CashCrisisEvent>(e =>
            Fire("🚨 Cash Crisis",
                 $"Balance: ${e.CashBalance:N0}. Budget cuts triggered.",
                 "critical", "BrushAlertCriticalBg", "BrushDanger",
                 DepartmentType.Finance));

        engine.Events.Subscribe<DepartmentCrisisEvent>(e =>
            Fire($"⚠ {e.Department} Crisis",
                 $"Stress at {e.StressLevel}%. Efficiency degrading.",
                 "high", "BrushAlertCriticalBg", "BrushDanger",
                 e.Department));

        // ── Budget requests (Finance AP review workflow) ────────────
        //
        // Submission notifies Finance specifically — they're the ones
        // with something to act on. The review result notifies the
        // requesting department, whichever way it went, so the wait
        // doesn't disappear into silence.

        engine.Events.Subscribe<BudgetRequestSubmittedEvent>(e =>
            Fire("📋 Budget Request Submitted",
                 $"{e.Request.Department} requested ${e.Request.Amount:N0} — {e.Request.Reason}",
                 "medium", "BrushAlertInfoBg", "BrushInfo",
                 DepartmentType.Finance));

        engine.Events.Subscribe<BudgetRequestReviewedEvent>(e =>
            Fire(
                e.Request.Status == BudgetRequestStatus.Approved
                    ? "✅ Budget Request Approved"
                    : "❌ Budget Request Denied",
                e.Request.Status == BudgetRequestStatus.Approved
                    ? $"${e.Request.Amount:N0} released to {e.Request.Department}."
                    : $"${e.Request.Amount:N0} request for {e.Request.Department} denied."
                      + (string.IsNullOrWhiteSpace(e.Request.ReviewNote) ? "" : $" — {e.Request.ReviewNote}"),
                e.Request.Status == BudgetRequestStatus.Approved ? "low" : "high",
                e.Request.Status == BudgetRequestStatus.Approved ? "BrushBadgeSuccessBg" : "BrushAlertWarningBg",
                e.Request.Status == BudgetRequestStatus.Approved ? "BrushSuccess" : "BrushWarning",
                e.Request.Department));

        // ── Workforce requests (HR fulfillment workflow) ────────────
        //
        // Submission notifies HR — they're the ones with a hire to make
        // or decline. The outcome notifies the requesting department,
        // whether it was filled or turned down.

        engine.Events.Subscribe<WorkforceRequestSubmittedEvent>(e =>
            Fire("🧑‍💼 Workforce Request Submitted",
                 $"{e.Request.RequestingDepartment} needs a {e.Request.Role} — {e.Request.Reason}",
                 "medium", "BrushAlertInfoBg", "BrushInfo",
                 DepartmentType.HumanResources));

        engine.Events.Subscribe<WorkforceRequestReviewedEvent>(e =>
            Fire(
                e.Request.Status == WorkforceRequestStatus.Fulfilled
                    ? "✅ Workforce Request Fulfilled"
                    : "❌ Workforce Request Declined",
                e.Request.Status == WorkforceRequestStatus.Fulfilled
                    ? $"HR hired a {e.Request.Role} for {e.Request.RequestingDepartment}."
                    : $"HR declined the {e.Request.Role} request for {e.Request.RequestingDepartment}."
                      + (string.IsNullOrWhiteSpace(e.Request.ReviewNote) ? "" : $" — {e.Request.ReviewNote}"),
                e.Request.Status == WorkforceRequestStatus.Fulfilled ? "low" : "high",
                e.Request.Status == WorkforceRequestStatus.Fulfilled ? "BrushBadgeSuccessBg" : "BrushAlertWarningBg",
                e.Request.Status == WorkforceRequestStatus.Fulfilled ? "BrushSuccess" : "BrushWarning",
                e.Request.RequestingDepartment));

        // ── Company lifecycle ────────────────────────────────────────

        engine.Events.Subscribe<CompanyEnteredGrandOpeningEvent>(_ =>
            Fire("🎉 Grand Opening!",
                 "The founding phase is over — the full simulation is live. Random events, cascading stress, and auto-generated work are now switched on.",
                 "medium", "BrushBadgeSuccessBg", "BrushSuccess",
                 DepartmentType.Management));
    }

    private void Fire(
        string title,
        string message,
        string severity,
        string bgKey,
        string borderKey,
        DepartmentType department)
    {
        NotificationFired?.Invoke(new NotificationModel
        {
            Title = title,
            Message = message,
            Severity = severity,
            BackgroundKey = bgKey,
            BorderKey = borderKey,
            Department = department,
        });
    }
}