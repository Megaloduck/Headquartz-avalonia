using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Services;
using Headquartz.App.ViewModels;

namespace Headquartz.App.ViewModels;

public partial class MainMenuViewModel : ViewModelBase
{
    private readonly OnboardingFlowService _flow;

    [ObservableProperty] private string _playerName = "";
    [ObservableProperty] private string _playerAvatar = "👤";
    [ObservableProperty] private int _playerLevel = 1;
    [ObservableProperty] private bool _showJoinInput;
    [ObservableProperty] private string _joinRoomCode = "";
    [ObservableProperty] private string _joinError = "";
    [ObservableProperty] private bool _hasJoinError;

    public MainMenuViewModel(OnboardingFlowService flow)
    {
        _flow = flow;

        if (flow.CurrentProfile != null)
        {
            PlayerName = flow.CurrentProfile.Username;
            PlayerAvatar = flow.CurrentProfile.AvatarEmoji;
            PlayerLevel = flow.CurrentProfile.Level;
        }
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void HostGame()
    {
        _flow.StartHosting();
    }

    [RelayCommand]
    private void ToggleJoinInput()
    {
        ShowJoinInput = !ShowJoinInput;
        JoinRoomCode = "";
        JoinError = "";
        HasJoinError = false;
    }

    [RelayCommand]
    private void ConfirmJoin()
    {
        string code = JoinRoomCode.Trim().ToUpperInvariant();

        if (code.Length != 6)
        {
            JoinError = "Room code must be 6 characters.";
            HasJoinError = true;
            return;
        }

        _flow.StartJoining(code);
    }

    [RelayCommand]
    private void PlaySolo()
    {
        _flow.StartSinglePlayer();
    }

    [RelayCommand]
    private void OpenTutorial()
    {
        // TODO: navigate to tutorial
    }

    [RelayCommand]
    private void OpenSettings()
    {
        // TODO: open settings overlay
    }

    [RelayCommand]
    private void OpenCredits()
    {
        // TODO: open credits overlay
    }

    [RelayCommand]
    private void ExitGame()
    {
        // In Avalonia desktop
        System.Environment.Exit(0);
    }
}
