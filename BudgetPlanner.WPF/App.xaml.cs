using System.Windows;
using BudgetPlanner.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Migrera databasen direkt, BudgetDbContext läser själv appsettings.json
            using var db = new BudgetDbContext();
            db.Database.Migrate();
        }
    }
}
