using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Commands;

public class HireEmployeeCommand
    : ICompanyCommand
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public string PlayerId { get; set; } =
        "";

    public string EmployeeName { get; set; } =
        "";

    public EmployeeRole Role { get; set; }

    public DepartmentType Department { get; set; }

    public decimal Salary { get; set; }

    public bool Validate(
        SimulationEngine engine)
    {
        return engine.Company.Cash >= Salary * 2;
    }

    public void Execute(
        SimulationEngine engine)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),

            Name = EmployeeName,

            Role = Role,

            Department = Department,

            Salary = Salary,

            Morale = 70,

            Productivity = 70,
        };

        engine.Company.Employees.Add(
            employee);

        engine.Company.Cash -= Salary;
    }
}