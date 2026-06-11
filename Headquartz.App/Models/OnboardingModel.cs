using System;
using System.Collections.Generic;

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

public enum IndustryType
{
    Manufacturing,
    Technology,
    Retail,
    Logistics,
    Finance,
    Healthcare,
    Energy,
    Media
}

// GameDifficulty has been moved to Headquartz.Domain.Enums.GameDifficulty.
// It is re-exported here as a using alias so existing code in this namespace
// compiles without changes.
// If any file in App.Models.Onboarding references GameDifficulty directly,
// it will resolve via the Domain using above.

public class GameSessionConfig
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public string CompanyName { get; set; } = "";
    public IndustryType Industry { get; set; } = IndustryType.Manufacturing;
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
    public PlayerRole? AssignedRole { get; set; }
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
        "👨‍💼", "👩‍💼", "🧑‍💼", "👨‍🔧", "👩‍🔧", "🧑‍🔧",
        "👨‍💻", "👩‍💻", "🧑‍💻", "👨‍🎓", "👩‍🎓", "🧑‍🎓",
        "🕵️", "🧑‍✈️", "👨‍🚀", "👩‍🚀", "🧙", "🦸",
        "🤵", "👑", "🎩", "🦊", "🐺", "🦁"
    ];
}

// ── Department Selection ──────────────────────────────────────────────────────

public class DepartmentSelectionCard
{
    public PlayerRole Role { get; set; }
    public string Title { get; set; } = "";
    public string Department { get; set; } = "";
    public string Description { get; set; } = "";
    public string Emoji { get; set; } = "";
    public string AccentColor { get; set; } = "";
    public string ResponsibilitiesSummary { get; set; } = "";
    public string DifficultyLabel { get; set; } = "";
    public int DifficultyStars { get; set; } = 2;
    public bool IsTaken { get; set; }
    public string? TakenByUsername { get; set; }
    public bool IsSelected { get; set; }
}