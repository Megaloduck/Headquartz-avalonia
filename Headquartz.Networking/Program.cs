using Headquartz.Networking.Server;

var builder =
    WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app =
    builder.Build();

app.MapHub<GameHub>(
    "/gamehub");

Console.WriteLine(
    "Headquartz server running...");

app.Run();