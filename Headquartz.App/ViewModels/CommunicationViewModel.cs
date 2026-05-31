using System;
using System.Collections.ObjectModel;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;

namespace Headquartz.App.ViewModels;

public partial class CommunicationViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;
    private readonly PlayerRole _currentRole;

    [ObservableProperty] private string _newMessage = "";
    [ObservableProperty] private long _currentTick;
    [ObservableProperty] private string _worldTime = "";

    public ObservableCollection<MessageModel> Messages { get; } = [];

    // Shared across all instances (simulating broadcast)
    private static readonly ObservableCollection<MessageModel>
        _sharedMessages = [];

    public CommunicationViewModel(
        SimulationService simulation,
        PlayerRole currentRole)
    {
        _simulation = simulation;
        _currentRole = currentRole;

        // Mirror shared messages
        foreach (var m in _sharedMessages)
            Messages.Add(m);

        _simulation.Engine.OnUpdated += RefreshClock;

        // Post system status update every 20 ticks
        _simulation.Engine.OnUpdated += MaybePostSystemUpdate;

        RefreshClock();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void PostMessage()
    {
        string text = NewMessage.Trim();
        if (string.IsNullOrEmpty(text)) return;

        var msg = new MessageModel
        {
            SenderRole = RoleDisplayName(_currentRole),
            SenderEmoji = RoleEmoji(_currentRole),
            Text = text,
            Timestamp = _simulation.Engine.Clock.WorldTime
                              .ToString("MM/dd HH:mm"),
            IsSystem = false,
        };

        _sharedMessages.Add(msg);
        Messages.Add(msg);
        NewMessage = "";
    }

    [RelayCommand]
    private void ClearMessages()
    {
        _sharedMessages.Clear();
        Messages.Clear();
        PostSystemMessage("Communication board cleared.");
    }

    // ── Helpers ───────────────────────────────────────────────

    private void RefreshClock()
    {
        CurrentTick = _simulation.Engine.Clock.Tick;
        WorldTime = _simulation.Engine.Clock.WorldTime.ToString("yyyy-MM-dd HH:mm");
    }

    private long _lastAutoPost;

    private void MaybePostSystemUpdate()
    {
        long tick = _simulation.Engine.Clock.Tick;
        if (tick - _lastAutoPost < 20) return;

        _lastAutoPost = tick;
        var c = _simulation.Engine.Company;

        string status = c.Cash < 0
            ? $"⚠ SYSTEM: Cash is negative (${c.Cash:N0}). Finance attention needed."
            : c.Reputation < 20
                ? $"⚠ SYSTEM: Reputation critical ({c.Reputation}/100). Marketing action required."
                : $"ℹ SYSTEM: Tick {tick} — Cash ${c.Cash:N0} | Rep {c.Reputation}/100 | Staff {c.Employees.Count}";

        Dispatcher.UIThread.Post(() => PostSystemMessage(status));
    }

    private void PostSystemMessage(string text)
    {
        var msg = new MessageModel
        {
            SenderRole = "System",
            SenderEmoji = "🤖",
            Text = text,
            Timestamp = _simulation.Engine.Clock.WorldTime
                              .ToString("MM/dd HH:mm"),
            IsSystem = true,
        };

        _sharedMessages.Add(msg);
        Messages.Add(msg);

        // Cap at 100 messages
        while (_sharedMessages.Count > 100)
        {
            _sharedMessages.RemoveAt(0);
            if (Messages.Count > 0) Messages.RemoveAt(0);
        }
    }

    private static string RoleDisplayName(PlayerRole role) => role switch
    {
        PlayerRole.HumanResourcesManager => "HR Manager",
        PlayerRole.FinanceManager => "Finance Manager",
        PlayerRole.SalesManager => "Sales Manager",
        PlayerRole.MarketingManager => "Marketing Manager",
        PlayerRole.ProductionManager => "Production Manager",
        PlayerRole.WarehouseManager => "Warehouse Manager",
        PlayerRole.LogisticsManager => "Logistics Manager",
        PlayerRole.Chairman => "Board Chairman",
        _ => "Unknown",
    };

    private static string RoleEmoji(PlayerRole role) => role switch
    {
        PlayerRole.HumanResourcesManager => "👥",
        PlayerRole.FinanceManager => "💰",
        PlayerRole.SalesManager => "📈",
        PlayerRole.MarketingManager => "📣",
        PlayerRole.ProductionManager => "🏭",
        PlayerRole.WarehouseManager => "📦",
        PlayerRole.LogisticsManager => "🚚",
        PlayerRole.Chairman => "🏛️",
        _ => "👤",
    };
}