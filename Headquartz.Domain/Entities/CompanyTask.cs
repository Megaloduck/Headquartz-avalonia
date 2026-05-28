using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Domain.Entities;

public class CompanyTask
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public DepartmentType Department { get; set; }

    public TaskPriority Priority { get; set; }

    public CompanyTaskStatus Status { get; set; }

    public int RequiredEmployees { get; set; }

    public int AssignedEmployees { get; set; }

    public double Progress { get; set; }

    public int DurationTicks { get; set; }

    public int RemainingTicks { get; set; }

    public decimal BudgetCost { get; set; }

    public bool IsBlocked { get; set; }
}