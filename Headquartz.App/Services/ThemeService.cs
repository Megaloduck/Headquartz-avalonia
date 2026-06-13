using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Headquartz.App.Services;

public class ThemeService
{
    private static readonly Lazy<ThemeService> _instance =
        new(() => new ThemeService());

    public static ThemeService Instance => _instance.Value;

    private ResourceInclude? _activeTheme;

    private readonly Uri _darkTheme =
        new("avares://Headquartz.App/Assets/Themes/DarkTheme.axaml");

    private readonly Uri _lightTheme =
        new("avares://Headquartz.App/Assets/Themes/LightTheme.axaml");

    public bool IsDark { get; private set; } = true;

    public event Action<bool>? ThemeChanged;

    private ThemeService()
    {
    }

    public void Initialize()
    {
        SetDark(true);
    }

    public void Toggle()
    {
        SetDark(!IsDark);
    }

    public void SetDark(bool dark)
    {
        if (Application.Current is null)
            return;

        var themeUri = dark ? _darkTheme : _lightTheme;

        if (_activeTheme != null)
        {
            Application.Current.Resources
                .MergedDictionaries
                .Remove(_activeTheme);
        }

        _activeTheme = new ResourceInclude(themeUri)
        {
            Source = themeUri
        };

        Application.Current.Resources
            .MergedDictionaries
            .Add(_activeTheme);

        IsDark = dark;

        ThemeChanged?.Invoke(IsDark);
    }
}