using Headquartz.App.Models.Onboarding;
using Headquartz.Domain.Enums;
using System;
using System.Linq;

namespace Headquartz.App.Services;

/// <summary>
/// Central state for the onboarding flow.
///
/// Host path:   MainMenu -> Lobby -> CompanySetup -> Lobby (pick dept + ready) -> Gameplay
/// Joiner path: MainMenu -> Lobby (pick dept + ready) -> Gameplay
/// Solo path:   MainMenu -> CompanySetup -> Lobby (pick dept) -> Gameplay
///
/// Department selection now lives inside the Lobby screen itself — there is no
/// separate DepartmentSelection screen anymore. Only the host ever configures
/// the company; joiners connect to the lobby and pick a department directly.
/// </summary>
public class OnboardingFlowService
{
    public PlayerProfile? CurrentProfile { get; private set; }
    public GameSessionConfig? SessionConfig { get; private set; }

    public event Action<OnboardingScreen>? ScreenChanged;

    public OnboardingScreen CurrentScreen { get; private set; } = OnboardingScreen.Splash;

    // ── Derived flow state ───────────────────────────────────

    /// <summary>True for the lobby host, and always true in solo play.</summary>
    public bool IsLocalPlayerHost =>
        SessionConfig == null ||
        (SessionConfig.Players.FirstOrDefault(p => p.IsLocalPlayer)?.IsHost ?? true);

    /// <summary>True once the host has confirmed company/industry/difficulty.</summary>
    public bool IsCompanyConfigured =>
        !string.IsNullOrWhiteSpace(SessionConfig?.CompanyName);

    /// <summary>True once the local player has picked a department to run.</summary>
    public bool LocalPlayerHasDepartment =>
        SessionConfig?.Players.FirstOrDefault(p => p.IsLocalPlayer)?.AssignedRole.HasValue ?? false;

    // ── Actions ───────────────────────────────────────────────

    public void CompleteProfile(PlayerProfile profile)
    {
        CurrentProfile = profile;
        NavigateTo(OnboardingScreen.MainMenu);
    }

    public void StartHosting()
    {
        SessionConfig = new GameSessionConfig
        {
            IsMultiplayer = true,
            RoomCode = GenerateRoomCode(),
            HostName = CurrentProfile?.Username ?? "Host",
        };

        SessionConfig.Players.Add(new LobbyPlayer
        {
            Username = CurrentProfile?.Username ?? "Host",
            AvatarEmoji = CurrentProfile?.AvatarEmoji ?? "👤",
            Level = CurrentProfile?.Level ?? 1,
            Status = LobbyPlayerStatus.Connected,
            IsHost = true,
            IsLocalPlayer = true,
        });

        NavigateTo(OnboardingScreen.Lobby);
    }

    public void StartJoining(string roomCode)
    {
        SessionConfig = new GameSessionConfig
        {
            IsMultiplayer = true,
            RoomCode = roomCode.ToUpperInvariant(),
            HostName = "",
        };

        SessionConfig.Players.Add(new LobbyPlayer
        {
            Username = CurrentProfile?.Username ?? "Player",
            AvatarEmoji = CurrentProfile?.AvatarEmoji ?? "👤",
            Level = CurrentProfile?.Level ?? 1,
            Status = LobbyPlayerStatus.Connecting,
            IsHost = false,
            IsLocalPlayer = true,
        });

        NavigateTo(OnboardingScreen.Lobby);
    }

    public void StartSinglePlayer()
    {
        SessionConfig = new GameSessionConfig
        {
            IsMultiplayer = false,
            RoomCode = "",
            HostName = CurrentProfile?.Username ?? "Player",
        };

        SessionConfig.Players.Add(new LobbyPlayer
        {
            Username = CurrentProfile?.Username ?? "Player",
            AvatarEmoji = CurrentProfile?.AvatarEmoji ?? "👤",
            Level = CurrentProfile?.Level ?? 1,
            Status = LobbyPlayerStatus.Connected,
            IsHost = true,
            IsLocalPlayer = true,
        });

        // Solo still configures a company first — but Lobby (not a separate
        // DepartmentSelection screen) is where the department gets picked.
        NavigateTo(OnboardingScreen.CompanySetup);
    }

    /// <summary>Host-only: leaves the lobby to configure the company.</summary>
    public void ProceedToCompanySetup()
    {
        if (!IsLocalPlayerHost) return;
        NavigateTo(OnboardingScreen.CompanySetup);
    }

    public void ConfirmCompanySetup(
        string name,
        IndustryType industry,
        GameDifficulty difficulty,
        decimal capital)
    {
        if (SessionConfig == null) return;

        SessionConfig.CompanyName = name;
        SessionConfig.Industry = industry;
        SessionConfig.Difficulty = difficulty;
        SessionConfig.InitialCapital = capital;

        // Back to the (now unlocked) Lobby to pick a department and ready up.
        NavigateTo(OnboardingScreen.Lobby);
    }

    /// <summary>
    /// Assigns a department to the local player. Called directly from the
    /// Lobby screen — no navigation involved, since department selection
    /// now happens in-place there.
    /// </summary>
    public void SelectDepartment(DepartmentType department)
    {
        var local = SessionConfig?.Players.FirstOrDefault(p => p.IsLocalPlayer);
        if (local == null) return;

        local.AssignedRole = department;

        if (local.Status == LobbyPlayerStatus.Connecting)
            local.Status = LobbyPlayerStatus.Connected;
    }

    /// <summary>Host-only (or solo): launches the simulation once everyone is ready.</summary>
    public void LaunchGame()
    {
        if (!IsLocalPlayerHost) return;
        NavigateTo(OnboardingScreen.Gameplay);
    }

    public void NavigateTo(OnboardingScreen screen)
    {
        CurrentScreen = screen;
        ScreenChanged?.Invoke(screen);
    }

    // ── Helpers ───────────────────────────────────────────────

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[6];
        for (int i = 0; i < 6; i++)
            code[i] = chars[Random.Shared.Next(chars.Length)];
        return new string(code);
    }
}

public enum OnboardingScreen
{
    Splash,
    ProfileCreation,
    MainMenu,
    Lobby,
    CompanySetup,
    Gameplay,
}   