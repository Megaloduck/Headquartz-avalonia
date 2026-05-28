using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.App.Models;

using Headquartz.Domain.Enums;

namespace Headquartz.App.Services;

public static class SidebarService
{
    public static List<SidebarItem>
        GetSidebar(
            PlayerRole role)
    {
        var items =
            new List<SidebarItem>();

        // =====================================================
        // COMMON
        // =====================================================

        items.Add(
            new SidebarItem
            {
                Title = "Company",
                Icon = "Building",
                Route = "company"
            });

        items.Add(
            new SidebarItem
            {
                Title = "Forecast",
                Icon = "ChartLine",
                Route = "forecast"
            });

        // =====================================================
        // HR
        // =====================================================

        if (role ==
            PlayerRole.HumanResourcesManager)
        {
            items.AddRange(
            [
                new()
                {
                    Title =
                        "HR Dashboard",

                    Icon = "Users",

                    Route =
                        "hr/dashboard"
                },

                new()
                {
                    Title =
                        "Employee Management",

                    Icon = "UserCog",

                    Route =
                        "hr/employees"
                },

                new()
                {
                    Title =
                        "Recruitment",

                    Icon = "UserPlus",

                    Route =
                        "hr/recruitment"
                },

                new()
                {
                    Title =
                        "Payroll",

                    Icon = "Wallet",

                    Route =
                        "hr/payroll"
                },

                new()
                {
                    Title =
                        "Development",

                    Icon = "GraduationCap",

                    Route =
                        "hr/development"
                },

                new()
                {
                    Title =
                        "Performance Evals",

                    Icon = "ClipboardList",

                    Route =
                        "hr/performance"
                },

                new()
                {
                    Title =
                        "HR Reports",

                    Icon = "FileBarChart",

                    Route =
                        "hr/reports"
                },
            ]);
        }

        // =====================================================
        // FINANCE
        // =====================================================

        if (role ==
            PlayerRole.FinanceManager)
        {
            items.AddRange(
            [
                new()
                {
                    Title =
                        "Finance Dashboard",

                    Icon = "Wallet",

                    Route =
                        "finance/dashboard"
                },

                new()
                {
                    Title =
                        "Account Payable",

                    Icon = "CreditCard",

                    Route =
                        "finance/payable"
                },

                new()
                {
                    Title =
                        "Account Receivable",

                    Icon = "Receipt",

                    Route =
                        "finance/receivable"
                },

                new()
                {
                    Title =
                        "Loans",

                    Icon = "Banknote",

                    Route =
                        "finance/loans"
                },

                new()
                {
                    Title =
                        "Budget Allocation",

                    Icon = "PieChart",

                    Route =
                        "finance/budget"
                },

                new()
                {
                    Title =
                        "Audits",

                    Icon = "Search",

                    Route =
                        "finance/audits"
                },

                new()
                {
                    Title =
                        "Finance Reports",

                    Icon = "FileBarChart",

                    Route =
                        "finance/reports"
                },
            ]);
        }

        // =====================================================
        // SALES
        // =====================================================

        if (role ==
            PlayerRole.SalesManager)
        {
            items.AddRange(
            [
                new()
                {
                    Title =
                        "Sales Dashboard",

                    Icon = "BarChart3",

                    Route =
                        "sales/dashboard"
                },

                new()
                {
                    Title =
                        "Client Management",

                    Icon = "Users",

                    Route =
                        "sales/clients"
                },

                new()
                {
                    Title =
                        "Leads",

                    Icon = "Target",

                    Route =
                        "sales/leads"
                },

                new()
                {
                    Title =
                        "Orders",

                    Icon = "ShoppingCart",

                    Route =
                        "sales/orders"
                },

                new()
                {
                    Title =
                        "Pipeline Management",

                    Icon = "GitBranch",

                    Route =
                        "sales/pipeline"
                },

                new()
                {
                    Title =
                        "Performance Incentives",

                    Icon = "Award",

                    Route =
                        "sales/incentives"
                },

                new()
                {
                    Title =
                        "Sales Reports",

                    Icon = "FileBarChart",

                    Route =
                        "sales/reports"
                },
            ]);
        }

        // =====================================================
        // MARKETING
        // =====================================================

        if (role ==
            PlayerRole.MarketingManager)
        {
            items.AddRange(
            [
                new()
                {
                    Title =
                        "Marketing Dashboard",

                    Icon = "Megaphone",

                    Route =
                        "marketing/dashboard"
                },

                new()
                {
                    Title =
                        "Campaign",

                    Icon = "Send",

                    Route =
                        "marketing/campaigns"
                },

                new()
                {
                    Title =
                        "Market Research",

                    Icon = "Search",

                    Route =
                        "marketing/research"
                },

                new()
                {
                    Title =
                        "Pricing Strategy",

                    Icon = "BadgeDollarSign",

                    Route =
                        "marketing/pricing"
                },

                new()
                {
                    Title =
                        "Product Research",

                    Icon = "FlaskConical",

                    Route =
                        "marketing/product"
                },

                new()
                {
                    Title =
                        "Branding",

                    Icon = "Palette",

                    Route =
                        "marketing/branding"
                },

                new()
                {
                    Title =
                        "Marketing Reports",

                    Icon = "FileBarChart",

                    Route =
                        "marketing/reports"
                },
            ]);
        }

        return items;
    }
}