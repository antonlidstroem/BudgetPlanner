using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetPlanner.DAL.Models
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    public enum AdjustmentType
    {
        Deduction,
        Addition
    }


    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TransactionType Type { get; set; }
        public bool ToggleGrossNet { get; set; } = false;
        public int? DefaultRate { get; set; }
        public AdjustmentType? AdjustmentType { get; set; }
        public string? Description { get; set; }
        public bool HasEndDate { get; set; } = false;
        public Category() { }

    }
}
