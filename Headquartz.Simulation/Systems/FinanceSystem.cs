using System;
using System.Collections.Generic;
using System.Text;

using System;
using System.Linq;

using Headquartz.Domain.Entities;
using Headquartz.Simulation.Events;

namespace Headquartz.Simulation.Systems;

public class FinanceSystem : ISimulationSystem
{
    private const decimal CashCrisisThreshold = -50_000m;
    private const decimal BudgetCutRate = 0.97m;
    private const int ReputationHitOnNegative = 1;
    private const int ReputationHitOnCrisis = 3;

    public void Update(SimulationEngine engine)
    {
        ApplyOperationalCosts(engine);
        ProcessLoanInterest(engine);
        CheckCashCrisis(engine);
    }

    // =========================================================
    // OPERATIONAL COSTS
    // =========================================================

    private static void ApplyOperationalCosts(SimulationEngine engine)
    {
        Company company = engine.Company;

        decimal operationalCosts = company.Departments.Sum(d =>
        {
            decimal efficiencyFactor =
                1.0m + (100 - d.Efficiency) / 500m;
            return d.Budget * 0.01m * efficiencyFactor;
        });

        company.Cash -= operationalCosts;
        company.Expenses += operationalCosts;

        if (company.Cash < 0)
            company.Reputation =
                Math.Max(0, company.Reputation - ReputationHitOnNegative);
    }

    // =========================================================
    // LOAN INTEREST — every payroll cycle (every 10 ticks)
    // =========================================================

    private static void ProcessLoanInterest(SimulationEngine engine)
    {
        if (engine.Clock.Tick % 10 != 0) return;

        foreach (var loan in engine.Company.Loans
                     .Where(l => !l.IsRepaid))
        {
            decimal interest = Math.Round(
                loan.TotalOwed * loan.InterestRate, 2);

            loan.TotalOwed += interest;
            loan.InterestPaid += interest;
            engine.Company.Cash -= interest;
            engine.Company.Expenses += interest;
        }
    }

    // =========================================================
    // CASH CRISIS
    // =========================================================

    private static void CheckCashCrisis(SimulationEngine engine)
    {
        Company company = engine.Company;

        if (company.Cash >= CashCrisisThreshold) return;

        engine.Events.Publish(
            new CashCrisisEvent { CashBalance = company.Cash });

        company.Reputation =
            Math.Max(0, company.Reputation - ReputationHitOnCrisis);

        foreach (var dept in company.Departments)
        {
            dept.Budget = Math.Max(1_000m, dept.Budget * BudgetCutRate);
            dept.Efficiency = Math.Max(10, dept.Efficiency - 3);
            dept.StressLevel = Math.Min(100, dept.StressLevel + 5);
        }
    }
}