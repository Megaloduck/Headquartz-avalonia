using CommunityToolkit.Mvvm.ComponentModel;
using Headquartz.App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Headquartz.App.ViewModels;

/// <summary>
/// The root view model owned by MainWindow.
/// It starts with OnboardingShellViewModel and swaps to
/// ShellViewModel once the player completes onboarding.
/// </summary>
public partial class RootViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _activeShell;

    public RootViewModel()
    {
        var onboarding = new OnboardingShellViewModel();
        onboarding.OnboardingComplete += HandleOnboardingComplete;
        _activeShell = onboarding;
    }

    private void HandleOnboardingComplete(OnboardingFlowService flow)
    {
        // Build SimulationService with config from the onboarding flow
        var simulation = new SimulationService();

        // Apply company name from session config
        if (!string.IsNullOrEmpty(flow.SessionConfig?.CompanyName))
            simulation.Engine.Company.Name = flow.SessionConfig.CompanyName;

        // Apply starting capital from difficulty
        if (flow.SessionConfig != null)
            simulation.Engine.Company.Cash = flow.SessionConfig.InitialCapital;

        // Determine starting role for the local player
        var localPlayer = flow.SessionConfig?.Players
            .FirstOrDefault(p => p.IsLocalPlayer);

        var startingRole = localPlayer?.AssignedRole
            ?? Headquartz.Domain.Enums.PlayerRole.Chairman;

        // Start simulation loop
        _ = simulation.StartAsync();

        // Swap shell to gameplay
        ActiveShell = new ShellViewModel(simulation, startingRole);
    }
}
