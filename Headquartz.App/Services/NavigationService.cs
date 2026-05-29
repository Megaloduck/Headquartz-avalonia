using Headquartz.App.ViewModels;
using System;

namespace Headquartz.App.Services;

public class NavigationService
{
    private readonly SimulationService _simulation;

    public ViewModelBase? CurrentView { get; private set; }

    public event Action? OnViewChanged;

    public NavigationService(SimulationService simulation)
    {
        _simulation = simulation;
    }

    public void Navigate(string route)
    {
        CurrentView = route switch
        {
            "company" => new CompanyViewModel(_simulation),
            "forecast" => new ForecastViewModel(),

            "hr/dashboard" => new HRDashboardViewModel(_simulation),

            "finance/dashboard" => new FinanceDashboardViewModel(_simulation),

            "sales/dashboard" => new SalesDashboardViewModel(_simulation),

            "marketing/dashboard" => new MarketingDashboardViewModel(_simulation),

            "production/dashboard" => new ProductionDashboardViewModel(_simulation),

            "warehouse/dashboard" => new WarehouseDashboardViewModel(_simulation),

            "logistics/dashboard" => new LogisticsDashboardViewModel(_simulation),

            _ => new NotFoundViewModel(),
        };

        OnViewChanged?.Invoke();
    }
}