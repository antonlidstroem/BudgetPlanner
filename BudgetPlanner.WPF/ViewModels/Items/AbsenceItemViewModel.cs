using System;
using System.Collections.Generic;
using System.Text;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;

namespace BudgetPlanner.WPF.ViewModels.Items
{
    public class AbsenceItemViewModel : ViewModelBase
    {
        public DateTime Date { get; set; }
        public AbsenceType Type { get; set; }
        public double Hours { get; set; }
    }

}
