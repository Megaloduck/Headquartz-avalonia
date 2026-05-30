using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class NotificationModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Severity { get; set; } = "info";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string BackgroundColor { get; set; } = "#1F2937";
    public string BorderColor { get; set; } = "#374151";
}