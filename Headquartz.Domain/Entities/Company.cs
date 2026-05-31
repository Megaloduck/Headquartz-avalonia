using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Domain.Entities;

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Cash { get; set; }
    public int Reputation { get; set; }

    public List<Employee> Employees { get; set; } = [];
    public List<InventoryItem> Inventory { get; set; } = [];
    public List<SalesOrder> Orders { get; set; } = [];
    public List<Department> Departments { get; set; } = [];
    public List<CompanyTask> Tasks { get; set; } = [];
    public List<CompanyEvent> Events { get; set; } = [];
    public List<LoanRecord> Loans { get; set; } = [];

    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit => Revenue - Expenses;
}