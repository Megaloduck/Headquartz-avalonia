using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Headquartz.App.Models;

public partial class SidebarItem : ObservableObject
{
    public string Title { get; set; } = "";

    public string Icon { get; set; } = "";

    public string Route { get; set; } = "";

    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// When > 0, a badge bubble is shown next to the menu item.
    /// Driven by the ShellViewModel which maps routes to alert counts.
    /// </summary>
    [ObservableProperty]
    private int _notificationCount;

    /// <summary>Computed — hides badge when count is zero.</summary>
    public bool HasNotification => NotificationCount > 0;

    partial void OnNotificationCountChanged(int value)
    {
        OnPropertyChanged(nameof(HasNotification));
    }
}