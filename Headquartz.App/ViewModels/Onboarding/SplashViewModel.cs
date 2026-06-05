using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Services;
using Headquartz.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headquartz.App.ViewModels;

public partial class SplashViewModel : ViewModelBase
{
    private readonly OnboardingFlowService _flow;

    [ObservableProperty] private double _loadingProgress;
    [ObservableProperty] private string _loadingMessage = "Initializing systems...";
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _gameVersion = "v0.1.0-alpha";

    private static readonly string[] LoadingMessages =
    [
        "Initializing systems...",
        "Loading simulation engine...",
        "Connecting departments...",
        "Preparing company assets...",
        "Calibrating market conditions...",
        "Ready.",
    ];

    public SplashViewModel(OnboardingFlowService flow)
    {
        _flow = flow;
        _ = RunLoadingSequenceAsync();
    }

    private async Task RunLoadingSequenceAsync()
    {
        for (int i = 0; i < LoadingMessages.Length; i++)
        {
            LoadingMessage = LoadingMessages[i];
            LoadingProgress = (double)(i + 1) / LoadingMessages.Length * 100;
            await Task.Delay(420);
        }

        await Task.Delay(300);
        IsLoading = false;

        // Auto-advance to profile after splash
        await Task.Delay(600);
        _flow.NavigateTo(OnboardingScreen.ProfileCreation);
    }
}
