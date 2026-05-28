using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.App.Models;
using Headquartz.Domain.Enums;

namespace Headquartz.App.Services;

public static class SidebarService
{
    // =========================================================
    // MAIN ENTRY POINT
    // =========================================================

    public static List<SidebarSection> GetSections(PlayerRole role)
    {
        return
        [
            BuildMainMenuSection(),
            BuildManagementSection(role),
            BuildPersonalizationSection(),
        ];
    }

    // =========================================================
    // COMMON SECTIONS
    // =========================================================

    private static SidebarSection BuildMainMenuSection()
    {
        return new SidebarSection
        {
            Title = "Main menu",
            Items =
            [
                new() { Title = "Company",   Icon = "Building",   Route = "company"   },
                new() { Title = "Forecast", Icon = "ChartLine",  Route = "forecast"  },
            ]
        };
    }

    private static SidebarSection BuildPersonalizationSection()
    {
        return new SidebarSection
        {
            Title = "Personalization",
            Items =
            [
                new() { Title = "Communication", Icon = "MessageCircle", Route = "communication" },
                new() { Title = "My Profile",    Icon = "Home",          Route = "profile"       },
            ]
        };
    }

    // =========================================================
    // ROLE-SPECIFIC MANAGEMENT SECTIONS
    // =========================================================

    private static SidebarSection BuildManagementSection(PlayerRole role)
    {
        return new SidebarSection
        {
            Title = "Managements",
            Items = role switch
            {
                PlayerRole.HumanResourcesManager => HRItems(),
                PlayerRole.FinanceManager => FinanceItems(),
                PlayerRole.SalesManager => SalesItems(),
                PlayerRole.MarketingManager => MarketingItems(),
                PlayerRole.ProductionManager => ProductionItems(),
                PlayerRole.WarehouseManager => WarehouseItems(),
                PlayerRole.LogisticsManager => LogisticsItems(),
                PlayerRole.Chairman => ChairmanItems(),
                _ => [],
            }
        };
    }

    // =========================================================
    // HR
    // =========================================================

    private static List<SidebarItem> HRItems() =>
    [
        new() { Title = "HR Dashboard",        Icon = "LayoutDashboard", Route = "hr/dashboard"    },
        new() { Title = "Employee Management", Icon = "Users",           Route = "hr/employees"    },
        new() { Title = "Recruitment",         Icon = "UserPlus",        Route = "hr/recruitment"  },
        new() { Title = "Payroll",             Icon = "Wallet",          Route = "hr/payroll", },
        new() { Title = "Development",         Icon = "GraduationCap",   Route = "hr/development"  },
        new() { Title = "Performance Evals",   Icon = "ClipboardList",   Route = "hr/performance"  },
        new() { Title = "HR Reports",          Icon = "BarChart2",       Route = "hr/reports"      },
    ];

    // =========================================================
    // FINANCE
    // =========================================================

    private static List<SidebarItem> FinanceItems() =>
    [
        new() { Title = "Finance Dashboard",    Icon = "LayoutDashboard", Route = "finance/dashboard"   },
        new() { Title = "Account Payable",      Icon = "CreditCard",      Route = "finance/payable"     },
        new() { Title = "Account Receivable",   Icon = "Receipt",         Route = "finance/receivable"  },
        new() { Title = "Loans",                Icon = "Banknote",        Route = "finance/loans" },
        new() { Title = "Budget Allocation",    Icon = "PieChart",        Route = "finance/budget"      },
        new() { Title = "Audits",               Icon = "Search",          Route = "finance/audits"      },
        new() { Title = "Finance Reports",      Icon = "BarChart2",       Route = "finance/reports"     },
    ];

    // =========================================================
    // SALES
    // =========================================================

    private static List<SidebarItem> SalesItems() =>
    [
        new() { Title = "Sales Dashboard",       Icon = "LayoutDashboard", Route = "sales/dashboard"   },
        new() { Title = "Client Management",     Icon = "Users",           Route = "sales/clients"     },
        new() { Title = "Leads",                 Icon = "Target",          Route = "sales/leads"       },
        new() { Title = "Orders",                Icon = "ShoppingCart",    Route = "sales/orders",},
        new() { Title = "Pipeline Management",   Icon = "GitBranch",       Route = "sales/pipeline"    },
        new() { Title = "Performance Incentives",Icon = "Award",           Route = "sales/incentives"  },
        new() { Title = "Sales Reports",         Icon = "BarChart2",       Route = "sales/reports"     },
    ];

