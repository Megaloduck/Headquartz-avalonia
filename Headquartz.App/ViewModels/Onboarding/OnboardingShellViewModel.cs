using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using Headquartz.App.Services;
using Headquartz.App.ViewModels;


namespace Headquartz.App.ViewModels;

/// <summary>
/// Top-level shell for the onboarding flow.
/// Owns the OnboardingFlowService and routes between
/// Splash → Profile → MainMenu → Lobby → CompanySetup → DeptSelection → Gameplay.
/// Once the flow completes (Gameplay), the app swaps to ShellViewModel.
/// </summary>
public partial class OnboardingShellViewModel : ViewModelBase
{
    private readonly OnboardingFlowService _flow;

    [ObservableProperty]
    private ViewModelBase _currentView;

    /// <summary>
    /// Fires when the player has completed the onboarding flow
    /// and is ready to enter the simulation.
    /// </summary>
    public event Action<OnboardingFlowService>? OnboardingComplete;

    public OnboardingShellViewModel()
    {
        _flow = new OnboardingFlowService();
        _flow.ScreenChanged += HandleScreenChanged;

        // Start at splash
        _currentView = new SplashViewModel(_flow);
    }

    private void HandleScreenChanged(OnboardingScreen screen)
    {
        CurrentView = screen switch
        {
            OnboardingScreen.Splash =>
                new SplashViewModel(_flow),

            OnboardingScreen.ProfileCreation =>
                new ProfileCreationViewModel(_flow),

            OnboardingScreen.MainMenu =>
                new MainMenuViewModel(_flow),

            OnboardingScreen.Lobby =>
                new LobbyViewModel(_flow),

            OnboardingScreen.CompanySetup =>
                new CompanySetupViewModel(_flow),

            OnboardingScreen.DepartmentSelection =>
                new DepartmentSelectionViewModel(_flow),

            OnboardingScreen.Gameplay =>
                HandleGameplayTransition(),

            _ => CurrentView,
        };
    }

    private ViewModelBase HandleGameplayTransition()
    {
        // Notify the root app to swap the main window content
        OnboardingComplete?.Invoke(_flow);

        // Return current view while transition happens
        return CurrentView;
    }
}
