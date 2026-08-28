using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Systems;

namespace Headquartz.Simulation.Commands;

/// <summary>
/// HR-only action: fulfills a pending WorkforceRequest by actually
/// hiring someone into the requesting department. Runs the same
/// solvency/budget gate as HireEmployeeCommand — the new hire's salary
/// still has to clear both company Cash and the requesting department's
/// own Budget, so a workforce request doesn't quietly bypass the budget
/// system the department is otherwise bound by.
/// </summary>
public class FulfillWorkforceRequestCommand
    : ICompanyCommand
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public string PlayerId { get; set; } =
        "";

    public Guid RequestId { get; set; }

    public string EmployeeName { get; set; } =
        "";

    public decimal Salary { get; set; }

    public bool Validate(
        SimulationEngine engine)
    {
        var request = engine.Company.WorkforceRequests
            .FirstOrDefault(r =>
                r.Id == RequestId &&
                r.Status == WorkforceRequestStatus.Pending);

        if (request == null)
            return false;

        decimal buffer = Salary * 2;

        return engine.Company.Cash >= buffer &&
               DepartmentBudgetGuard.CanAfford(engine, request.RequestingDepartment, buffer);
    }

    public void Execute(
        SimulationEngine engine)
    {
        var request = engine.Company.WorkforceRequests
            .FirstOrDefault(r =>
                r.Id == RequestId &&
                r.Status == WorkforceRequestStatus.Pending);

        if (request == null)
            return;

        var employee = new Employee
        {
            Id = Guid.NewGuid(),

            Name = EmployeeName,

            Role = request.Role,

            Department = request.RequestingDepartment,

            Salary = Salary,

            Morale = 70,

            Productivity = 70,
        };

        engine.Company.Employees.Add(employee);

        engine.Company.Cash -= Salary;

        DepartmentBudgetGuard.Spend(engine, request.RequestingDepartment, Salary);

        request.Status = WorkforceRequestStatus.Fulfilled;
        request.ResolvedAtTick = engine.Clock.Tick;
        request.HiredEmployeeId = employee.Id;

        engine.Events.Publish(
            new WorkforceRequestReviewedEvent { Request = request });
    }
}