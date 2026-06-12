using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using System;
using System.Linq;

namespace Headquartz.App.Services;

/// <summary>
/// Handles runtime dark/light theme switching.
///
/// Strategy: App.axaml holds a single permanent <ResourceInclude> wrapper
/// whose Source is swapped at runtime. We never add/remove from
/// MergedDictionaries (that crashes Avalonia) — we only mutate the
/// Source property of the existing ResourceInclude entry.
/// </summary>
public class ThemeService
{
    // ── Singleton ─────────────────────────────────────────────

    public static ThemeService Instance { get; } = new();

    // ── State ─────────────────────────────────────────────────

    private bool _isDark = true;

    public bool IsDark => _isDark;

    public event Action<bool>? ThemeChanged;

    // ── URIs ──────────────────────────────────────────────────

    private static readonly Uri DarkUri =
        new("avares://Headquartz.App/Assets/Themes/DarkTheme.axaml");

    private static readonly Uri LightUri =
        new("avares://Headquartz.App/Assets/Themes/LightTheme.axaml");

    // ── Public API ────────────────────────────────────────────

    public void Toggle()
    {
        _isDark = !_isDark;
        Apply();
    }

    public void SetDark(bool dark)
    {
        if (_isDark == dark) return;
        _isDark = dark;
        Apply();
    }

    // ── Internal ──────────────────────────────────────────────

    private void Apply()
    {
        var app = Application.Current;
        if (app is null) return;

        var merged = app.Resources.MergedDictionaries;

        // Find the first ResourceInclude that points to either theme file.
        // This is the permanent wrapper we installed in App.axaml.
        var themeInclude = merged
            .OfType<ResourceInclude>()
            .FirstOrDefault(r =>
                r.Source == DarkUri ||
                r.Source == LightUri);

        if (themeInclude is not null)
        {
            themeInclude.Source = _isDark ? DarkUri : LightUri;
        }

        ThemeChanged?.Invoke(_isDark);
    }
}