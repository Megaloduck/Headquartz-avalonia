using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class BudgetRowModel
{
    public string DepartmentName { get; set; } = "";
    public decimal Budget { get; set; }
    public int Efficiency { get; set; }
    public int StressLevel { get; set; }
    public string StatusText { get; set; } = "";
}
