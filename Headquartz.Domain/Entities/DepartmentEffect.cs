using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Domain.Entities;

/// <summary>
/// Describes a single effect that a calendar event applies to one department.
/// Magnitude is interpreted based on the EffectType:
///   - Multipliers: 1.0 = no change, 1.2 = +20%, 0.8 = -20%
///   - Modifiers:   flat value (e.g., +5000 cash, -10 morale)
///   - Toggles:     1.0 = enable, 0.0 = disable
/// </summary>
public class DepartmentEffect
{
    public DepartmentType Department { get; set; }

    public EventEffectType EffectType { get; set; }

    /// <summary>
    /// Numeric parameter whose meaning depends on <see cref="EffectType"/>.
    /// For multipliers use 1.0 baseline; for flat modifiers use raw values.
    /// </summary>
    public double Magnitude { get; set; }

    /// <summary>
    /// Human-readable description shown in the forecast UI (e.g., "Marketing +10%").
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// How many ticks the effect persists after the event starts.
    /// One tick = 15 minutes. 96 ticks = 1 day.
    /// </summary>
    public int DurationTicks { get; set; } = 96;
}
