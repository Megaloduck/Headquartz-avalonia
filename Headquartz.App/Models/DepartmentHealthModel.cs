using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class DepartmentHealthModel
{
    public string Name { get; set; } = "";
    public string Emoji { get; set; } = "";
    public int Efficiency { get; set; }
    public int StressLevel { get; set; }
    public bool IsOperational { get; set; }
    public int StaffCount { get; set; }
    public string StatusText { get; set; } = "";
}   
