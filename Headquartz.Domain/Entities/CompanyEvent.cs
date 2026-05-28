using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Domain.Entities;

public class CompanyEvent
{
    public Guid Id { get; set; }
        = Guid.NewGuid();

    public string Title { get; set; }
        = string.Empty;

    public string Description { get; set; }
        = string.Empty;

    public EventSeverity Severity { get; set; }
        = EventSeverity.Low;

    public DepartmentType Department { get; set; }

    public bool IsResolved { get; set; }

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public int RemainingTicks { get; set; }
}
