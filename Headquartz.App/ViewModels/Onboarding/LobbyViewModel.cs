using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models.Onboarding;
using Headquartz.App.Services;
using Headquartz.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Headquartz.App.ViewModels;

public partial class LobbyViewModel : ViewModelBase
{
    private readonly OnboardingFlowService _flow;
    private readonly DispatcherTimer _pingTimer;

    // ── State ─────────────────────────────────────────────────

    [ObservableProperty] private string _roomCode = "";
    [ObservableProperty] private bool _isHost;
    [ObservableProperty] private bool _isReady;
    [ObservableProperty] private bool _canStart;
    [ObservableProperty] private string _statusMessage = "Waiting for players...";
    [ObservableProperty] private int _localPing = 0;

    // ── Flow gating ──────────────────────────────────────────
    //
    // The lobby now gates progression on two things before anyone can
    // ready up: the host must have configured the company, and the local
    // player must have picked a department. Joiners skip the company step
    // entirely (only the host configures it) and go straight to picking
    // a department.

    [ObservableProperty] private bool _isCompanyConfigured;
    [ObservableProperty] private bool _hasLocalDepartment;
    [ObservableProperty] private bool _showConfigureCompanyButton;
    [ObservableProperty] private bool _showPickDepartmentButton;
    [ObservableProperty] private string _companyConfigSummary = "";

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<LobbyPlayerRowModel> Players { get; } = [];

    // ── Slot labels (always show 8 slots) ────────────────────

    public ObservableCollection<LobbySlotModel> Slots { get; } = [];

    private static readonly (string Role, string Emoji, string Color)[] RoleSlots =
    [
        ("Human Resources", "👥", "#8B5CF6"),
        ("Finance",         "💰", "#10B981"),
        ("Sales",           "📈", "#3B82F6"),
        ("Marketing",       "📣", "#F59E0B"),
        ("Production",      "🏭", "#EF4444"),
        ("Warehouse",       "📦", "#F97316"),
        ("Logistics",       "🚚", "#06B6D4"),
        ("Board Chairman",      "🏛️", "#EAB308"),
  ];

