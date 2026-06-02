using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class TrainingProgramModel : ObservableObject
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Effect { get; set; } = "";
    public decimal Cost { get; set; }
    public string Emoji { get; set; } = "";
}

public partial class HRDevelopmentViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private int _employeeCount;
    [ObservableProperty] private int _avgMorale;
    [ObservableProperty] private int _avgProductivity;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<TrainingProgramModel> Programs { get; } = [];
    public ObservableCollection<string> Log { get; } = [];

    public HRDevelopmentViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        BuildPrograms();
        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void RunMoraleWorkshop() => RunTraining("Morale Workshop", 2_000m, emp => { emp.Morale = Math.Min(100, emp.Morale + 15); });

    [RelayCommand]
    private void RunProductivityTraining() => RunTraining("Productivity Training", 3_000m, emp => { emp.Productivity = Math.Min(100, emp.Productivity + 10); });

    [RelayCommand]
    private void RunStressManagement() => RunTraining("Stress Management", 1_500m, emp => {
        emp.Morale = Math.Min(100, emp.Morale + 8);
        emp.Productivity = Math.Min(100, emp.Productivity + 5);
    });

    [RelayCommand]
    private void RunLeadershipProgram() => RunTraining("Leadership Program", 5_000m, emp =>
    {
        if (emp.Role is EmployeeRole.Supervisor or EmployeeRole.Manager or EmployeeRole.Director)
        {
            emp.Morale = Math.Min(100, emp.Morale + 12);
            emp.Productivity = Math.Min(100, emp.Productivity + 12);
        }
        else
        {
            emp.Morale = Math.Min(100, emp.Morale + 5);
            emp.Productivity = Math.Min(100, emp.Productivity + 5);
        }
    });

    // ── Internal ──────────────────────────────────────────────

    private void RunTraining(string name, decimal cost, Action<Headquartz.Domain.Entities.Employee> effect)
    {
        var company = _simulation.Engine.Company;

        if (company.Cash < cost)
        {
            StatusMessage = $"❌ Insufficient funds for {name}. Need ${cost:N0}.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        int count = 0;
        foreach (var emp in company.Employees)
        {
            effect(emp);
            count++;
        }

        string entry = $"[Tick {_simulation.Engine.Clock.Tick}] {name} — {count} employees trained. Cost: ${cost:N0}.";
        Log.Insert(0, entry);
        if (Log.Count > 20) Log.RemoveAt(Log.Count - 1);

        StatusMessage = $"✅ {name} completed. {count} employees trained.";
        Refresh();
    }

    private void BuildPrograms()
    {
        Programs.Clear();
        Programs.Add(new TrainingProgramModel
        {
            Name = "Morale Workshop",
            Description = "Team-building activities and wellness sessions.",
            Effect = "All employees: +15 Morale",
            Cost = 2_000m,
            Emoji = "🎉",
        });
        Programs.Add(new TrainingProgramModel
        {
            Name = "Productivity Training",
            Description = "Skills development and workflow optimisation.",
            Effect = "All employees: +10 Productivity",
            Cost = 3_000m,
            Emoji = "📈",
        });
        Programs.Add(new TrainingProgramModel
        {
            Name = "Stress Management",
            Description = "Mindfulness and workload balance sessions.",
            Effect = "All employees: +8 Morale, +5 Productivity",
            Cost = 1_500m,
            Emoji = "🧘",
        });
        Programs.Add(new TrainingProgramModel
        {
            Name = "Leadership Program",
            Description = "Advanced leadership for supervisors and managers.",
            Effect = "Leaders: +12 Morale & Productivity / Others: +5",
            Cost = 5_000m,
            Emoji = "🏆",
        });
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        CompanyCash = company.Cash;
        EmployeeCount = company.Employees.Count;
        AvgMorale = company.Employees.Count > 0 ? (int)company.Employees.Average(e => e.Morale) : 0;
        AvgProductivity = company.Employees.Count > 0 ? (int)company.Employees.Average(e => e.Productivity) : 0;

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Employees", Value = EmployeeCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Avg Morale", Value = $"{AvgMorale}%" });
        Kpis.Add(new KpiCardModel { Title = "Avg Productivity", Value = $"{AvgProductivity}%" });
        Kpis.Add(new KpiCardModel { Title = "Company Cash", Value = $"${CompanyCash:N0}" });
    }
}