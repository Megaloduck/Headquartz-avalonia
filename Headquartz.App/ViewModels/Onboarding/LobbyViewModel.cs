using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Converters;
using Headquartz.App.Models.Onboarding;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class LobbyViewModel : ViewModelBase
{
    private readonly OnboardingFlowService _flow;
    private readonly DispatcherTimer _pingTimer;

    // ── Room / session state ─────────────────────────────────

    [ObservableProperty] private string _roomCode = "";
    [ObservableProperty] private bool _isHost;
    [ObservableProperty] private bool _isMultiplayer;
    [ObservableProperty] private bool _isReady;
    [ObservableProperty] private bool _canStart;
    [ObservableProperty] private string _statusMessage = "Waiting for players...";
    [ObservableProperty] private int _localPing;
    [ObservableProperty] private bool _isDarkTheme = true;

    // ── Company gating ────────────────────────────────────────

    [ObservableProperty] private bool _isCompanyConfigured;
    [ObservableProperty] private bool _showConfigureCompanyButton;
    [ObservableProperty] private bool _showWaitingForCompany;
    [ObservableProperty] private string _companyConfigSummary = "";
    [ObservableProperty] private string _companyName = "";
    [ObservableProperty] private string _industryLabel = "";
    [ObservableProperty] private string _difficultyLabel = "";
    [ObservableProperty] private string _capitalDisplay = "";

    // ── Local player ID card ──────────────────────────────────

    [ObservableProperty] private string _playerName = "";
    [ObservableProperty] private string _playerAvatar = "👤";
    [ObservableProperty] private int _playerLevel = 1;
    [ObservableProperty] private string _playerDepartmentName = "Unassigned";
    [ObservableProperty] private bool _hasLocalDepartment;
    [ObservableProperty] private string _printButtonLabel = "Print My ID Card";

    // ── Department grid + preview ────────────────────────────

    [ObservableProperty] private DepartmentSelectionCard? _previewCard;
    [ObservableProperty] private DepartmentSelectionCard? _selectedCard;
    [ObservableProperty] private int _departmentsCovered;
    [ObservableProperty] private int _departmentsTotal = 7;
    [ObservableProperty] private double _coverageRatio;

    public ObservableCollection<DepartmentSelectionCard> Cards { get; } = [];
    public ObservableCollection<LobbyPlayerRowModel> Players { get; } = [];

    // NOTE: "Manager" is intentionally omitted from every Title here — it's
    // reserved for GameDifficulty.Manager. Management is special-cased as
    // "Board Chairman" and excluded from the 7-department coverage count.
    private static readonly (DepartmentType Department, string Title, string DeptLabel,
        string Emoji, string Color, string Responsibilities)[] DepartmentData =
    [
        (DepartmentType.HumanResources, "Human Resources", "Human Resources", "👥", "#8B5CF6",
            "Workforce · Hiring · Morale · Training · Payroll · Resignations"),
        (DepartmentType.Finance, "Finance", "Finance", "💰", "#10B981",
            "Budgets · Cash Flow · Loans · Payroll Risk · Audits"),
        (DepartmentType.Sales, "Sales", "Sales", "📈", "#3B82F6",
            "Revenue · Orders · Clients · Pipeline · Deadlines"),
        (DepartmentType.Marketing, "Marketing", "Marketing", "📣", "#F59E0B",
            "Campaigns · Brand · Reputation · Research · Demand"),
        (DepartmentType.Production, "Production", "Production", "🏭", "#EF4444",
            "Manufacturing · Maintenance · Quality and Quantity Controls"),
        (DepartmentType.Warehouse, "Warehouse", "Warehouse", "📦", "#F97316",
            "Inventory · Stock Levels · Resource Planning · Storage"),
        (DepartmentType.Logistics, "Logistics", "Logistics", "🚚", "#06B6D4",
            "Shipments · Routes · Delivery SLAs · Fleet and Vehicle Controls"),
        (DepartmentType.Management, "Board Chairman", "Management", "🏛️", "#EAB308",
            "Full Oversight · All Reports · Strategic Decisions"),
    ];

    public LobbyViewModel(OnboardingFlowService flow)
    {
        _flow = flow;

        var config = flow.SessionConfig;
        IsMultiplayer = config?.IsMultiplayer ?? false;
        RoomCode = config?.RoomCode ?? "";
        IsHost = config?.Players.Any(p => p.IsLocalPlayer && p.IsHost) ?? true;

        PlayerName = flow.CurrentProfile?.Username ?? "Player";
        PlayerAvatar = flow.CurrentProfile?.AvatarEmoji ?? "👤";
        PlayerLevel = flow.CurrentProfile?.Level ?? 1;

        IsDarkTheme = ThemeService.Instance.IsDark;

        BuildCards();
        SyncPlayers();
        RefreshCompanyState();
        SyncCardClaims();
        EvaluateCanStart();

        _pingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _pingTimer.Tick += (_, _) => SimulatePingUpdate();
        _pingTimer.Start();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void ToggleTheme()
    {
        ThemeService.Instance.Toggle();
        IsDarkTheme = ThemeService.Instance.IsDark;
    }

    [RelayCommand]
    private void CopyRoomCode()
    {
        // Clipboard copy — wired in code-behind
    }

    /// <summary>Host-only: leaves the lobby to configure company/industry/difficulty.</summary>
    [RelayCommand]
    private void ConfigureCompany() => _flow.ProceedToCompanySetup();

    [RelayCommand]
    private void SelectCard(DepartmentSelectionCard card)
    {
        if (card.IsTaken) return;
        if (IsReady) return; // un-ready first to change departments

        _flow.SelectDepartment(card.Role);

        HasLocalDepartment = true;
        PlayerDepartmentName = card.Department;
        PreviewCard = card;

        SyncCardClaims();
    }

    /// <summary>Hover preview — wired from the view's PointerEntered handler.</summary>
    [RelayCommand]
    private void SetPreview(DepartmentSelectionCard card) => PreviewCard = card;

    [RelayCommand]
    private void PrintIdCard()
    {
        if (!HasLocalDepartment) return;

        IsReady = !IsReady;
        PrintButtonLabel = IsReady ? "✓ ID Printed" : "Print My ID Card";

        var local = _flow.SessionConfig?.Players.FirstOrDefault(p => p.IsLocalPlayer);
        if (local != null)
            local.Status = IsReady ? LobbyPlayerStatus.Ready : LobbyPlayerStatus.Connected;

        SyncPlayers();
        EvaluateCanStart();

        StatusMessage = IsReady
            ? "ID printed. Waiting for the rest of the team..."
            : "Update your department, then print your ID again.";
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

    private void BuildCards()
    {
        Cards.Clear();
        foreach (var (department, title, deptLabel, emoji, color, resp) in DepartmentData)
        {
            Cards.Add(new DepartmentSelectionCard
            {
                Role = department,
                Title = title,
                Department = deptLabel,
                Emoji = emoji,
                AccentColor = color,
                ResponsibilitiesSummary = resp,
            });
        }
    }

    private void SyncCardClaims()
    {
        var config = _flow.SessionConfig;
        int covered = 0;

        foreach (var card in Cards)
        {
            var claimedBy = config?.Players.FirstOrDefault(p => p.AssignedRole == card.Role);
            bool isLocal = claimedBy?.IsLocalPlayer == true;
            bool isTakenByOther = claimedBy != null && !isLocal;

            card.IsTaken = isTakenByOther;
            card.TakenByUsername = isTakenByOther ? claimedBy!.Username : null;
            card.IsSelected = isLocal;

            if (claimedBy != null && card.Role != DepartmentType.Management)
                covered++;
        }

        DepartmentsCovered = Math.Min(covered, DepartmentsTotal);
        CoverageRatio = DepartmentsTotal > 0 ? (double)DepartmentsCovered / DepartmentsTotal : 0;

        SelectedCard = Cards.FirstOrDefault(c => c.IsSelected);
        PreviewCard ??= SelectedCard ?? Cards.FirstOrDefault();
    }

    private void SyncPlayers()
    {
        Players.Clear();

        foreach (var p in _flow.SessionConfig?.Players ?? [])
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
    }

    private void RefreshCompanyState()
    {
        IsCompanyConfigured = _flow.IsCompanyConfigured;
        ShowConfigureCompanyButton = IsHost && !IsCompanyConfigured;
        ShowWaitingForCompany = !IsHost && !IsCompanyConfigured;

        var config = _flow.SessionConfig;
        CompanyName = config?.CompanyName ?? "";
        IndustryLabel = config?.Industry.ToString() ?? "";
        DifficultyLabel = config?.Difficulty.ToString() ?? "";
        CapitalDisplay = config != null ? $"${config.InitialCapital:N0}" : "";

        CompanyConfigSummary = IsCompanyConfigured
            ? $"{CompanyName} · {IndustryLabel} · {DifficultyLabel} Difficulty"
            : IsHost
                ? "Configure your company to continue."
                : "Waiting for the host to configure the company...";

        var localPlayer = config?.Players.FirstOrDefault(p => p.IsLocalPlayer);
        HasLocalDepartment = localPlayer?.AssignedRole.HasValue ?? false;
        if (HasLocalDepartment)
            PlayerDepartmentName = DepartmentTypeToNameConverter.GetDisplayName(localPlayer!.AssignedRole!.Value);

        if (!IsCompanyConfigured)
        {
            StatusMessage = IsHost
                ? "Configure your company to unlock department selection."
                : "Waiting for the host to configure the company...";
        }
    }

    private void EvaluateCanStart()
    {
        if (!IsMultiplayer)
        {
            CanStart = IsCompanyConfigured && HasLocalDepartment;
            return;
        }

        if (!IsHost) { CanStart = false; return; }

        var players = _flow.SessionConfig?.Players ?? [];
        CanStart = IsCompanyConfigured
                   && HasLocalDepartment
                   && players.Count >= 1
                   && players.All(p => p.Status == LobbyPlayerStatus.Ready);
    }

    private void SimulatePingUpdate()
    {
        LocalPing = Random.Shared.Next(8, 45);

        var local = _flow.SessionConfig?.Players.FirstOrDefault(p => p.IsLocalPlayer);
        if (local != null)
            local.PingMs = LocalPing;

        SyncPlayers();
        SyncCardClaims();
    }
}

// ── Row model ────────────────────────────────────────────────────────────────

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
}