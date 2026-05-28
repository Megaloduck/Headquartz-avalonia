using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.ViewModels;

public class CompanyViewModel
    : CompanyDashboardViewModel
{
    public CompanyViewModel(
        Services.SimulationService simulation)
        : base(simulation)
    {
    }
}