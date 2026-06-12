using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;
    private readonly SimulationService _simulation;
    private readonly NotificationService _notifications;

    // =========================================================
    // THEME
    // =========================================================

    [ObservableProperty]
    private bool _isDarkTheme = true;

    [RelayCommand]
    private void ToggleTheme()
    {
        ThemeService.Instance.Toggle();
        IsDarkTheme = ThemeService.Instance.IsDark;
    }

    // =========================================================
    // TOP BAR
    // =========================================================

    /// <summary>Current page title shown in the top bar.</summary>
    [ObservableProperty]
    private string _pageTitle = "Company Overview";

    /// <summary>Current section label (e.g. "Overview", "Managements").</summary>
    [ObservableProperty]
    private string _sectionLabel = "Overview";

    [ObservableProperty]
    private string _companyName = "Company Name";

    // =========================================================
    // BOTTOM TICK WIDGET
    // =========================================================

    [ObservableProperty]
    private string _worldDate = "";

    /// <summary>0-1 progress of the current tick within the day (8 ticks/day).</summary>
    [ObservableProperty]
    private double _tickDayProgress;

    /// <summary>0-1 progress of work hours within the current week (40 ticks/week).</summary>
    [ObservableProperty]
    private double _workWeekProgress;

    [ObservableProperty]
    private long _currentTick;

    // =========================================================
    // SHELL STATE
    // =========================================================

    [ObservableProperty]
    private PlayerRole _currentRole = PlayerRole.HumanResourcesManager;

    private string _roleName = "";
    public string RoleName
    {
        get => _roleName;
        set => SetProperty(ref _roleName, value);
    }

    private ViewModelBase? _currentView;
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public ObservableCollection<SidebarSection> SidebarSections { get; } = [];
    public ObservableCollection<NotificationModel> Notifications { get; } = [];

    // =========================================================
    // CONSTRUCTORS
    // =========================================================

    /// <summary>Post-onboarding constructor — role already chosen.</summary>
    public ShellViewModel(SimulationService simulation, PlayerRole startingRole)
    {
        _simulation = simulation;
        _navigation = new NavigationService(_simulation);
        _navigation.OnViewChanged += HandleViewChanged;

        _notifications = new NotificationService(_simulation.Engine);
        _notifications.NotificationFired += OnNotificationFired;

        _simulation.Engine.OnUpdated += () =>
            Dispatcher.UIThread.Post(RefreshTickWidget);

        _simulation.Engine.OnUpdated += () =>
            Dispatcher.UIThread.Post(PruneOldNotifications);

        IsDarkTheme = ThemeService.Instance.IsDark;
        CompanyName = _simulation.Engine.Company.Name;

        CurrentRole = startingRole;
        LoadSidebar();
        _navigation.Navigate("company", startingRole);
        RefreshTickWidget();
    }

    /// <summary>Parameterless — design-time / standalone use.</summary>
    public ShellViewModel()
    {
        _simulation = new SimulationService();
        _navigation = new NavigationService(_simulation);

        _ = _simulation.StartAsync();

        _navigation.OnViewChanged += HandleViewChanged;

        _notifications = new NotificationService(_simulation.Engine);
        _notifications.NotificationFired += OnNotificationFired;

        _simulation.Engine.OnUpdated += () =>
            Dispatcher.UIThread.Post(RefreshTickWidget);

        _simulation.Engine.OnUpdated += () =>
            Dispatcher.UIThread.Post(PruneOldNotifications);

        IsDarkTheme = ThemeService.Instance.IsDark;
        CompanyName = _simulation.Engine.Company.Name;
        RefreshTickWidget();
    }

    // =========================================================
    // TICK WIDGET REFRESH
    // =========================================================

    private void RefreshTickWidget()
    {
        var clock = _simulation.Engine.Clock;
        CurrentTick = clock.Tick;
        CompanyName = _simulation.Engine.Company.Name;

        // Display date — skip time portion
        WorldDate = clock.WorldTime.ToString("dd MMMM yyyy");

        // Tick within current day (8 ticks = 1 day)
        long tickOfDay = clock.Tick % 8;
        TickDayProgress = tickOfDay / 8.0;

        // Ticks within current work week (40 ticks = 1 week)
        long tickOfWeek = clock.Tick % 40;
        WorkWeekProgress = tickOfWeek / 40.0;

        // Update notification badges on sidebar items
        RefreshNotificationBadges();
    }

    // =========================================================
    // NOTIFICATION BADGES
    // =========================================================

    private void RefreshNotificationBadges()
    {
        var company = _simulation.Engine.Company;

        foreach (var section in SidebarSections)
        {
            foreach (var item in section.Items)
            {
                item.NotificationCount = item.Route switch
                {
                    "hr/dashboard" or "hr/employees" =>
                        company.Employees.Count(e => e.Morale <= 15),

                    "hr/payroll" =>
                        company.Cash < company.Employees.Sum(e => e.Salary) ? 1 : 0,

                    "finance/dashboard" =>
                        company.Cash < 0 ? 1 : 0,

                    "finance/loans" =>
                        company.Loans.Count(l => !l.IsRepaid),

                    "sales/dashboard" or "sales/orders" =>
                        company.Orders.Count(o =>
                            o.DeliveryDeadline.HasValue &&
                            _simulation.Engine.Clock.WorldTime > o.DeliveryDeadline.Value &&
                            o.Status is not Domain.Enums.OrderStatus.Delivered
                                     and not Domain.Enums.OrderStatus.Cancelled),

                    "production/dashboard" or "production/workorders" =>
                        company.Tasks.Count(t =>
                            t.Department == Domain.Enums.DepartmentType.Production &&
                            t.IsBlocked),

                    "warehouse/dashboard" or "warehouse/inventory" =>
                        company.Inventory.Count(i => i.Quantity < i.MinimumStock),

                    "logistics/dashboard" or "logistics/shipments" =>
                        company.Orders.Count(o =>
                            o.DeliveryDeadline.HasValue &&
                            _simulation.Engine.Clock.WorldTime > o.DeliveryDeadline.Value &&
                            o.Status == Domain.Enums.OrderStatus.Shipping),

                    _ => 0,
                };
            }
        }
    }

    // =========================================================
    // NOTIFICATIONS
    // =========================================================

    private void OnNotificationFired(NotificationModel notification)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Notifications.Insert(0, notification);
            while (Notifications.Count > 4)
                Notifications.RemoveAt(Notifications.Count - 1);
        });
    }

    private void PruneOldNotifications()
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-6);
        var stale = Notifications.Where(n => n.CreatedAt < cutoff).ToList();
        foreach (var n in stale)
            Notifications.Remove(n);
    }

    [RelayCommand]
    private void DismissNotification(NotificationModel n) =>
        Notifications.Remove(n);

    // =========================================================
    // NAVIGATION
    // =========================================================

    [RelayCommand]
    private void Navigate(SidebarItem item)
    {
        _navigation.Navigate(item.Route, CurrentRole);
        UpdateBreadcrumb(item);
    }

    [RelayCommand]
    private void NavigateSettings()
    {
        _navigation.Navigate("settings", CurrentRole);
        PageTitle = "Settings";
        SectionLabel = "System";
    }

    private void HandleViewChanged()
    {
        CurrentView = _navigation.CurrentView;
    }

    private void UpdateBreadcrumb(SidebarItem item)
    {
        PageTitle = item.Title;

        SectionLabel = SidebarSections
            .FirstOrDefault(s => s.Items.Contains(item))
            ?.Title ?? "";
    }

    // =========================================================
    // SIDEBAR
    // =========================================================

    partial void OnCurrentRoleChanged(PlayerRole value) => LoadSidebar();

    private void LoadSidebar()
    {
        SidebarSections.Clear();

        RoleName = CurrentRole switch
        {
            PlayerRole.HumanResourcesManager => "HR Manager",
            PlayerRole.FinanceManager => "Finance Manager",
            PlayerRole.SalesManager => "Sales Manager",
            PlayerRole.MarketingManager => "Marketing Manager",
            PlayerRole.ProductionManager => "Production Manager",
            PlayerRole.WarehouseManager => "Warehouse Manager",
            PlayerRole.LogisticsManager => "Logistics Manager",
            PlayerRole.Chairman => "Board Chairman",
            _ => "Unknown Role",
        };

        foreach (var section in SidebarService.GetSections(CurrentRole))
            SidebarSections.Add(section);
    }
}