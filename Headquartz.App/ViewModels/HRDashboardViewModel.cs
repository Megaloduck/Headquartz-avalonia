using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Headquartz.App.ViewModels;

public partial class HRDashboardViewModel
    : ViewModelBase
{
    private readonly SimulationService
        _simulation;

    public ObservableCollection<KpiCardModel>
        Kpis
    { get; }
            = [];

    public ObservableCollection<string>
        PendingTasks
    { get; }
            = [];

    public ObservableCollection<string>
        Employees
    { get; }
            = [];

    public ObservableCollection<EventViewModel>
        Events
    { get; }
        = [];

    [ObservableProperty]
    private int morale;

    [ObservableProperty]
    private int burnout;

    [ObservableProperty]
    private int hiringProgress;

    public HRDashboardViewModel(
        SimulationService simulation)
    {
        _simulation =
            simulation;

        Refresh();
    }

    [RelayCommand]
    private void HireEmployee()
    {
        HiringProgress += 10;

        Refresh();
    }

    [RelayCommand]
    private void StartTraining()
    {
        Morale += 5;

        Refresh();
    }

    private void Refresh()
    {
        Kpis.Clear();

        Kpis.Add(
            new KpiCardModel
            {
                Title = "Employees",
                Value =
                    _simulation.Engine
                        .Company
                        .Employees
                        .Count
                        .ToString()
            });

        Kpis.Add(
            new KpiCardModel
            {
                Title = "Morale",
                Value = $"{Morale}%"
            });

        Kpis.Add(
            new KpiCardModel
            {
                Title = "Burnout",
                Value = $"{Burnout}%"
            });

        Kpis.Add(
            new KpiCardModel
            {
                Title = "Hiring",
                Value = $"{HiringProgress}%"
            });

        PendingTasks.Clear();

        PendingTasks.Add(
            "Review Employee Complaints");

        PendingTasks.Add(
            "Process New Applicants");

        PendingTasks.Add(
            "Schedule Team Training");

        Employees.Clear();

        foreach (var employee
                 in _simulation.Engine
                     .Company
                     .Employees)
        {
            Employees.Add(
                employee.Name);
        }
        Events.Clear();

        foreach (var companyEvent
                 in _simulation.Engine
                     .Company
                     .Events
                     .Where(x =>
                         x.Department ==
                         Headquartz.Domain.Enums.DepartmentType.HumanResources
                         && !x.IsResolved))
        {
            Events.Add(
                new EventViewModel
                {
                    Id =
                        companyEvent.Id,

                    Title =
                        companyEvent.Title,

                    Description =
                        companyEvent.Description,

                    Severity =
                        companyEvent.Severity,

                    RemainingTicks =
                        companyEvent.RemainingTicks
                });
        }
    }
}
