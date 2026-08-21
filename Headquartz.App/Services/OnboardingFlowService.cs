using Headquartz.App.Models.Onboarding;
using Headquartz.Domain.Enums;
using System;
using System.Linq;

namespace Headquartz.App.Services;

/// <summary>
/// Central state for the onboarding flow.
///
/// Host path:   MainMenu -> Lobby -> CompanySetup -> DepartmentSelection -> Lobby -> Gameplay
/// Joiner path: MainMenu -> Lobby -> DepartmentSelection -> Lobby -> Gameplay
/// Solo path:   MainMenu -> CompanySetup -> DepartmentSelection -> Gameplay (no lobby)
///
/// Only the host ever configures the company. Joiners connect to the lobby
/// and go straight to picking a department. Both paths route back through
/// the lobby afterward — only the host's explicit LaunchGame() call ends
/// onboarding and enters the simulation.
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

        // Host lands in the Lobby first. Company setup is a deliberate
        // next step triggered from there via ProceedToCompanySetup().
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

        // Non-host players never configure the company — they connect to
        // the lobby and go straight to picking a department.
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
            Status = LobbyPlayerStatus.Ready,
            IsHost = true,
            IsLocalPlayer = true,
        });

        // Solo play skips the lobby entirely.
        NavigateTo(OnboardingScreen.CompanySetup);
    }

    /// <summary>Host-only: leaves the lobby to configure the company.</summary>
    public void ProceedToCompanySetup()
    {
        if (!IsLocalPlayerHost) return;
        NavigateTo(OnboardingScreen.CompanySetup);
    }

    /// <summary>
    /// Sent to DepartmentSelection either by the host (right after
    /// configuring the company) or by a joining player (right after
    /// connecting to the lobby).
    /// </summary>
    public void ProceedToDepartmentSelection()
    {
        NavigateTo(OnboardingScreen.DepartmentSelection);
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

        // The host picks their own department immediately after setup.
        NavigateTo(OnboardingScreen.DepartmentSelection);
    }

    public void SelectDepartment(DepartmentType department)
    {
        var local = SessionConfig?.Players.FirstOrDefault(p => p.IsLocalPlayer);

        if (local != null)
        {
            local.AssignedRole = department;

            if (local.Status == LobbyPlayerStatus.Connecting)
                local.Status = LobbyPlayerStatus.Connected;
        }

        if (SessionConfig == null || !SessionConfig.IsMultiplayer)
        {
            // Solo: no lobby — straight into the simulation.
            NavigateTo(OnboardingScreen.Gameplay);
        }
        else
        {
            // Multiplayer: back to the lobby to ready up / wait for others.
            NavigateTo(OnboardingScreen.Lobby);
        }
    }

    /// <summary>Host-only: launches the simulation once everyone is ready.</summary>
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
    DepartmentSelection,
    Gameplay,
}