using Headquartz.App.Models.Onboarding;
using Headquartz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Headquartz.App.Services;

/// <summary>
/// Central state for the onboarding flow.
/// Lives as a singleton shared by all onboarding ViewModels.
/// </summary>
public class OnboardingFlowService
{
    // ── Current player ────────────────────────────────────────
    public PlayerProfile? CurrentProfile { get; private set; }

    // ── Active session config ─────────────────────────────────
    public GameSessionConfig? SessionConfig { get; private set; }

    // ── Navigation ────────────────────────────────────────────
    public event Action<OnboardingScreen>? ScreenChanged;

    public OnboardingScreen CurrentScreen { get; private set; }
        = OnboardingScreen.Splash;

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
            Status = LobbyPlayerStatus.Ready,
            IsHost = true,
            IsLocalPlayer = true,
        });

        NavigateTo(OnboardingScreen.CompanySetup);
    }

    public void ConfirmLobby()
    {
        NavigateTo(OnboardingScreen.CompanySetup);
    }

    public void ConfirmCompanySetup(string name, IndustryType industry,
        GameDifficulty difficulty, decimal capital)
    {
        if (SessionConfig == null) return;

        SessionConfig.CompanyName = name;
        SessionConfig.Industry = industry;
        SessionConfig.Difficulty = difficulty;
        SessionConfig.InitialCapital = capital;

        NavigateTo(OnboardingScreen.DepartmentSelection);
    }

    public void SelectDepartment(PlayerRole role)
    {
        // Mark local player's role in the session
        var local = SessionConfig?.Players
            .FirstOrDefault(p => p.IsLocalPlayer);

        if (local != null)
            local.AssignedRole = role;

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
