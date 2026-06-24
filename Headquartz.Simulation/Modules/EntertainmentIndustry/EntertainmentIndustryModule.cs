using Headquartz.Domain.Enums;
using Headquartz.Simulation.Modules.Base;
using Headquartz.Simulation.Ticks;

namespace Headquartz.Simulation.Modules.EntertainmentIndustry;

public class EntertainmentIndustryModule : IIndustryModule
{
    private readonly EntertainmentIndustryContext _ctx;
    public EntertainmentIndustryModule(EntertainmentIndustryContext ctx) => _ctx = ctx;

    public IndustryType IndustryType => _ctx.IndustryType;
    public string IndustryName => _ctx.IndustryName;
    public string IndustryDescription => _ctx.IndustryDescription;
    public string IndustryEmoji => _ctx.IndustryEmoji;

    public IndustryProfile GetProfile() => _ctx.GetProfile();
    public IReadOnlyDictionary<DepartmentType, DepartmentRelevance> GetDepartmentRelevances() => _ctx.GetDepartmentRelevances();
}