    public LobbyViewModel(OnboardingFlowService flow)
    {
        _flow = flow;

        RoomCode = flow.SessionConfig?.RoomCode ?? "";
        IsHost = flow.SessionConfig?.Players.Any(p => p.IsLocalPlayer && p.IsHost) ?? false;

        BuildSlots();
        SyncPlayers();
        RefreshFlowState();

        // Simulate ping updates
        _pingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _pingTimer.Tick += (_, _) => SimulatePingUpdate();
        _pingTimer.Start();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void CopyRoomCode()
    {
        // Clipboard copy — wired in code-behind
    }

    /// <summary>Host-only: leaves the lobby to configure company/industry/difficulty.</summary>
    [RelayCommand]
    private void ConfigureCompany()
    {
        _flow.ProceedToCompanySetup();
    }

    /// <summary>
    /// Leaves the lobby to pick a department. Used by both the host
    /// (after configuring the company) and joining players (immediately).
    /// </summary>
    [RelayCommand]
    private void PickDepartment()
    {
        _flow.ProceedToDepartmentSelection();
    }

    [RelayCommand]
    private void ToggleReady()
    {
        // Can't ready up without a department assigned yet.
        if (!HasLocalDepartment) return;

        IsReady = !IsReady;

        var local = _flow.SessionConfig?.Players
            .FirstOrDefault(p => p.IsLocalPlayer);

        if (local != null)
        {
            local.Status = IsReady
                ? LobbyPlayerStatus.Ready
                : LobbyPlayerStatus.Connected;
        }

        SyncPlayers();
        EvaluateCanStart();

        StatusMessage = IsReady
            ? "You are ready. Waiting for others..."
            : "Waiting for players...";
    }

    [RelayCommand]
    private void StartGame()
    {
        if (!CanStart) return;

        _pingTimer.Stop();
        _flow.LaunchGame();
    }

    [RelayCommand]
    private void LeaveRoom()
    {
        _pingTimer.Stop();
        _flow.NavigateTo(OnboardingScreen.MainMenu);
    }

    // ── Internal ──────────────────────────────────────────────

    private void BuildSlots()
    {
        Slots.Clear();
        for (int i = 0; i < RoleSlots.Length; i++)
        {
            var (role, emoji, color) = RoleSlots[i];
            Slots.Add(new LobbySlotModel
            {
                SlotIndex = i,
                RoleName = role,
                RoleEmoji = emoji,
                AccentColor = color,
                IsOccupied = false,
            });
        }
    }

    private void SyncPlayers()
    {
        Players.Clear();

        var sessionPlayers = _flow.SessionConfig?.Players ?? [];

        foreach (var p in sessionPlayers)
        {
            Players.Add(new LobbyPlayerRowModel
            {
                Username = p.Username,
                AvatarEmoji = p.AvatarEmoji,
                Level = p.Level,
                IsHost = p.IsHost,
                IsLocalPlayer = p.IsLocalPlayer,
                Status = p.Status,
                StatusLabel = p.Status switch
                {
                    LobbyPlayerStatus.Ready => "✓ Ready",
                    LobbyPlayerStatus.Connected => "Waiting",
                    LobbyPlayerStatus.Connecting => "Connecting...",
                    _ => "Disconnected",
                },
                StatusColor = p.Status switch
                {
                    LobbyPlayerStatus.Ready => "#10B981",
                    LobbyPlayerStatus.Connected => "#6B7280",
                    LobbyPlayerStatus.Connecting => "#F59E0B",
                    _ => "#EF4444",
                },
                PingMs = p.PingMs,
            });
        }

        // Fill remaining slots as empty
        int emptySlots = Math.Max(0, 2 - Players.Count);
        for (int i = 0; i < emptySlots; i++)
        {
            Players.Add(new LobbyPlayerRowModel { IsEmpty = true });
        }
    }

    /// <summary>
    /// Recomputes the company-configured / department-picked gating state.
    /// Called on construction; since a fresh LobbyViewModel is created each
    /// time the onboarding shell routes back to the Lobby screen, this is
    /// sufficient to reflect state after a detour to CompanySetup or
    /// DepartmentSelection.
    /// </summary>
    private void RefreshFlowState()
    {
        IsCompanyConfigured = _flow.IsCompanyConfigured;
        HasLocalDepartment = _flow.LocalPlayerHasDepartment;

        // Host must configure the company before picking a department.
        // Joiners skip straight to department picking.
        ShowConfigureCompanyButton = IsHost && !IsCompanyConfigured;
        ShowPickDepartmentButton = (!IsHost || IsCompanyConfigured) && !HasLocalDepartment;

        CompanyConfigSummary = IsCompanyConfigured
            ? $"{_flow.SessionConfig?.CompanyName} · {_flow.SessionConfig?.Industry} · {_flow.SessionConfig?.Difficulty} Difficulty"
            : IsHost
                ? "Configure your company to continue."
                : "Waiting for the host to configure the company...";
    }

    private void EvaluateCanStart()
    {
        if (!IsHost) return;

        var players = _flow.SessionConfig?.Players ?? [];
        CanStart = IsCompanyConfigured
                   && HasLocalDepartment
                   && players.Count >= 1
                   && players.All(p => p.Status == LobbyPlayerStatus.Ready);
    }

    private void SimulatePingUpdate()
    {
        LocalPing = Random.Shared.Next(8, 45);

        var local = _flow.SessionConfig?.Players
            .FirstOrDefault(p => p.IsLocalPlayer);

        if (local != null)
            local.PingMs = LocalPing;

        SyncPlayers();
    }
}

// ── Row models ────────────────────────────────────────────────────────────────

public class LobbyPlayerRowModel
{
    public string Username { get; set; } = "";
    public string AvatarEmoji { get; set; } = "";
    public int Level { get; set; }
    public bool IsHost { get; set; }
    public bool IsLocalPlayer { get; set; }
    public LobbyPlayerStatus Status { get; set; }
    public string StatusLabel { get; set; } = "";
    public string StatusColor { get; set; } = "#6B7280";
    public int PingMs { get; set; }
    public bool IsEmpty { get; set; }
}

public class LobbySlotModel
{
    public int SlotIndex { get; set; }
    public string RoleName { get; set; } = "";
    public string RoleEmoji { get; set; } = "";
    public string AccentColor { get; set; } = "";
    public bool IsOccupied { get; set; }
    public string? OccupiedBy { get; set; }
}