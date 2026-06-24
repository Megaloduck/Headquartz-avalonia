using Headquartz.Domain.Enums;
using Headquartz.Simulation.Ticks;

namespace Headquartz.Simulation.Modules.Base;

/// <summary>
/// Lightweight module contract used by the setup UI.
/// Simulation behavior is provided by <see cref="IIndustrySimulationContext"/>.
/// </summary>
public interface IIndustryModule
{
    IndustryType IndustryType { get; }
    string IndustryName { get; }
    string IndustryDescription { get; }
    string IndustryEmoji { get; }

    /// <summary>
    /// Returns the simulation profile (cash multipliers, department deltas, etc.)
    /// </summary>
    IndustryProfile GetProfile();

    /// <summary>
    /// Maps each department to its relevance level for this industry.
    /// Used in the setup UI to preview department fit.
    /// </summary>
    IReadOnlyDictionary<DepartmentType, DepartmentRelevance> GetDepartmentRelevances();
}
