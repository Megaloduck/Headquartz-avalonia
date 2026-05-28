using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Shared.Networking;

using Microsoft.AspNetCore.SignalR;

namespace Headquartz.Networking.Server;

public class GameHub : Hub
{
    public async Task SendCommand(
        CommandPacket packet)
    {
        await Clients.Others.SendAsync(
            "ReceiveCommand",
            packet);
    }

    public async Task SendSnapshot(
        CompanySnapshot snapshot)
    {
        await Clients.All.SendAsync(
            "ReceiveSnapshot",
            snapshot);
    }
}