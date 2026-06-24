using System;
using Headquartz.Domain.Enums;

namespace Headquartz.App.Models;

public class NotificationModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Severity { get; set; } = "info";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string BackgroundKey { get; set; } = "BrushAlertInfoBg";
    public string BorderKey { get; set; } = "BrushInfo";
    public DepartmentType Department { get; set; } = DepartmentType.Management;
}