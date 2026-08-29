namespace Headquartz.Domain.Enums;

/// <summary>
/// Where the company is in its lifecycle. PreOpening is the founding
/// window — departments exist but start unfunded and unstaffed, and the
/// ambient simulation (random events, cascading stress, auto-generated
/// tasks/orders) is dormant so a team can hire, allocate budget, and
/// stock up without being punished for not being staffed yet.
/// GrandOpening switches all of that on for real.
///
/// More stages can be appended here later (e.g. an end-of-run state)
/// without touching what's already defined.
/// </summary>
public enum CompanyPhase
{
    PreOpening,
    GrandOpening,
}