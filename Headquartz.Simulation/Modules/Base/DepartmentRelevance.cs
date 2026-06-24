using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Modules.Base;

/// <summary>
/// Defines how critical a department is for a given industry.
/// Used in CompanySetupView to preview department fit.
/// </summary>
public enum DepartmentRelevance
{
    /// <summary>Not applicable or fully outsourced in this industry.</summary>
    Outsourced,

    /// <summary>Minimal operational footprint.</summary>
    Light,

    /// <summary>Standard operational requirement.</summary>
    Standard,

    /// <summary>Important for success; industry-specific optimizations apply.</summary>
    Important,

    /// <summary>Core to the business model; heavily emphasized.</summary>
    Critical,
}
