using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public EmployeeRole Role { get; set; }

    public DepartmentType Department { get; set; }

    public decimal Salary { get; set; }

    public int Morale { get; set; } = 50;

    public int Productivity { get; set; } = 50;

    public bool IsAssigned { get; set; }
}
