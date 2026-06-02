using System;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;

namespace Headquartz.App.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;
    private readonly SimulationService _simulation;
    private readonly NotificationService _notifications;

    // =========================================================
    // STATE
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

    // RoleSelected is always true when ShellViewModel is active
    // (onboarding is handled by OnboardingShellViewModel now)
    public bool RoleSelected => true;

    public ObservableCollection<SidebarSection> SidebarSections { get; } = [];
    public ObservableCollection<NotificationModel> Notifications { get; } = [];

    // =========================================================
    // CONSTRUCTOR — accepts pre-built simulation + chosen role
    // =========================================================

    public ShellViewModel(
        SimulationService simulation,
        PlayerRole startingRole)
    {
        _simulation = simulation;
        _navigation = new NavigationService(_simulation);

        CurrentRole = startingRole;

        _navigation.OnViewChanged += HandleViewChanged;

        _notifications = new NotificationService(_simulation.Engine);
        _notifications.NotificationFired += OnNotificationFired;

        _simulation.Engine.OnUpdated += () =>
            Dispatcher.UIThread.Post(PruneOldNotifications);

        LoadSidebar();
        _navigation.Navigate("company", CurrentRole);
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
    private void DismissNotification(NotificationModel n)
    {
        Notifications.Remove(n);
    }

    // =========================================================
    // NAVIGATION
    // =========================================================

    [RelayCommand]
    private void Navigate(SidebarItem item)
    {
        _navigation.Navigate(item.Route, CurrentRole);
    }

    [RelayCommand]
    private void NavigateSettings()
    {
        _navigation.Navigate("settings", CurrentRole);
    }

    private void HandleViewChanged()
    {
        CurrentView = _navigation.CurrentView;
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