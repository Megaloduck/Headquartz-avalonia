using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.App.Models;

public class EventViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; }
        = string.Empty;

    public string Description { get; set; }
        = string.Empty;

    public EventSeverity Severity { get; set; }

    public int RemainingTicks { get; set; }
}
