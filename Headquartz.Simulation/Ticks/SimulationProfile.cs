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

    /// <summary>Real-world milliseconds between simulation ticks. Always 5 000 ms (5 s).</summary>
    public int TickDelayMs { get; init; } = 5_000;

    // ── Time hierarchy ────────────────────────────────────────

    /// <summary>
    /// How many simulation ticks make up one in-game work hour.
    /// Drives both the tick-bar segment count and the work-hours fill rate.
    ///
    ///   Trainee  → 4 ticks / work hour
    ///   Manager  → 5 ticks / work hour
    ///   Director → 6 ticks / work hour
    ///   Chairman → 7 ticks / work hour
    ///
    /// Work hours per day is always 8.
    /// </summary>
    public int TicksPerWorkHour { get; init; } = 5;

    /// <summary>In-game work hours per day. Always 8.</summary>
    public int WorkHoursPerDay { get; init; } = 8;

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

    /// <summary>
    /// Multiplier applied to event duration in ticks.
    /// Higher difficulties make events last longer (more punishing).
    ///   Trainee  → 0.75 (events resolve faster)
    ///   Manager  → 1.00 (baseline)
    ///   Director → 1.50 (events last 50% longer)
    ///   Chairman → 2.00 (events last 2× as long)
    /// </summary>
    public double EventDurationMultiplier { get; init; } = 1.0;

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
        TickDelayMs = 5_000,
        TicksPerWorkHour = 4,
        EventFrequency = 0.08,
        SeverityBias = -0.3,
        CascadeMultiplier = 0.5,
        EventDurationMultiplier = 0.75,
        InitialCapital = 150_000m,
    };

    public static SimulationProfile Manager => new()
    {
        TickDelayMs = 5_000,
        TicksPerWorkHour = 5,
        EventFrequency = 0.15,
        SeverityBias = 0.0,
        CascadeMultiplier = 1.0,
        EventDurationMultiplier = 1.0,
        InitialCapital = 100_000m,
    };

    public static SimulationProfile Director => new()
    {
        TickDelayMs = 5_000,
        TicksPerWorkHour = 6,
        EventFrequency = 0.25,
        SeverityBias = 0.3,
        CascadeMultiplier = 1.5,
        EventDurationMultiplier = 1.5,
        InitialCapital = 60_000m,
    };

    public static SimulationProfile Chairman => new()
    {
        TickDelayMs = 5_000,
        TicksPerWorkHour = 7,
        EventFrequency = 0.40,
        SeverityBias = 0.6,
        CascadeMultiplier = 2.5,
        EventDurationMultiplier = 2.0,
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