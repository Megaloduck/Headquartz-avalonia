using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class TaskRowModel
{
    public Guid TaskId { get; set; }
    public string Name { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Status { get; set; } = "";
    public double Progress { get; set; }
    public int RemainingTicks { get; set; }
    public int Workers { get; set; }
    public bool IsBlocked { get; set; }
}