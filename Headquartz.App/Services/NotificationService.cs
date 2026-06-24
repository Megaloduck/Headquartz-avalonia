using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.App.Models;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Systems;

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
