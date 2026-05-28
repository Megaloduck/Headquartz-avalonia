using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Domain.Enums;

public enum Permission
{
    HireEmployee,
    FireEmployee,

    AllocateBudget,
    TakeLoan,

    CreateCampaign,

    CreateOrder,
    ApproveOrder,

    StartProduction,

    ManageInventory,

    DispatchShipment,

    ViewReports,

    FullAccess
}
