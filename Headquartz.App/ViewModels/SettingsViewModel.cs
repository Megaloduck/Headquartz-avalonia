using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Services;

namespace Headquartz.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── Simulation speed ──────────────────────────────────────

    [ObservableProperty] private double _currentSpeed = 1.0;
    [ObservableProperty] private string _speedLabel = "1× Normal";
    [ObservableProperty] private long _currentTick;
    [ObservableProperty] private string _worldTime = "";

    // ── Company ───────────────────────────────────────────────

    [ObservableProperty] private string _companyName = "";
    [ObservableProperty] private string _statusMessage = "";

    // ── Constructor ───────────────────────────────────────────

    public SettingsViewModel(SimulationService simulation)
    {
        _simulation = simulation;
        CompanyName = simulation.Engine.Company.Name;
        CurrentTick = simulation.Engine.Clock.Tick;
        WorldTime = simulation.Engine.Clock.WorldTime.ToString("yyyy-MM-dd HH:mm");

        simulation.Engine.OnUpdated += RefreshTick;
    }

    // ── Speed commands ────────────────────────────────────────

    [RelayCommand]
    private void SetHalfSpeed() => ApplySpeed(0.5, "½× Slow");

    [RelayCommand]
    private void SetNormalSpeed() => ApplySpeed(1.0, "1× Normal");

    [RelayCommand]
    private void SetDoubleSpeed() => ApplySpeed(2.0, "2× Fast");

    [RelayCommand]
    private void SetTripleSpeed() => ApplySpeed(3.0, "3× Turbo");

    // ── Company name ─────────────────────────────────────────

    [RelayCommand]
    private void ApplyCompanyName()
    {
        string trimmed = CompanyName.Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        _simulation.Engine.Company.Name = trimmed;
        StatusMessage = $"✅ Company renamed to \"{trimmed}\".";
    }

    // ── Helpers ───────────────────────────────────────────────

    private void ApplySpeed(double multiplier, string label)
    {
        _simulation.SetTickSpeed(multiplier);
        CurrentSpeed = multiplier;
        SpeedLabel = label;
        StatusMessage = $"✅ Speed set to {label}.";
    }

    private void RefreshTick()
    {
        CurrentTick = _simulation.Engine.Clock.Tick;
        WorldTime = _simulation.Engine.Clock.WorldTime.ToString("yyyy-MM-dd HH:mm");
    }
}