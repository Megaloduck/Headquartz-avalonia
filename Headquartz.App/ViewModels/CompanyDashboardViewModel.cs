using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;

using Headquartz.App.Services;

namespace Headquartz.App.ViewModels;

public partial class CompanyDashboardViewModel
    : ViewModelBase
{
    private readonly SimulationService
        _simulation;

    [ObservableProperty]
    private decimal cash;

    [ObservableProperty]
    private decimal revenue;

    [ObservableProperty]
    private decimal expenses;

    [ObservableProperty]
    private int reputation;

    [ObservableProperty]
    private int employeeCount;

    [ObservableProperty]
    private int activeTasks;

    [ObservableProperty]
    private int activeOrders;

    [ObservableProperty]
    private long tick;

    public CompanyDashboardViewModel(
        SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            HandleSimulationUpdated;

        Refresh();
    }

    private void HandleSimulationUpdated()
    {
        Refresh();
    }

    private void Refresh()
    {
        var company =
            _simulation.Engine.Company;

        Cash =
            company.Cash;

        Revenue =
            company.Revenue;

        Expenses =
            company.Expenses;

        Reputation =
            company.Reputation;

        EmployeeCount =
            company.Employees.Count;

        ActiveTasks =
            company.Tasks.Count;

        ActiveOrders =
            company.Orders.Count;

        Tick =
            _simulation.Engine.Clock.Tick;
    }
}
    