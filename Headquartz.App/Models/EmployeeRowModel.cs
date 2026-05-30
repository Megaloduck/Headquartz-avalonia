using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class EmployeeRowModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Department { get; set; } = "";
    public string Role { get; set; } = "";
    public int Morale { get; set; }
    public int Productivity { get; set; }
    public decimal Salary { get; set; }
    public bool IsAssigned { get; set; }
    public bool IsLowMorale { get; set; }
    public string MoraleStatus { get; set; } = "";
}
