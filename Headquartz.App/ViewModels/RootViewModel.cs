using CommunityToolkit.Mvvm.ComponentModel;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;
using Headquartz.Simulation;
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
        var profile = SimulationProfile.FromDifficulty(
            flow.SessionConfig?.Difficulty
            ?? GameDifficulty.Manager);

        var industry = flow.SessionConfig!.Industry;

        var simulation = new SimulationService(profile, industry);

        if (!string.IsNullOrEmpty(flow.SessionConfig?.CompanyName))
            simulation.Engine.Company.Name = flow.SessionConfig.CompanyName;

        var localPlayer = flow.SessionConfig?.Players
            .FirstOrDefault(p => p.IsLocalPlayer);

        var startingDepartment = localPlayer?.AssignedRole
            ?? DepartmentType.Management;

        // Under 7 players, nobody has full coverage — everyone can switch into
        // any unclaimed department from the shell's role picker. At 7, seats
        // are 1:1 and the switcher locks.
        int playerCount = flow.SessionConfig?.Players.Count ?? 1;
        bool canSwitchDepartments = playerCount < 7;

        _ = simulation.StartAsync();

        ActiveShell = new ShellViewModel(simulation, startingDepartment, canSwitchDepartments);
    }
}