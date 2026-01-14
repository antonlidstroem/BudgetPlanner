using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BudgetPlanner.DAL.Models;

namespace BudgetPlanner.DAL.Data
{
    public class BudgetDbContext : DbContext
    {
        public BudgetDbContext() { }

        public DbSet<BudgetTransaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Absence> Absences { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = config.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Mat", Type = TransactionType.Expense },
                new Category { Id = 2, Name = "Hus & drift", Type = TransactionType.Expense },
                new Category { Id = 3, Name = "Transport", Type = TransactionType.Expense },
                new Category { Id = 4, Name = "Fritid", Type = TransactionType.Expense },
                new Category { Id = 5, Name = "Barn", Type = TransactionType.Expense },
                new Category { Id = 6, Name = "Streaming-tjänster", Type = TransactionType.Expense },
                new Category { Id = 7, Name = "SaaS-produkter", Type = TransactionType.Expense },
                new Category { Id = 8, Name = "Lön", Type = TransactionType.Income },
                new Category { Id = 9, Name = "Bidrag", Type = TransactionType.Income },
                new Category { Id = 10, Name = "Hobbyverksamhet", Type = TransactionType.Income }
            );

            modelBuilder.Entity<Absence>()
        .HasKey(a => a.Date);

        }

    }
}
