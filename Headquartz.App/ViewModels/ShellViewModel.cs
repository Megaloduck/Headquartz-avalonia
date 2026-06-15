using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;
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

    [ObservableProperty] private string _pageTitle = "Company Overview";
    [ObservableProperty] private string _sectionLabel = "Overview";
    [ObservableProperty] private string _companyName = "Company Name";

    // =========================================================
    // TICK STATUS CARD
    // =========================================================

    /// <summary>Formatted in-game date shown as card header, e.g. "22 September 2026".</summary>
    [ObservableProperty] private string _dateLabel = "";

    /// <summary>
    /// How many ticks make one work hour — sourced from SimulationProfile.
    ///   Trainee=4  Manager=5  Director=6  Chairman=7
    /// Drives the tick bar's segment count.
    /// </summary>
    [ObservableProperty] private int _ticksPerWorkHour = 5;

    /// <summary>
    /// Ticks completed in the current work hour (0 .. TicksPerWorkHour).
    /// Fills one green pill per completed tick.
    /// </summary>
    [ObservableProperty] private int _ticksElapsedInWorkHour;

    /// <summary>
    /// Work hours completed in the current day (0 .. 8).
    /// Fills one cyan pill per completed work hour.
    /// </summary>
    [ObservableProperty] private int _workHoursElapsedInDay;

    // =========================================================
    // LEGACY TICK WIDGET (kept for any other bindings)
    // =========================================================

    [ObservableProperty] private string _worldDate = "";
    [ObservableProperty] private long _currentTick;

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

    /// <summary>Post-onboarding constructor — role and profile already chosen.</summary>
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

        // Read TicksPerWorkHour once from the profile — it never changes at runtime.
        TicksPerWorkHour = _simulation.Engine.Profile.TicksPerWorkHour;

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
        TicksPerWorkHour = _simulation.Engine.Profile.TicksPerWorkHour;

        RefreshTickWidget();
    }

    // =========================================================
    // TICK WIDGET REFRESH
    // =========================================================

    private void RefreshTickWidget()
    {
        var clock = _simulation.Engine.Clock;
        var profile = _simulation.Engine.Profile;

        CurrentTick = clock.Tick;
        CompanyName = _simulation.Engine.Company.Name;

        // In-game date header (no time portion)
        DateLabel = clock.WorldTime.ToString("dd MMMM yyyy");
        WorldDate = DateLabel; // keep legacy binding in sync

        // ── Tick bar ─────────────────────────────────────────
        // Each pill = 1 completed tick within the current work hour.
        // After TicksPerWorkHour ticks the pill count wraps back to 0.
        TicksElapsedInWorkHour = (int)(clock.Tick % profile.TicksPerWorkHour);

        // ── Work hours bar ────────────────────────────────────
        // Each pill = 1 completed work hour within the current day.
        // A full day = WorkHoursPerDay * TicksPerWorkHour ticks.
        int ticksPerDay = profile.TicksPerWorkHour * profile.WorkHoursPerDay;
        long tickOfDay = clock.Tick % ticksPerDay;
        WorkHoursElapsedInDay = (int)(tickOfDay / profile.TicksPerWorkHour);

        // Update notification badges
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