using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Shared.Networking;

using Microsoft.AspNetCore.SignalR.Client;

namespace Headquartz.Networking.Client;

public class GameClient
{
    private HubConnection? _connection;

    public event Action<CompanySnapshot>?
        SnapshotReceived;

    public async Task ConnectAsync(
        string url)
    {
        _connection =
            new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

        _connection.On<CompanySnapshot>(
            "ReceiveSnapshot",
            snapshot =>
            {
                SnapshotReceived?.Invoke(
                    snapshot);
            });

        await _connection.StartAsync();
    }

    public async Task SendCommandAsync(
        CommandPacket packet)
    {
        if (_connection == null)
        {
            return;
        }

        await _connection.InvokeAsync(
            "SendCommand",
            packet);
    }
}