    // =========================================================
    // MARKETING
    // =========================================================

    private static List<SidebarItem> MarketingItems() =>
    [
        new() { Title = "Marketing Dashboard", Icon = "LayoutDashboard",  Route = "marketing/dashboard" },
        new() { Title = "Campaign",            Icon = "Megaphone",        Route = "marketing/campaigns" },
        new() { Title = "Market Research",     Icon = "Search",           Route = "marketing/research"  },
        new() { Title = "Pricing Strategy",    Icon = "BadgeDollarSign",  Route = "marketing/pricing" },
        new() { Title = "Product Research",    Icon = "FlaskConical",     Route = "marketing/product"   },
        new() { Title = "Branding",            Icon = "Palette",          Route = "marketing/branding"  },
        new() { Title = "Marketing Reports",   Icon = "BarChart2",        Route = "marketing/reports"   },
    ];

    // =========================================================
    // PRODUCTION
    // =========================================================

    private static List<SidebarItem> ProductionItems() =>
    [
        new() { Title = "Production Dashboard", Icon = "LayoutDashboard", Route = "production/dashboard"  },
        new() { Title = "Work Order",           Icon = "ClipboardList",   Route = "production/workorders" },
        new() { Title = "Line Management",      Icon = "Layers",          Route = "production/lines"      },
        new() { Title = "Resource Planner",     Icon = "Calendar",        Route = "production/resources", },
        new() { Title = "Maintenance",          Icon = "Wrench",          Route = "production/maintenance" },
        new() { Title = "Quality Control",      Icon = "ShieldCheck",     Route = "production/quality"    },
        new() { Title = "Production Reports",   Icon = "BarChart2",       Route = "production/reports"    },
    ];

    // =========================================================
    // WAREHOUSE
    // =========================================================

    private static List<SidebarItem> WarehouseItems() =>
    [
        new() { Title = "Warehouse Dashboard",  Icon = "LayoutDashboard", Route = "warehouse/dashboard"  },
        new() { Title = "Inventory Management", Icon = "Package",         Route = "warehouse/inventory"  },
        new() { Title = "Stock In",             Icon = "PackagePlus",     Route = "warehouse/stockin"    },
        new() { Title = "Stock Out",            Icon = "PackageMinus",    Route = "warehouse/stockout" },
        new() { Title = "Flow Management",      Icon = "ArrowLeftRight",  Route = "warehouse/flow"       },
        new() { Title = "Storage Allocation",   Icon = "Map",             Route = "warehouse/storage"    },
        new() { Title = "Warehouse Reports",    Icon = "BarChart2",       Route = "warehouse/reports"    },
    ];

    // =========================================================
    // LOGISTICS
    // =========================================================

    private static List<SidebarItem> LogisticsItems() =>
    [
        new() { Title = "Logistics Dashboard", Icon = "LayoutDashboard", Route = "logistics/dashboard"  },
        new() { Title = "Shipments",           Icon = "Truck",           Route = "logistics/shipments"  },
        new() { Title = "Delivery Tracking",   Icon = "MapPin",          Route = "logistics/tracking"   },
        new() { Title = "Route Planner",       Icon = "Navigation",      Route = "logistics/routes" },
        new() { Title = "SLA Management",      Icon = "FileCheck",       Route = "logistics/sla"        },
        new() { Title = "Fleet Management",    Icon = "Car",             Route = "logistics/fleet"      },
        new() { Title = "Logistics Reports",   Icon = "BarChart2",       Route = "logistics/reports"    },
    ];

    // =========================================================
    // CHAIRMAN
    // =========================================================

    private static List<SidebarItem> ChairmanItems() =>
    [
        new() { Title = "HR Reports",          Icon = "BarChart2",       Route = "hr/reports"           },
        new() { Title = "Finance Reports",     Icon = "BarChart2",       Route = "finance/reports"      },
        new() { Title = "Sales Reports",       Icon = "BarChart2",       Route = "sales/reports"        },
        new() { Title = "Marketing Reports",   Icon = "BarChart2",       Route = "marketing/reports"    },
        new() { Title = "Production Reports",  Icon = "BarChart2",       Route = "production/reports"   },
        new() { Title = "Warehouse Reports",   Icon = "BarChart2",       Route = "warehouse/reports"    },
        new() { Title = "Logistics Reports",   Icon = "BarChart2",       Route = "logistics/reports"    },
    ];
}