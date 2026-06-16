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

    // Sub-tick timer — fires every 100 ms to animate the partial pill
    private readonly DispatcherTimer _subTickTimer;

    // Timestamp of when the most recent simulation tick completed
    private DateTime _lastTickAt = DateTime.UtcNow;

    // =========================================================
    // THEME
    // =========================================================

    [ObservableProperty] private bool _isDarkTheme = true;

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

    /// <summary>Formatted in-game date, e.g. "22 September 2026".</summary>
    [ObservableProperty] private string _dateLabel = "";

    /// <summary>
    /// Ticks per work hour from SimulationProfile — drives the tick bar's segment count.
    /// Trainee=4  Manager=5  Director=6  Chairman=7
    /// </summary>
    [ObservableProperty] private int _ticksPerWorkHour = 5;

    /// <summary>Whole completed ticks in the current work hour (0 .. TicksPerWorkHour).</summary>
    [ObservableProperty] private int _ticksElapsedInWorkHour;

    /// <summary>
    /// 0.0–1.0. How far the current (in-progress) tick pill is filled.
    /// Updated every 100 ms based on real elapsed ms since last tick.
    /// </summary>
    [ObservableProperty] private double _tickPartialFillRatio;

    /// <summary>Whole work hours completed in the current day (0 .. 8).</summary>
    [ObservableProperty] private int _workHoursElapsedInDay;

    // =========================================================
    // LEGACY — kept so any existing bindings don't break
    // =========================================================

    [ObservableProperty] private string _worldDate = "";
    [ObservableProperty] private long _currentTick;
    [ObservableProperty] private double _tickDayProgress;
    [ObservableProperty] private double _workWeekProgress;

    // =========================================================
    // SHELL STATE
    // =========================================================

    [ObservableProperty] private PlayerRole _currentRole = PlayerRole.HumanResourcesManager;

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

        // Simulation tick — reset the ms clock and refresh static values
        _simulation.Engine.OnUpdated += OnSimulationTick;

        _simulation.Engine.OnUpdated += () =>
            Dispatcher.UIThread.Post(PruneOldNotifications);

        IsDarkTheme = ThemeService.Instance.IsDark;
        CompanyName = _simulation.Engine.Company.Name;
        TicksPerWorkHour = _simulation.Engine.Profile.TicksPerWorkHour;

        // Sub-tick timer — 100 ms resolution for smooth partial-pill animation
        _subTickTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(100),
            DispatcherPriority.Render,
            OnSubTickTimer);

        _subTickTimer.Start();

        CurrentRole = startingRole;
        LoadSidebar();
        _navigation.Navigate("company", startingRole);
        RefreshStaticTickValues();
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

        _simulation.Engine.OnUpdated += OnSimulationTick;

        _simulation.Engine.OnUpdated += () =>
            Dispatcher.UIThread.Post(PruneOldNotifications);

        IsDarkTheme = ThemeService.Instance.IsDark;
        CompanyName = _simulation.Engine.Company.Name;
        TicksPerWorkHour = _simulation.Engine.Profile.TicksPerWorkHour;

        _subTickTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(100),
            DispatcherPriority.Render,
            OnSubTickTimer);

        _subTickTimer.Start();

        RefreshStaticTickValues();
    }

    // =========================================================
    // TICK UPDATES
    // =========================================================

    /// <summary>
    /// Called by SimulationEngine.OnUpdated (every tick = 5 real seconds).
    /// Resets the ms timestamp so the partial-pill animation restarts cleanly,
    /// then refreshes all the stable per-tick values.
    /// </summary>
    private void OnSimulationTick()
    {
        // Record exactly when this tick fired so elapsed-ms is accurate
        _lastTickAt = DateTime.UtcNow;

        Dispatcher.UIThread.Post(() =>
        {
            RefreshStaticTickValues();
            // Reset partial ratio instantly so the pill doesn't jump backwards
            TickPartialFillRatio = 0.0;
        });
    }

    /// <summary>
    /// Updates everything that only changes once per tick:
    /// date label, completed-tick counts, work-hour counts, badges.
    /// </summary>
    private void RefreshStaticTickValues()
    {
        var clock = _simulation.Engine.Clock;
        var profile = _simulation.Engine.Profile;

        CurrentTick = clock.Tick;
        CompanyName = _simulation.Engine.Company.Name;
        DateLabel = clock.WorldTime.ToString("dd MMMM yyyy");
        WorldDate = DateLabel;

        // ── Tick bar (whole pills) ────────────────────────────
        TicksElapsedInWorkHour = (int)(clock.Tick % profile.TicksPerWorkHour);

        // ── Work hours bar ────────────────────────────────────
        int ticksPerDay = profile.TicksPerWorkHour * profile.WorkHoursPerDay;
        long tickOfDay = clock.Tick % ticksPerDay;
        WorkHoursElapsedInDay = (int)(tickOfDay / profile.TicksPerWorkHour);

        // ── Legacy progress values ────────────────────────────
        TickDayProgress = (double)TicksElapsedInWorkHour / profile.TicksPerWorkHour;
        WorkWeekProgress = (double)WorkHoursElapsedInDay / profile.WorkHoursPerDay;

        RefreshNotificationBadges();
    }

    /// <summary>
    /// Fires every 100 ms. Computes how many ms have elapsed since the last
    /// tick and updates TickPartialFillRatio so the in-progress pill
    /// animates smoothly between 0.0 and 1.0 over 5 real seconds.
    /// </summary>
    private void OnSubTickTimer(object? sender, EventArgs e)
    {
        double tickDelayMs = _simulation.Engine.Profile.TickDelayMs;
        double elapsedMs = (DateTime.UtcNow - _lastTickAt).TotalMilliseconds;

        // Clamp to [0, 1] — the simulation tick will reset _lastTickAt before
        // this can overshoot significantly, but guard anyway.
        TickPartialFillRatio = Math.Clamp(elapsedMs / tickDelayMs, 0.0, 1.0);
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

    private void HandleViewChanged() => CurrentView = _navigation.CurrentView;

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