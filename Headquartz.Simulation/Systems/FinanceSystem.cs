using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;

namespace Headquartz.Simulation.Systems;

public class FinanceSystem
    : ISimulationSystem
{
    public void Update(
        SimulationEngine engine)
    {
        Company company =
            engine.Company;

        decimal operationalCosts =
            company.Departments.Sum(
                d => d.Budget * 0.01m);

        company.Cash -= operationalCosts;

        company.Expenses += operationalCosts;

        if (company.Cash < 0)
        {
            company.Reputation -= 1;
        }
    }
}