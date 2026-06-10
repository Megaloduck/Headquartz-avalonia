using Headquartz.App.ViewModels;
using Headquartz.Domain.Enums;
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

    public void Navigate(
        string route,
        PlayerRole currentRole = PlayerRole.HumanResourcesManager)
    {
        CurrentView = route switch
        {
            // ── Main menu ─────────────────────────────────────
            "company" => new CompanyViewModel(_simulation),
            "forecast" => new ForecastViewModel(_simulation),
            "settings" => new SettingsViewModel(_simulation),
            "communication" => new CommunicationViewModel(_simulation, currentRole),

            // ── Human Resources ───────────────────────────────
            "hr/dashboard" => new HRDashboardViewModel(_simulation),
            "hr/employees" => new HREmployeeManagementViewModel(_simulation),
            "hr/recruitment" => new HRRecruitmentViewModel(_simulation),
            "hr/payroll" => new HRPayrollViewModel(_simulation),
            "hr/development" => new HRDevelopmentViewModel(_simulation),
            "hr/performance" => new HRPerformanceViewModel(_simulation),
            "hr/reports" => new DepartmentReportViewModel(_simulation, DepartmentType.HumanResources),

            // ── Finance ───────────────────────────────────────
            "finance/dashboard" => new FinanceDashboardViewModel(_simulation),
            "finance/budget" => new FinanceBudgetAllocationViewModel(_simulation),
            "finance/loans" => new FinanceLoansViewModel(_simulation),
            "finance/receivable" => new FinanceAccountReceivableViewModel(_simulation),
            "finance/payable" => new FinanceAccountPayableViewModel(_simulation),
            "finance/audits" => new FinanceAuditsViewModel(_simulation),
            "finance/reports" => new DepartmentReportViewModel(_simulation, DepartmentType.Finance),    

            // ── Sales ─────────────────────────────────────────
            "sales/dashboard" => new SalesDashboardViewModel(_simulation),
            "sales/orders" => new SalesOrdersViewModel(_simulation),
            "sales/leads" => new SalesLeadsViewModel(_simulation),
            "sales/clients" => new SalesClientsViewModel(_simulation),
            "sales/pipeline" => new SalesPipelineViewModel(_simulation),
            "sales/incentives" => new SalesIncentivesViewModel(_simulation),
            "sales/reports" => new DepartmentReportViewModel(_simulation, DepartmentType.Sales),

            // ── Marketing ─────────────────────────────────────
            "marketing/dashboard" => new MarketingDashboardViewModel(_simulation),
            "marketing/campaigns" => new MarketingCampaignsViewModel(_simulation),
            "marketing/research" => new MarketingResearchViewModel(_simulation),
            "marketing/pricing" => new MarketingPricingViewModel(_simulation),
            "marketing/product" => new MarketingProductViewModel(_simulation),
            "marketing/branding" => new MarketingBrandingViewModel(_simulation),
            "marketing/reports" => new DepartmentReportViewModel(_simulation, DepartmentType.Marketing),

            // ── Production ────────────────────────────────────
            "production/dashboard" => new ProductionDashboardViewModel(_simulation),
            "production/workorders" => new ProductionWorkOrdersViewModel(_simulation),
            "production/maintenance" => new ProductionMaintenanceViewModel(_simulation),
            "production/lines" => new ProductionLinesViewModel(_simulation),
            "production/resources" => new ProductionResourcesViewModel(_simulation),
            "production/quality" => new ProductionQualityViewModel(_simulation),
            "production/reports" => new DepartmentReportViewModel(_simulation, DepartmentType.Production),

            // ── Warehouse ─────────────────────────────────────
            "warehouse/dashboard" => new WarehouseDashboardViewModel(_simulation),
            "warehouse/inventory" => new WarehouseInventoryViewModel(_simulation),
            "warehouse/reports" => new DepartmentReportViewModel(_simulation, DepartmentType.Warehouse),

            // ── Logistics ─────────────────────────────────────
            "logistics/dashboard" => new LogisticsDashboardViewModel(_simulation),
            "logistics/shipments" => new LogisticsShipmentsViewModel(_simulation),
            "logistics/reports" => new DepartmentReportViewModel(_simulation, DepartmentType.Logistics),

            // ── Fallback ──────────────────────────────────────
            _ => new NotFoundViewModel(),
        };

        OnViewChanged?.Invoke();
    }
}