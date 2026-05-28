using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Systems;

public class HumanResourcesSystem
    : ISimulationSystem
{
    public void Update(
        SimulationEngine engine)
    {
        foreach (var employee
                 in engine.Company.Employees)
        {
            if (employee.IsAssigned)
            {
                employee.Morale -= 1;
            }
            else
            {
                employee.Morale += 1;
            }

            employee.Morale =
                Math.Clamp(
                    employee.Morale,
                    0,
                    100);

            if (employee.Morale < 20)
            {
                employee.Productivity -= 1;
            }

            employee.Productivity =
                Math.Clamp(
                    employee.Productivity,
                    0,
                    100);
        }
    }
}
