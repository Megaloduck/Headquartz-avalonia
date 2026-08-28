using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Domain.Entities;

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public IndustryType Industry { get; set; }
    public decimal Cash { get; set; }
    public int Reputation { get; set; }

    public List<Employee> Employees { get; set; } = [];
    public List<InventoryItem> Inventory { get; set; } = [];
    public List<SalesOrder> Orders { get; set; } = [];
    public List<Department> Departments { get; set; } = [];
    public List<CompanyTask> Tasks { get; set; } = [];
    public List<CompanyEvent> Events { get; set; } = [];
    public List<LoanRecord> Loans { get; set; } = [];

    /// <summary>
    /// Case-by-case funding requests raised by departments that have run
    /// past their own Budget. Reviewed by Finance via
    /// ReviewBudgetRequestCommand — nothing here moves money until then.
    /// </summary>
    public List<BudgetRequest> BudgetRequests { get; set; } = [];

    /// <summary>
    /// Headcount asks raised by any department — reacting to a shortfall
    /// or planning ahead. HR fulfills or declines each one via
    /// FulfillWorkforceRequestCommand / DeclineWorkforceRequestCommand.
    /// </summary>
    public List<WorkforceRequest> WorkforceRequests { get; set; } = [];

    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit => Revenue - Expenses;
}