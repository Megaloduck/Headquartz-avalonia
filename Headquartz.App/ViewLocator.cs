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

            HRDashboardViewModel => new HRDashboardView(),
            HREmployeeManagementViewModel => new HREmployeeManagementView(),
            HRRecruitmentViewModel => new HRRecruitmentView(),
            HRPayrollViewModel => new HRPayrollView(),

            FinanceDashboardViewModel => new FinanceDashboardView(),
            FinanceBudgetAllocationViewModel => new FinanceBudgetAllocationView(),

            SalesDashboardViewModel => new SalesDashboardView(),
            MarketingDashboardViewModel => new MarketingDashboardView(),
            ProductionDashboardViewModel => new ProductionDashboardView(),
            WarehouseDashboardViewModel => new WarehouseDashboardView(),
            LogisticsDashboardViewModel => new LogisticsDashboardView(),

            NotFoundViewModel => new NotFoundView(),

            _ => new TextBlock { Text = "View Not Found" }
        };
    }

    public bool Match(object? data) => data is ViewModelBase;
}