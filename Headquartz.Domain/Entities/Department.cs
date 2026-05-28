using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Domain.Entities;

public class Department
{
    public DepartmentType Type { get; set; }

    public decimal Budget { get; set; }

    public int Efficiency { get; set; } = 50;

    public int StressLevel { get; set; }

    public bool IsOperational { get; set; } = true;
}