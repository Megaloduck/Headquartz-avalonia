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
            "forecast" => new ForecastViewModel(_simulation),
            "settings" => new SettingsViewModel(_simulation),

            "hr/dashboard" => new HRDashboardViewModel(_simulation),
            "hr/employees" => new HREmployeeManagementViewModel(_simulation),
            "hr/recruitment" => new HRRecruitmentViewModel(_simulation),
            "hr/payroll" => new HRPayrollViewModel(_simulation),

            "finance/dashboard" => new FinanceDashboardViewModel(_simulation),
            "finance/budget" => new FinanceBudgetAllocationViewModel(_simulation),
            "finance/loans" => new FinanceLoansViewModel(_simulation),

            "sales/dashboard" => new SalesDashboardViewModel(_simulation),
            "sales/orders" => new SalesOrdersViewModel(_simulation),

            "marketing/dashboard" => new MarketingDashboardViewModel(_simulation),

            "production/dashboard" => new ProductionDashboardViewModel(_simulation),
            "production/workorders" => new ProductionWorkOrdersViewModel(_simulation),
            "production/maintenance" => new ProductionMaintenanceViewModel(_simulation),

            "warehouse/dashboard" => new WarehouseDashboardViewModel(_simulation),
            "warehouse/inventory" => new WarehouseInventoryViewModel(_simulation),

            "logistics/dashboard" => new LogisticsDashboardViewModel(_simulation),
            "logistics/shipments" => new LogisticsShipmentsViewModel(_simulation),

            _ => new NotFoundViewModel(),
        };

        OnViewChanged?.Invoke();
    }
}