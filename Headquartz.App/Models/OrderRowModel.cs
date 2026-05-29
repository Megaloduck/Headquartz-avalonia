using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class OrderRowModel
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public string Status { get; set; } = "";
    public string Deadline { get; set; } = "";
    public bool IsOverdue { get; set; }
    public decimal Revenue { get; set; }
}
