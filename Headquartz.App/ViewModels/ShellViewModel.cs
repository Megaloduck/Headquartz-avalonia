using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;

using Headquartz.Domain.Enums;

using System.Collections.ObjectModel;

namespace Headquartz.App.ViewModels;

public partial class ShellViewModel
    : ViewModelBase
{
    private readonly NavigationService
        _navigation;

    [ObservableProperty]
    private PlayerRole currentRole =
        PlayerRole.HumanResourcesManager;

    private ViewModelBase? _currentView;

    public ViewModelBase? CurrentView
    {
        get => _currentView;

        set => SetProperty(
            ref _currentView,
            value);
    }

    public ObservableCollection<SidebarItem>
        SidebarItems
    { get; } = [];

    public ShellViewModel()
    {
        var simulation =
     new SimulationService();

        _navigation =
            new NavigationService(
                simulation);

        _ = simulation.StartAsync();
        _navigation.OnViewChanged +=
            HandleViewChanged;

        LoadSidebar();

        _navigation.Navigate(
            "company");
    }

    partial void OnCurrentRoleChanged(
        PlayerRole value)
    {
        LoadSidebar();
    }

    [RelayCommand]
    private void Navigate(
        SidebarItem item)
    {
        _navigation.Navigate(
            item.Route);
    }

    private void HandleViewChanged()
    {
        CurrentView =
            _navigation.CurrentView;
    }

    private void LoadSidebar()
    {
        SidebarItems.Clear();

        var items =
            SidebarService.GetSidebar(
                CurrentRole);

        foreach (var item in items)
        {
            SidebarItems.Add(item);
        }
    }
}