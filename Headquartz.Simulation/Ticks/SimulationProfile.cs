using Headquartz.Domain.Enums;

public sealed record SimulationProfile
{
    public int TickDelayMs { get; init; } = 5_000;
    public int TicksPerWorkHour { get; init; } = 5;
    public int WorkHoursPerDay { get; init; } = 8;
    public double EventFrequency { get; init; } = 0.15;
    public double SeverityBias { get; init; } = 0.0;
    public double EventDurationMultiplier { get; init; } = 1.0;
    public double CascadeMultiplier { get; init; } = 1.0;
    public decimal InitialCapital { get; init; } = 100_000m;

    /// <summary>Which difficulty preset produced this profile — needed so the
    /// UI can display "Department + Difficulty" after onboarding is over.</summary>
    public GameDifficulty Difficulty { get; init; } = GameDifficulty.Manager;

    public static SimulationProfile Trainee => new()
    {
        TickDelayMs = 5_000,
        TicksPerWorkHour = 4,
        EventFrequency = 0.08,
        SeverityBias = -0.3,
        CascadeMultiplier = 0.5,
        EventDurationMultiplier = 0.75,
        InitialCapital = 150_000m,
        Difficulty = GameDifficulty.Trainee,
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
        Difficulty = GameDifficulty.Manager,
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
        Difficulty = GameDifficulty.Director,
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
        Difficulty = GameDifficulty.Chairman,
    };

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