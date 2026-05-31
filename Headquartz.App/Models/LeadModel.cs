using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class LeadModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CompanyName { get; set; } = "";
    public string ProductInterest { get; set; } = "";
    public decimal EstimatedValue { get; set; }
    public string Stage { get; set; } = "New";
    public string CreatedAt { get; set; } = "";
    public int TickAge { get; set; }
}
