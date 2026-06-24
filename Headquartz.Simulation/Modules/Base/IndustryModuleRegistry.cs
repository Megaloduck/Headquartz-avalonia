using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Modules.Base;

/// <summary>
/// Global registry for all industry modules and their simulation contexts.
/// Initialize once at app startup via <see cref="Initialize"/>.
/// </summary>
public static class IndustryModuleRegistry
{
    private static readonly Dictionary<IndustryType, IIndustryModule> Modules = new();
    private static readonly Dictionary<IndustryType, IIndustrySimulationContext> Contexts = new();

    public static void Register(IIndustryModule module, IIndustrySimulationContext context)
    {
        Modules[module.IndustryType] = module;
        Contexts[context.IndustryType] = context;
    }

    public static IIndustryModule? GetModule(IndustryType type)
        => Modules.GetValueOrDefault(type);

    public static IIndustrySimulationContext? GetContext(IndustryType type)
        => Contexts.GetValueOrDefault(type);

    public static bool HasModule(IndustryType type)
        => Modules.ContainsKey(type);

    public static IReadOnlyCollection<IIndustryModule> AllModules
        => Modules.Values;

    public static IReadOnlyCollection<IIndustrySimulationContext> AllContexts
        => Contexts.Values;

    /// <summary>
    /// Registers all built-in industry modules and contexts.
    /// Call this once during application startup.
    /// </summary>
    public static void Initialize()
    {
        if (Modules.Count > 0) return;

        var foodCtx = new FoodIndustry.FoodIndustryContext();
        Register(new FoodIndustry.FoodIndustryModule(foodCtx), foodCtx);

        var bevCtx = new BeverageIndustry.BeverageIndustryContext();
        Register(new BeverageIndustry.BeverageIndustryModule(bevCtx), bevCtx);

        var entCtx = new EntertainmentIndustry.EntertainmentIndustryContext();
        Register(new EntertainmentIndustry.EntertainmentIndustryModule(entCtx), entCtx);

        var autoCtx = new AutomotiveIndustry.AutomotiveIndustryContext();
        Register(new AutomotiveIndustry.AutomotiveIndustryModule(autoCtx), autoCtx);

        var fashCtx = new FashionIndustry.FashionIndustryContext();
        Register(new FashionIndustry.FashionIndustryModule(fashCtx), fashCtx);
    }
}
