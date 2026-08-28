    using System;
using System.Collections.Generic;
using System.Text;

using System;

using Headquartz.Domain.Enums;

namespace Headquartz.Domain.Entities;

/// <summary>
/// A department's ask for a new hire — either reacting to a shortfall
/// (a blocked task, an understaffed line) or strategic headcount
/// planning. Created by RequestWorkforceCommand. HR is the only
/// department that can act on it, via FulfillWorkforceRequestCommand
/// (which actually hires) or DeclineWorkforceRequestCommand.
///
/// Deliberately doesn't carry a proposed Salary — HR sets that at
/// fulfillment time, the same way a real hiring manager decides comp
/// when they act on a headcount request rather than the requester
/// dictating it upfront.
/// </summary>
public class WorkforceRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DepartmentType RequestingDepartment { get; set; }

    public string RequestedBy { get; set; } = "";

    public EmployeeRole Role { get; set; }

    public string Reason { get; set; } = "";

    public WorkforceRequestStatus Status { get; set; } = WorkforceRequestStatus.Pending;

    public long RequestedAtTick { get; set; }

    public long? ResolvedAtTick { get; set; }

    /// <summary>Set once fulfilled — links back to the Employee that was hired for this request.</summary>
    public Guid? HiredEmployeeId { get; set; }

    public string ReviewNote { get; set; } = "";
}