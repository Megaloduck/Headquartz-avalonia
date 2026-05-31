using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class LoanRowModel
{
    public Guid Id { get; set; }
    public decimal Principal { get; set; }
    public decimal TotalOwed { get; set; }
    public decimal InterestPaid { get; set; }
    public decimal InterestRate { get; set; }
    public long TakenAtTick { get; set; }
    public string TakenAtTime { get; set; } = "";
    public bool IsRepaid { get; set; }
}