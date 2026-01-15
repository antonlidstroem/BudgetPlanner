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
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal NetAmount { get; set; } 
        public decimal? GrossAmount { get; set; } 
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public Recurrence Recurrence { get; set; }
        public bool IsActive { get; set; } = true;
        public int? Month { get; set; }
        public decimal? Rate { get; set; }
        //public TransactionType Type => Category?.Type ?? TransactionType.Expense;
        public TransactionType Type { get; set; }

        public AdjustmentType RateAdjustmentType => Category?.AdjustmentType ?? AdjustmentType.Deduction;
    }
}
