using System;
using System.Collections.Generic;
using System.Text;
using BudgetPlanner.DAL.Models;

namespace BudgetPlanner.DAL.Interfaces
{
    public interface IBudgetTransactionRepository
    {
        Task<List<BudgetTransaction>> GetAllAsync();
        Task AddAsync(BudgetTransaction transaction);
        Task DeleteAsync(BudgetTransaction transaction);
        Task UpdateAsync(BudgetTransaction transaction);
        Task<List<Category>> GetCategoriesAsync();

    }

}
