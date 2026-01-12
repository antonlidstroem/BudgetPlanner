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

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TransactionType Type { get; set; }

        public Category() { }

    }
}
