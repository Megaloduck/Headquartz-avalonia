using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Domain.Entities;

public class LoanRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Principal { get; set; }
    public decimal InterestRate { get; set; } = 0.05m;   // 5% per payroll cycle
    public decimal TotalOwed { get; set; }
    public decimal InterestPaid { get; set; }
    public long TakenAtTick { get; set; }
    public bool IsRepaid { get; set; }
}