using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.App.ViewModels;

namespace Headquartz.App.Services;

public class NavigationService
{
    private readonly SimulationService
        _simulation;

    public ViewModelBase? CurrentView
    {
        get;
        private set;
    }

    public event Action?
        OnViewChanged;

    public NavigationService(
        SimulationService simulation)
    {
        _simulation = simulation;
    }

    public void Navigate(
        string route)
    {
        CurrentView =
            route switch
            {
                "company" =>
                    new CompanyViewModel(_simulation),

                "forecast" =>
                    new ForecastViewModel(),

                "hr/dashboard" =>
                    new HRDashboardViewModel(_simulation),

                "finance/dashboard" =>
                    new FinanceDashboardViewModel(_simulation),

                "sales/dashboard" =>
                    new SalesDashboardViewModel(_simulation),

                "marketing/dashboard" =>
                    new MarketingDashboardViewModel(),

                "production/dashboard" =>
                    new ProductionDashboardViewModel(_simulation),

                "warehouse/dashboard" =>
                    new WarehouseDashboardViewModel(),

                "logistics/dashboard" =>
                    new LogisticsDashboardViewModel(),

                _ =>
                    new NotFoundViewModel()
            };

        OnViewChanged?.Invoke();
    }
}