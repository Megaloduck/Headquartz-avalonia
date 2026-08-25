using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

using Headquartz.Domain.Enums;

namespace Headquartz.App.Models.Onboarding;

// ── Player Profile ────────────────────────────────────────────────────────────

public class PlayerProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = "";
    public string AvatarEmoji { get; set; } = "👤";
    public int Level { get; set; } = 1;
    public int TotalGamesPlayed { get; set; }
    public int TotalWins { get; set; }
    public decimal TotalRevenueEarned { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;
}

// ── Game Session ──────────────────────────────────────────────────────────────

public class GameSessionConfig
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public string CompanyName { get; set; } = "";
    public IndustryType Industry { get; set; } = IndustryType.Food;
    public GameDifficulty Difficulty { get; set; } = GameDifficulty.Manager;
    public decimal InitialCapital { get; set; } = 100_000m;
    public bool IsMultiplayer { get; set; }
    public string RoomCode { get; set; } = "";
    public string HostName { get; set; } = "";
    public List<LobbyPlayer> Players { get; set; } = [];
}

// ── Lobby ─────────────────────────────────────────────────────────────────────

public enum LobbyPlayerStatus
{
    Connecting,
    Connected,
    Ready,
    Disconnected
}

public class LobbyPlayer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = "";
    public string AvatarEmoji { get; set; } = "👤";
    public int Level { get; set; } = 1;

    /// <summary>
    /// Which department this player is running. A "role" is now purely
    /// "which department do I run" — PlayerRole has been retired in favor
    /// of reusing DepartmentType directly, since the two enums represented
    /// the exact same eight values and kept drifting out of sync (most
    /// visibly: PlayerRole titles all said "X Manager", which collided
    /// with GameDifficulty.Manager once the shell started showing
    /// "Role · Difficulty" together).
    /// </summary>
    public DepartmentType? AssignedRole { get; set; }

    public LobbyPlayerStatus Status { get; set; } = LobbyPlayerStatus.Connecting;
    public int PingMs { get; set; }
    public bool IsHost { get; set; }
    public bool IsLocalPlayer { get; set; }
}

// ── Avatar options ────────────────────────────────────────────────────────────

public static class AvatarOptions
{
    public static readonly string[] All =
    [
        "🐨","🐮","🐻","🐭","🐰","🦝","🐼",
        "🐱","🐯","🦊","🐶","🦁","🐻‍❄", "🐺",
    ];
}

// ── Department Selection ──────────────────────────────────────────────────────

public partial class DepartmentSelectionCard : ObservableObject
{
    public DepartmentType Role { get; set; }
    public string Title { get; set; } = "";
    public string Department { get; set; } = "";
    public string Description { get; set; } = "";
    public string Emoji { get; set; } = "";
    public string AccentColor { get; set; } = "";
    public string ResponsibilitiesSummary { get; set; } = "";
    public string DifficultyLabel { get; set; } = "";
    public int DifficultyStars { get; set; } = 2;

    [ObservableProperty] private bool _isTaken;
    [ObservableProperty] private string? _takenByUsername;
    [ObservableProperty] private bool _isSelected;
}