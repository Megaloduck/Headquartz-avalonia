using CommunityToolkit.Mvvm.ComponentModel;

using Headquartz.Simulation.Systems;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Headquartz.App.ViewModels;

public partial class MainWindowViewModel
    : ViewModelBase
{
    private readonly SimulationEngine _simulation;

    [ObservableProperty]
    private string _companyName = "";

    [ObservableProperty]
    private decimal _cash;

    [ObservableProperty]
    private int _reputation;

    [ObservableProperty]
    private int _employeeCount;

    [ObservableProperty]
    private string _worldTime = "";

    [ObservableProperty]
    private long _tick;

    public MainWindowViewModel(
        SimulationEngine simulation)
    {
        _simulation = simulation;

        _simulation.OnUpdated += () =>
        {
            Dispatcher.UIThread.Post(
                UpdateState
            );
        };

        Task.Run(_simulation.StartAsync);

        UpdateState();
    }

    private void UpdateState()
    {
        var company = _simulation.Company;

        CompanyName = company.Name;

        Cash = company.Cash;

        Reputation = company.Reputation;

        EmployeeCount = company.Employees.Count;

        Tick = _simulation.Clock.Tick;

        WorldTime =
            _simulation.Clock.WorldTime
                .ToString("yyyy-MM-dd HH:mm");
    }
}