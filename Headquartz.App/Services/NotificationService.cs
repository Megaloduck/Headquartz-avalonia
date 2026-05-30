using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.App.Models;
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
                 "critical", "#7F1D1D", "#EF4444"));

        engine.Events.Subscribe<EmployeeResignedEvent>(e =>
            Fire("👤 Employee Resigned",
                 $"{e.Employee.Name} ({e.Employee.Department}) left due to low morale.",
                 "high", "#78350F", "#F97316"));

        engine.Events.Subscribe<OrderFailedEvent>(e =>
            Fire("📦 Order Cancelled",
                 $"Missed deadline for {e.Order.ClientName}. Reputation −3.",
                 "medium", "#1E3A5F", "#3B82F6"));

        engine.Events.Subscribe<CashCrisisEvent>(e =>
            Fire("🚨 Cash Crisis",
                 $"Balance: ${e.CashBalance:N0}. Budget cuts triggered.",
                 "critical", "#7F1D1D", "#EF4444"));

        engine.Events.Subscribe<DepartmentCrisisEvent>(e =>
            Fire($"⚠ {e.Department} Crisis",
                 $"Stress at {e.StressLevel}%. Efficiency degrading.",
                 "high", "#3B0764", "#8B5CF6"));
    }

    private void Fire(
        string title,
        string message,
        string severity,
        string bg,
        string border)
    {
        NotificationFired?.Invoke(new NotificationModel
        {
            Title = title,
            Message = message,
            Severity = severity,
            BackgroundColor = bg,
            BorderColor = border,
        });
    }
}
