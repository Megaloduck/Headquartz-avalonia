using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class ReportMetricModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string SubLabel { get; set; } = "";
    public bool IsAlert { get; set; }
}
