using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Domain.Entities;

/// <summary>
/// A department's ask for funds beyond its current Budget allocation.
/// Created by RequestBudgetIncreaseCommand, resolved by
/// ReviewBudgetRequestCommand. Nothing auto-approves — Finance reviews
/// every request case-by-case, which is the point: it's a real
/// coordination gate between departments, not a formality.
/// </summary>
public class BudgetRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DepartmentType Department { get; set; }

    public string RequestedBy { get; set; } = "";       

    public decimal Amount { get; set; }

    public string Reason { get; set; } = "";

    public BudgetRequestStatus Status { get; set; } = BudgetRequestStatus.Pending;

    public long RequestedAtTick { get; set; }

    public long? ReviewedAtTick { get; set; }

    public string ReviewNote { get; set; } = "";
}