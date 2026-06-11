using Headquartz.Domain.Enums;

namespace Headquartz.Simulation;

/// <summary>
/// Immutable configuration that controls how the simulation
/// behaves for a given difficulty. Created once during onboarding
/// and passed into SimulationEngine at startup.
/// </summary>
public sealed record SimulationProfile
{
    // ── Tick timing ───────────────────────────────────────────

    /// <summary>Real-world milliseconds between simulation ticks.</summary>
    public int TickDelayMs { get; init; } = 8_000;

    // ── Event system ──────────────────────────────────────────

    /// <summary>
    /// Probability (0–1) that a random company event fires each tick.
    /// </summary>
    public double EventFrequency { get; init; } = 0.15;

    /// <summary>
    /// Shifts severity rolls toward higher values.
    /// 0 = balanced distribution, 1 = all Critical.
    /// </summary>
    public double SeverityBias { get; init; } = 0.0;

    // ── Cascade system ────────────────────────────────────────

    /// <summary>
    /// Multiplier applied to all stress propagation amounts
    /// in CascadeSystem. 1.0 = default behaviour.
    /// </summary>
    public double CascadeMultiplier { get; init; } = 1.0;

    // ── Starting capital ──────────────────────────────────────

    /// <summary>Company cash at simulation start.</summary>
    public decimal InitialCapital { get; init; } = 100_000m;

    // =========================================================
    // PRESETS
    // =========================================================

    public static SimulationProfile Trainee => new()
    {
        TickDelayMs = 8_000,
        EventFrequency = 0.08,
        SeverityBias = -0.3,   // skews toward Low / Medium
        CascadeMultiplier = 0.5,
        InitialCapital = 150_000m,
    };

    public static SimulationProfile Manager => new()
    {
        TickDelayMs = 8_000,
        EventFrequency = 0.15,
        SeverityBias = 0.0,    // balanced
        CascadeMultiplier = 1.0,
        InitialCapital = 100_000m,
    };

    public static SimulationProfile Director => new()
    {
        TickDelayMs = 8_000,
        EventFrequency = 0.25,
        SeverityBias = 0.3,    // skews toward High / Critical
        CascadeMultiplier = 1.5,
        InitialCapital = 60_000m,
    };

    public static SimulationProfile Chairman => new()
    {
        TickDelayMs = 8_000,
        EventFrequency = 0.40,
        SeverityBias = 0.6,    // mostly Critical
        CascadeMultiplier = 2.5,
        InitialCapital = 30_000m,
    };

    // =========================================================
    // FACTORY
    // =========================================================

    /// <summary>
    /// Creates the appropriate preset from a GameDifficulty value.
    /// </summary>
    public static SimulationProfile FromDifficulty(GameDifficulty difficulty) =>
        difficulty switch
        {
            GameDifficulty.Trainee => Trainee,
            GameDifficulty.Manager => Manager,
            GameDifficulty.Director => Director,
            GameDifficulty.Chairman => Chairman,
            _ => Manager,
        };
}