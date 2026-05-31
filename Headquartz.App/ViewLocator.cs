using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Headquartz.App.ViewModels;
using Headquartz.App.Views;

namespace Headquartz.App;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        return data switch
        {
            CompanyViewModel => new CompanyView(),
            ForecastViewModel => new ForecastView(),
            SettingsViewModel => new SettingsView(),

            HRDashboardViewModel => new HRDashboardView(),
            HREmployeeManagementViewModel => new HREmployeeManagementView(),
            HRRecruitmentViewModel => new HRRecruitmentView(),
            HRPayrollViewModel => new HRPayrollView(),

            FinanceDashboardViewModel => new FinanceDashboardView(),
            FinanceBudgetAllocationViewModel => new FinanceBudgetAllocationView(),
            FinanceLoansViewModel => new FinanceLoansView(),

            SalesDashboardViewModel => new SalesDashboardView(),
            SalesOrdersViewModel => new SalesOrdersView(),

            MarketingDashboardViewModel => new MarketingDashboardView(),

            ProductionDashboardViewModel => new ProductionDashboardView(),
            ProductionWorkOrdersViewModel => new ProductionWorkOrdersView(),
            ProductionMaintenanceViewModel => new ProductionMaintenanceView(),

            WarehouseDashboardViewModel => new WarehouseDashboardView(),
            WarehouseInventoryViewModel => new WarehouseInventoryView(),

            LogisticsDashboardViewModel => new LogisticsDashboardView(),
            LogisticsShipmentsViewModel => new LogisticsShipmentsView(),

            NotFoundViewModel => new NotFoundView(),

            _ => new TextBlock { Text = "View Not Found" }
        };
    }

    public bool Match(object? data) => data is ViewModelBase;
}