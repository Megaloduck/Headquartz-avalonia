using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Headquartz.Networking.Server;

public class GameServer
{
    public async Task StartAsync()
    {
        var builder =
            WebApplication.CreateBuilder();

        builder.Services.AddSignalR();

        var app =
            builder.Build();

        app.MapHub<GameHub>(
            "/gamehub");

        await app.StartAsync();
    }
}
