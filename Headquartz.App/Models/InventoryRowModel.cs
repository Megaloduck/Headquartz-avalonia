using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class InventoryRowModel
{
    public Guid ItemId { get; set; }
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public decimal UnitCost { get; set; }
    public bool IsLow { get; set; }
    public double FillPercent { get; set; }
}