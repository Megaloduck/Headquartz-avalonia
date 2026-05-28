using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Shared.Networking;

public class CompanySnapshot
{
    public decimal Cash { get; set; }

    public int Reputation { get; set; }

    public int EmployeeCount { get; set; }

    public int TaskCount { get; set; }

    public int OrderCount { get; set; }

    public long Tick { get; set; }

    public DateTime WorldTime { get; set; }
}
