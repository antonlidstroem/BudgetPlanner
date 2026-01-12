using System;
using System.Collections.Generic;
using System.Text;
using BudgetPlanner.DAL.Data;
using BudgetPlanner.DAL.Interfaces;
using BudgetPlanner.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.DAL.Repositories
{
    public class BudgetTransactionRepository : IBudgetTransactionRepository
    {
        private readonly BudgetDbContext context;

        public BudgetTransactionRepository(BudgetDbContext context)
        {
            this.context = context;
        }

        public async Task<List<BudgetTransaction>> GetAllAsync()
        {
            return await context.Transactions
                .Include(t => t.Category)
                .ToListAsync();
        }

        public async Task AddAsync(BudgetTransaction transaction)
        {
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(BudgetTransaction transaction)
        {
            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
        }
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await context.Categories.ToListAsync();
        }

    }

}
