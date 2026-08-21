using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Infrastructure.Security;

public static class PermissionService
{
    public static bool HasPermission(
        DepartmentType department,
        Permission permission)
    {
        if (department == DepartmentType.Management)
        {
            return true;
        }

        return department switch
        {
            DepartmentType.HumanResources =>
                permission is
                    Permission.HireEmployee or
                    Permission.FireEmployee,

            DepartmentType.Finance =>
                permission is
                    Permission.AllocateBudget or
                    Permission.TakeLoan,

            DepartmentType.Marketing =>
                permission is
                    Permission.CreateCampaign,

            DepartmentType.Sales =>
                permission is
                    Permission.CreateOrder or
                    Permission.ApproveOrder,

            DepartmentType.Production =>
                permission is
                    Permission.StartProduction,

            DepartmentType.Warehouse =>
                permission is
                    Permission.ManageInventory,

            DepartmentType.Logistics =>
                permission is
                    Permission.DispatchShipment,

            _ => false
        };
    }
}