using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Domain.Enums;

/// <summary>
/// Founding-phase gate. PreOpening = players are hiring the first
/// employee, landing the first client, building the first production
/// line — the catalog is intentionally empty and nothing should be
/// auto-restocking, auto-depleting, or accruing understaffed-stress yet.
/// GrandOpening = normal operations; all systems run unconstrained.
/// Transition is driven by the "grand-opening" CompanyAgenda calendar
/// event ending — see SimulationEngine.Update().
/// </summary>
public enum CompanyPhase
{
    PreOpening,
    GrandOpening,
}