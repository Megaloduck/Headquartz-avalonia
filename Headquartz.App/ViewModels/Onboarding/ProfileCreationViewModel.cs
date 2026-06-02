using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models.Onboarding;
using Headquartz.App.Services;
using Headquartz.App.ViewModels;

namespace Headquartz.App.ViewModels;

public partial class ProfileCreationViewModel : ViewModelBase
{
    private readonly OnboardingFlowService _flow;

    // ── Form state ────────────────────────────────────────────

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _selectedAvatar = "👨‍💼";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _hasError;

    // ── Stats (shown on profile card preview) ────────────────

    [ObservableProperty] private string _previewLevel = "Lv. 1";
    [ObservableProperty] private string _previewUsername = "Your Name";

    // ── Avatar grid ───────────────────────────────────────────

    public ObservableCollection<AvatarOptionModel> Avatars { get; } = [];

    public ProfileCreationViewModel(OnboardingFlowService flow)
    {
        _flow = flow;
        LoadAvatars();
    }

    private void LoadAvatars()
    {
        foreach (var emoji in AvatarOptions.All)
        {
            Avatars.Add(new AvatarOptionModel
            {
                Emoji = emoji,
                IsSelected = emoji == SelectedAvatar,
            });
        }
    }

    partial void OnUsernameChanged(string value)
    {
        PreviewUsername = string.IsNullOrWhiteSpace(value) ? "Your Name" : value.Trim();
        HasError = false;
        ErrorMessage = "";
    }

    partial void OnSelectedAvatarChanged(string value)
    {
        foreach (var a in Avatars)
            a.IsSelected = a.Emoji == value;
    }

    [RelayCommand]
    private void SelectAvatar(AvatarOptionModel avatar)
    {
        SelectedAvatar = avatar.Emoji;
    }

    [RelayCommand]
    private void CreateProfile()
    {
        string trimmed = Username.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            ErrorMessage = "Please enter a username to continue.";
            HasError = true;
            return;
        }

        if (trimmed.Length < 2)
        {
            ErrorMessage = "Username must be at least 2 characters.";
            HasError = true;
            return;
        }

        if (trimmed.Length > 24)
        {
            ErrorMessage = "Username must be 24 characters or fewer.";
            HasError = true;
            return;
        }

        var profile = new PlayerProfile
        {
            Username = trimmed,
            AvatarEmoji = SelectedAvatar,
        };

        _flow.CompleteProfile(profile);
    }
}

public partial class AvatarOptionModel : ObservableObject
{
    public string Emoji { get; set; } = "";

    [ObservableProperty] private bool _isSelected;
}