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
            // ── Root shells ───────────────────────────────────
            ShellViewModel => new ShellView(),
            OnboardingShellViewModel => new OnboardingShellView(),

            // ── Onboarding screens ────────────────────────────
            SplashViewModel => new SplashView(),
            ProfileCreationViewModel => new ProfileCreationView(),
            MainMenuViewModel => new MainMenuView(),
            LobbyViewModel => new LobbyView(),
            CompanySetupViewModel => new CompanySetupView(),
            DepartmentSelectionViewModel => new DepartmentSelectionView(),

            // ── Main menu ─────────────────────────────────────
            CompanyViewModel => new CompanyView(),
            ForecastViewModel => new ForecastView(),
            SettingsViewModel => new SettingsView(),
            CommunicationViewModel => new CommunicationView(),

            // ── Generic report (all 7 depts) ─────────────────
            DepartmentReportViewModel => new DepartmentReportView(),

            // ── Human Resources ───────────────────────────────
            HRDashboardViewModel => new HRDashboardView(),
            HREmployeeManagementViewModel => new HREmployeeManagementView(),
            HRRecruitmentViewModel => new HRRecruitmentView(),
            HRPayrollViewModel => new HRPayrollView(),

            // ── Finance ───────────────────────────────────────
            FinanceDashboardViewModel => new FinanceDashboardView(),
            FinanceBudgetAllocationViewModel => new FinanceBudgetAllocationView(),
            FinanceLoansViewModel => new FinanceLoansView(),

            // ── Sales ─────────────────────────────────────────
            SalesDashboardViewModel => new SalesDashboardView(),
            SalesOrdersViewModel => new SalesOrdersView(),
            SalesLeadsViewModel => new SalesLeadsView(),

            // ── Marketing ─────────────────────────────────────
            MarketingDashboardViewModel => new MarketingDashboardView(),

            // ── Production ────────────────────────────────────
            ProductionDashboardViewModel => new ProductionDashboardView(),
            ProductionWorkOrdersViewModel => new ProductionWorkOrdersView(),
            ProductionMaintenanceViewModel => new ProductionMaintenanceView(),

            // ── Warehouse ─────────────────────────────────────
            WarehouseDashboardViewModel => new WarehouseDashboardView(),
            WarehouseInventoryViewModel => new WarehouseInventoryView(),

            // ── Logistics ─────────────────────────────────────
            LogisticsDashboardViewModel => new LogisticsDashboardView(),
            LogisticsShipmentsViewModel => new LogisticsShipmentsView(),

            // ── Fallback ──────────────────────────────────────
            NotFoundViewModel => new NotFoundView(),
            _ => new TextBlock { Text = $"View not found for {data?.GetType().Name}" }
        };
    }

    public bool Match(object? data) => data is ViewModelBase;
}