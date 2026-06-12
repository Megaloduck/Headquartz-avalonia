using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using System;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // =========================================================
    // SIMULATION
    // =========================================================

    [ObservableProperty] private long _currentTick;
    [ObservableProperty] private string _worldTime = "";
    [ObservableProperty] private string _speedLabel = "1× Normal";
    [ObservableProperty] private double _currentSpeed = 1.0;

    // Which speed button is active — drives border highlight
    [ObservableProperty] private bool _isHalfSpeed;
    [ObservableProperty] private bool _isNormalSpeed = true;
    [ObservableProperty] private bool _isDoubleSpeed;
    [ObservableProperty] private bool _isTripleSpeed;

    // =========================================================
    // APPEARANCE
    // =========================================================

    /// <summary>Mirrors ThemeService.IsDark — drives toggle state in UI.</summary>
    [ObservableProperty] private bool _isDarkTheme;

    // =========================================================
    // COMPANY
    // =========================================================

    [ObservableProperty] private string _companyName = "";
    [ObservableProperty] private string _companyStatus = "";
    [ObservableProperty] private bool _companyHasError;

    // =========================================================
    // GAME INFO
    // =========================================================

    [ObservableProperty] private int _employeeCount;
    [ObservableProperty] private int _activeOrders;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private int _reputation;
    [ObservableProperty] private string _gameVersion = "v0.1.0-alpha";

    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public SettingsViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        // Mirror current theme state
        IsDarkTheme = ThemeService.Instance.IsDark;
        ThemeService.Instance.ThemeChanged += on => IsDarkTheme = on;

        CompanyName = simulation.Engine.Company.Name;

        RefreshStats();
        simulation.Engine.OnUpdated += () =>
            Dispatcher.UIThread.Post(RefreshStats);
    }

    // =========================================================
    // SIMULATION SPEED COMMANDS
    // =========================================================

    [RelayCommand]
    private void SetHalfSpeed() => ApplySpeed(0.5, "½× Slow",
        half: true, normal: false, dbl: false, triple: false);

    [RelayCommand]
    private void SetNormalSpeed() => ApplySpeed(1.0, "1× Normal",
        half: false, normal: true, dbl: false, triple: false);

    [RelayCommand]
    private void SetDoubleSpeed() => ApplySpeed(2.0, "2× Fast",
        half: false, normal: false, dbl: true, triple: false);

    [RelayCommand]
    private void SetTripleSpeed() => ApplySpeed(3.0, "3× Turbo",
        half: false, normal: false, dbl: false, triple: true);

    private void ApplySpeed(
        double multiplier, string label,
        bool half, bool normal, bool dbl, bool triple)
    {
        _simulation.SetTickSpeed(multiplier);
        CurrentSpeed = multiplier;
        SpeedLabel = label;
        IsHalfSpeed = half;
        IsNormalSpeed = normal;
        IsDoubleSpeed = dbl;
        IsTripleSpeed = triple;
    }

    // =========================================================
    // APPEARANCE COMMANDS
    // =========================================================

    [RelayCommand]
    private void SetDarkTheme()
    {
        ThemeService.Instance.SetDark(true);
        IsDarkTheme = true;
    }

    [RelayCommand]
    private void SetLightTheme()
    {
        ThemeService.Instance.SetDark(false);
        IsDarkTheme = false;
    }

    // =========================================================
    // COMPANY COMMANDS
    // =========================================================

    [RelayCommand]
    private void ApplyCompanyName()
    {
        string trimmed = CompanyName.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            CompanyStatus = "Company name cannot be empty.";
            CompanyHasError = true;
            return;
        }

        if (trimmed.Length < 2 || trimmed.Length > 40)
        {
            CompanyStatus = "Name must be between 2 and 40 characters.";
            CompanyHasError = true;
            return;
        }

        _simulation.Engine.Company.Name = trimmed;
        CompanyStatus = $"Renamed to \"{trimmed}\" successfully.";
        CompanyHasError = false;
    }

    // =========================================================
    // REFRESH
    // =========================================================

    private void RefreshStats()
    {
        var engine = _simulation.Engine;
        var company = engine.Company;

        CurrentTick = engine.Clock.Tick;
        WorldTime = engine.Clock.WorldTime.ToString("yyyy-MM-dd HH:mm");
        EmployeeCount = company.Employees.Count;
        ActiveOrders = company.Orders.Count(o =>
            o.Status is not Domain.Enums.OrderStatus.Delivered
                     and not Domain.Enums.OrderStatus.Cancelled);
        CompanyCash = company.Cash;
        Reputation = company.Reputation;
    }
}