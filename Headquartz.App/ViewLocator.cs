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
            HRDevelopmentViewModel => new HRDevelopmentView(),
            HRPerformanceViewModel => new HRPerformanceView(),

            // ── Finance ───────────────────────────────────────
            FinanceDashboardViewModel => new FinanceDashboardView(),
            FinanceBudgetAllocationViewModel => new FinanceBudgetAllocationView(),
            FinanceLoansViewModel => new FinanceLoansView(),
            FinanceAccountReceivableViewModel => new FinanceAccountReceivableView(),
            FinanceAccountPayableViewModel => new FinanceAccountPayableView(),
            FinanceAuditsViewModel => new FinanceAuditsView(),

            // ── Sales ─────────────────────────────────────────
            SalesDashboardViewModel => new SalesDashboardView(),
            SalesOrdersViewModel => new SalesOrdersView(),
            SalesLeadsViewModel => new SalesLeadsView(),
            SalesClientsViewModel => new SalesClientsView(),
            SalesPipelineViewModel => new SalesPipelineView(),
            SalesIncentivesViewModel => new SalesIncentivesView(),

            // ── Marketing ─────────────────────────────────────
            MarketingDashboardViewModel => new MarketingDashboardView(),
            MarketingCampaignsViewModel => new MarketingCampaignsView(),
            MarketingResearchViewModel => new MarketingResearchView(),
            MarketingPricingViewModel => new MarketingPricingView(),
            MarketingProductViewModel => new MarketingProductView(),
            MarketingBrandingViewModel => new MarketingBrandingView(),  

            // ── Production ────────────────────────────────────
            ProductionDashboardViewModel => new ProductionDashboardView(),
            ProductionWorkOrdersViewModel => new ProductionWorkOrdersView(),
            ProductionMaintenanceViewModel => new ProductionMaintenanceView(),
            ProductionLinesViewModel => new ProductionLinesView(),
            ProductionResourcesViewModel => new ProductionResourcesView(),
            ProductionQualityViewModel => new ProductionQualityView(),

            // ── Warehouse ─────────────────────────────────────
            WarehouseDashboardViewModel => new WarehouseDashboardView(),
            WarehouseInventoryViewModel => new WarehouseInventoryView(),
            WarehouseStockInViewModel => new WarehouseStockInView(),
            WarehouseStockOutViewModel => new WarehouseStockOutView(),
            WarehouseFlowViewModel => new WarehouseFlowView(),
            WarehouseStorageViewModel => new WarehouseStorageView(),

            // ── Logistics ─────────────────────────────────────
            LogisticsDashboardViewModel => new LogisticsDashboardView(),
            LogisticsShipmentsViewModel => new LogisticsShipmentsView(),
            LogisticsTrackingViewModel => new LogisticsTrackingView(),
            LogisticsRoutesViewModel => new LogisticsRoutesView(),
            LogisticsSLAViewModel => new LogisticsSLAView(),
            LogisticsFleetViewModel => new LogisticsFleetView(),

            // ── Fallback ──────────────────────────────────────
            NotFoundViewModel => new NotFoundView(),
            _ => new TextBlock { Text = $"View not found for {data?.GetType().Name}" }
        };
    }

    public bool Match(object? data) => data is ViewModelBase;
}