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
            // Main menu
            CompanyViewModel => new CompanyView(),
            ForecastViewModel => new ForecastView(),

            // HR
            HRDashboardViewModel => new HRDashboardView(),
            HREmployeeManagementViewModel => new HREmployeeManagementView(),
            HRRecruitmentViewModel => new HRRecruitmentView(),

            // Finance
            FinanceDashboardViewModel => new FinanceDashboardView(),

            // Sales
            SalesDashboardViewModel => new SalesDashboardView(),

            // Marketing
            MarketingDashboardViewModel => new MarketingDashboardView(),

            // Production
            ProductionDashboardViewModel => new ProductionDashboardView(),

            // Warehouse
            WarehouseDashboardViewModel => new WarehouseDashboardView(),

            // Logistics
            LogisticsDashboardViewModel => new LogisticsDashboardView(),

            // Fallback
            NotFoundViewModel => new NotFoundView(),

            _ => new TextBlock { Text = "View Not Found" }
        };
    }

    public bool Match(object? data) => data is ViewModelBase;
}