using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class ForecastDataPoint
{
    public long Tick { get; set; }
    public string WorldTime { get; set; } = "";
    public decimal ProjectedCash { get; set; }
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public bool IsNegative { get; set; }
    public bool IsPayrollTick { get; set; }
    public string CashDisplay { get; set; } = "";
}