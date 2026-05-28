using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Domain.Enums;

public enum OrderStatus
{
    Pending,
    Approved,
    InProduction,
    ReadyForShipment,
    Shipping,
    Delivered,
    Cancelled
}
