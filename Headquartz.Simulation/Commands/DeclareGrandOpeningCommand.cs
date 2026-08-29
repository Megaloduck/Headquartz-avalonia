using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Phase;
using Headquartz.Simulation.Systems;

namespace Headquartz.Simulation.Commands;

/// <summary>
/// The team's collective call that founding is done and the real clock
/// starts. Flips Company.Phase to GrandOpening, which switches on the
/// ambient simulation — random events, cascading stress, auto-generated
/// tasks/orders (see the phase gate in SimulationEngine.Update()) — and
/// fires the "grand-opening" calendar definition's department effects
/// for real, at whatever moment this is actually declared rather than a
/// fixed calendar date.
///
/// Deliberately carries no readiness requirement. A team can declare
/// Grand Opening with an empty warehouse and zero hires if they want
/// to — that's a real decision with real consequences, not something
/// the game should block. It's just usually a bad idea.
/// </summary>
public class DeclareGrandOpeningCommand
    : ICompanyCommand
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public string PlayerId { get; set; } =
        "";

    public bool Validate(
        SimulationEngine engine)
    {
        // Idempotency guard only — no minimum-viable-company check.
        return engine.Company.Phase == CompanyPhase.PreOpening;
    }

    public void Execute(
        SimulationEngine engine)
    {
        engine.Company.Phase = CompanyPhase.GrandOpening;

        engine.CalendarEvents.TriggerManualEvent(engine, "grand-opening");

        engine.Events.Publish(
            new CompanyEnteredGrandOpeningEvent());
    }
}