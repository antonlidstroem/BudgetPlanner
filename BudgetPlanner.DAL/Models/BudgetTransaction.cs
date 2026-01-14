using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetPlanner.DAL.Models
{
    public enum Recurrence
    {
        OneTime,
        Monthly,
        Yearly
    }

    public class BudgetTransaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; } //NETTO
        public decimal GrossAmount { get; set; } //BRUTTO

        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public Recurrence Recurrence { get; set; }
        public bool IsActive { get; set; } = true;
        public int? Month { get; set; }
        public decimal? TaxRate { get; set; } = 30;
        public bool? IsGrossIncome { get; set; } = true;



        public TransactionType Type => Category?.Type ?? TransactionType.Expense;

    }
}
