using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Headquartz.App.Services;

/// <summary>
/// Manages the active Headquartz colour theme at runtime.
///
/// Strategy — avoids InvalidCastException and NullReferenceException:
///   A single "wrapper" ResourceDictionary is inserted once into
///   Application.Resources.MergedDictionaries and never removed.
///   Switching theme clears that wrapper and refills it with the
///   entries from the desired palette — no Remove/Insert cycle on
///   MergedDictionaries, which is what caused both previous crashes.
/// </summary>
public class ThemeService
{
    // ── Singleton ─────────────────────────────────────────────

    private static ThemeService? _instance;
    public static ThemeService Current => _instance ??= new ThemeService();

    // ── State ─────────────────────────────────────────────────

    public bool IsDark { get; private set; } = true;

    public event Action<bool>? ThemeChanged;

    // Wrapper dict that lives permanently in MergedDictionaries
    private readonly ResourceDictionary _wrapper = new();

    // Raw key→value snapshots loaded once at startup
    private Dictionary<object, object?> _darkEntries = new();
    private Dictionary<object, object?> _lightEntries = new();

    // ── Initialisation ────────────────────────────────────────

    /// <summary>
    /// Call once from App.OnFrameworkInitializationCompleted(),
    /// after AvaloniaXamlLoader.Load(this) has run.
    /// </summary>
    public void Initialize()
    {
        // Load both palettes into plain dictionaries
        _darkEntries = LoadEntries(
            "avares://Headquartz.App/Assets/Themes/DarkTheme.axaml");
        _lightEntries = LoadEntries(
            "avares://Headquartz.App/Assets/Themes/LightTheme.axaml");

        var appResources = Application.Current!.Resources;

        // Remove any ResourceInclude entries that the XAML seeded
        // (the static <ResourceInclude> in App.axaml) so we don't
        // have duplicate keys.
        for (int i = appResources.MergedDictionaries.Count - 1; i >= 0; i--)
        {
            appResources.MergedDictionaries.RemoveAt(i);
        }

        // Insert our permanent wrapper — this is the only
        // MergedDictionaries operation we ever do.
        appResources.MergedDictionaries.Add(_wrapper);

        // Populate wrapper with dark theme
        ApplyEntries(_darkEntries);
        IsDark = true;
    }

    // ── Public API ────────────────────────────────────────────

    public void SetDark() => Apply(true);
    public void SetLight() => Apply(false);
    public void Toggle() => Apply(!IsDark);

    public void Apply(bool dark)
    {
        if (IsDark == dark) return;

        IsDark = dark;
        ApplyEntries(dark ? _darkEntries : _lightEntries);

        // Keep Fluent built-in controls (scrollbars etc.) in sync
        Application.Current!.RequestedThemeVariant =
            dark ? ThemeVariant.Dark : ThemeVariant.Light;

        ThemeChanged?.Invoke(IsDark);
    }

    // ── Internals ─────────────────────────────────────────────

    /// <summary>
    /// Replaces the wrapper's contents in-place.
    /// Because the wrapper object itself never leaves
    /// MergedDictionaries, Avalonia never loses the owner reference
    /// and the NullReferenceException cannot occur.
    /// </summary>
    private void ApplyEntries(Dictionary<object, object?> entries)
    {
        _wrapper.Clear();

        foreach (var (key, value) in entries)
            _wrapper.Add(key, value);
    }

    /// <summary>
    /// Loads a ResourceDictionary from an avares:// URI and
    /// snapshots its flat key→value pairs into a plain Dictionary.
    /// </summary>
    private static Dictionary<object, object?> LoadEntries(string uri)
    {
        var loaded = AvaloniaXamlLoader.Load(new Uri(uri));

        if (loaded is not ResourceDictionary rd)
            throw new InvalidOperationException(
                $"Expected ResourceDictionary at {uri}, " +
                $"got {loaded?.GetType().Name}");

        var result = new Dictionary<object, object?>();

        foreach (var key in rd.Keys)
            result[key] = rd[key];

        return result;
    }
}