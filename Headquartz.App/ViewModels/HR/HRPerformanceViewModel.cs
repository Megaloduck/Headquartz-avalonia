using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class PerformanceReviewModel : ObservableObject
{
    public Guid EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public string Department { get; set; } = "";
    public string Role { get; set; } = "";
    public int Morale { get; set; }
    public int Productivity { get; set; }
    public decimal Salary { get; set; }
    public string PerformanceGrade { get; set; } = "";
    public string GradeColor { get; set; } = "";
    public bool IsReviewed { get; set; }
    public string Recommendation { get; set; } = "";
}

public partial class HRPerformanceViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private int _totalEmployees;
    [ObservableProperty] private int _excellentCount;
    [ObservableProperty] private int _goodCount;
    [ObservableProperty] private int _poorCount;
    [ObservableProperty] private int _criticalCount;
    [ObservableProperty] private int _reviewedCount;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _filterGrade = "All";

    public ObservableCollection<string> GradeFilters { get; } =
    [
        "All", "Excellent", "Good", "Poor", "Critical"
    ];

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<PerformanceReviewModel> Reviews { get; } = [];

    public HRPerformanceViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    partial void OnFilterGradeChanged(string value) => Refresh();

    [RelayCommand]
    private void ReviewAll()
    {
        foreach (var review in Reviews)
            review.IsReviewed = true;

        ReviewedCount = Reviews.Count;
        StatusMessage = $"✅ All {Reviews.Count} employees reviewed.";
        Refresh();
    }

    [RelayCommand]
    private void GiveRaise(PerformanceReviewModel review)
    {
        var company = _simulation.Engine.Company;
        const decimal raisePct = 0.10m;
        decimal raiseAmount = review.Salary * raisePct;

        if (company.Cash < raiseAmount * 2)
        {
            StatusMessage = $"❌ Insufficient cash to give raise to {review.Name}.";
            return;
        }

        var emp = company.Employees.FirstOrDefault(e => e.Id == review.EmployeeId);
        if (emp == null) return;

        emp.Salary += raiseAmount;
        emp.Morale = Math.Min(100, emp.Morale + 15);
        emp.Productivity = Math.Min(100, emp.Productivity + 10);
        review.IsReviewed = true;

        StatusMessage = $"✅ Gave {review.Name} a 10% raise. Morale +15.";
        Refresh();
    }

    [RelayCommand]
    private void PutOnPIP(PerformanceReviewModel review)
    {
        var emp = _simulation.Engine.Company.Employees
            .FirstOrDefault(e => e.Id == review.EmployeeId);

        if (emp == null) return;

        emp.Productivity = Math.Min(100, emp.Productivity + 5);
        review.IsReviewed = true;
        review.Recommendation = "On PIP — monitoring required";

        StatusMessage = $"⚠ {review.Name} placed on Performance Improvement Plan.";
        Refresh();
    }

    [RelayCommand]
    private void Terminate(PerformanceReviewModel review)
    {
        var company = _simulation.Engine.Company;
        var emp = company.Employees.FirstOrDefault(e => e.Id == review.EmployeeId);
        if (emp == null) return;

        company.Employees.Remove(emp);
        company.Reputation = Math.Max(0, company.Reputation - 1);

        StatusMessage = $"🔥 {review.Name} terminated. Reputation −1.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;
        TotalEmployees = company.Employees.Count;

        var allReviews = company.Employees.Select(emp =>
        {
            int score = (emp.Morale + emp.Productivity) / 2;

            string grade = score switch
            {
                >= 80 => "Excellent",
                >= 60 => "Good",
                >= 35 => "Poor",
                _ => "Critical",
            };

            string color = grade switch
            {
                "Excellent" => "#10B981",
                "Good" => "#3B82F6",
                "Poor" => "#F59E0B",
                _ => "#EF4444",
            };

            string rec = grade switch
            {
                "Excellent" => "Consider promotion or bonus",
                "Good" => "Performing within expectations",
                "Poor" => "Coaching recommended",
                _ => "Immediate intervention required",
            };

            return new PerformanceReviewModel
            {
                EmployeeId = emp.Id,
                Name = emp.Name,
                Department = emp.Department.ToString(),
                Role = emp.Role.ToString(),
                Morale = emp.Morale,
                Productivity = emp.Productivity,
                Salary = emp.Salary,
                PerformanceGrade = grade,
                GradeColor = color,
                Recommendation = rec,
            };
        }).ToList();

        ExcellentCount = allReviews.Count(r => r.PerformanceGrade == "Excellent");
        GoodCount = allReviews.Count(r => r.PerformanceGrade == "Good");
        PoorCount = allReviews.Count(r => r.PerformanceGrade == "Poor");
        CriticalCount = allReviews.Count(r => r.PerformanceGrade == "Critical");

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total Employees", Value = TotalEmployees.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Excellent", Value = ExcellentCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Good", Value = GoodCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Poor", Value = PoorCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Critical", Value = CriticalCount.ToString() });

        var filtered = FilterGrade == "All"
            ? allReviews
            : allReviews.Where(r => r.PerformanceGrade == FilterGrade).ToList();

        Reviews.Clear();
        foreach (var r in filtered.OrderBy(r => r.PerformanceGrade switch
        {
            "Critical" => 0,
            "Poor" => 1,
            "Good" => 2,
            _ => 3,
        }))
            Reviews.Add(r);
    }
}