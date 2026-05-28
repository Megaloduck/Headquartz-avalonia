using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Domain.Entities;

public class InventoryItem
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public int MinimumStock { get; set; }

    public int MaximumStock { get; set; }
}
