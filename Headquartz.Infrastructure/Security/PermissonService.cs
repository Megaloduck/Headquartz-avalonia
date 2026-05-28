using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Infrastructure.Security;

public static class PermissionService
{
    public static bool HasPermission(
        PlayerRole role,
        Permission permission)
    {
        if (role == PlayerRole.Chairman)
        {
            return true;
        }

        return role switch
        {
            PlayerRole.HumanResourcesManager =>
                permission is
                    Permission.HireEmployee or
                    Permission.FireEmployee,

            PlayerRole.FinanceManager =>
                permission is
                    Permission.AllocateBudget or
                    Permission.TakeLoan,

            PlayerRole.MarketingManager =>
                permission is
                    Permission.CreateCampaign,

            PlayerRole.SalesManager =>
                permission is
                    Permission.CreateOrder or
                    Permission.ApproveOrder,

            PlayerRole.ProductionManager =>
                permission is
                    Permission.StartProduction,

            PlayerRole.WarehouseManager =>
                permission is
                    Permission.ManageInventory,

            PlayerRole.LogisticsManager =>
                permission is
                    Permission.DispatchShipment,

            _ => false
        };
    }
}