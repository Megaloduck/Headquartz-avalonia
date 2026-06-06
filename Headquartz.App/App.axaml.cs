using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

using Headquartz.App.Views;
using System;
using System.Linq;

namespace Headquartz.App;

public partial class App : Application
{
    // ── Singleton access ──────────────────────────────────────

    public static App Current =>
        (App)Application.Current!;

    // ── Theme state ───────────────────────────────────────────

    private bool _isDark = true;

    public bool IsDarkTheme => _isDark;

    // ── Lifecycle ─────────────────────────────────────────────

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is
            IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    // ── Public API ────────────────────────────────────────────

    /// <summary>
    /// Swap the active theme at runtime.
    /// Call from SettingsViewModel or any other place.
    /// </summary>
    public void SetTheme(bool dark)
    {
        if (_isDark == dark) return;

        _isDark = dark;

        // Remove all StyleIncludes that reference our theme files
        var toRemove = Styles
            .OfType<StyleInclude>()
            .Where(s =>
                s.Source != null &&
                (s.Source.ToString().Contains("DarkTheme") ||
                 s.Source.ToString().Contains("LightTheme")))
            .ToList();

        foreach (var style in toRemove)
            Styles.Remove(style);

        // Add the correct one
        string uri = dark
            ? "avares://Headquartz.App/Assets/Themes/DarkTheme.axaml"
            : "avares://Headquartz.App/Assets/Themes/LightTheme.axaml";

        Styles.Add(new StyleInclude(new Uri("avares://Headquartz.App"))
        {
            Source = new Uri(uri),
        });

        // Also update Fluent's RequestedThemeVariant for controls
        // that rely on the system variant (scrollbars, etc.)
        RequestedThemeVariant = dark
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
    }
}