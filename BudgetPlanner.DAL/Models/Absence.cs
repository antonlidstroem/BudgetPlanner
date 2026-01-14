using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetPlanner.DAL.Models
{
    public enum AbsenceType
    {
        Sick,
        Vab
    }

    public class Absence
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public AbsenceType Type { get; set; }
        public double Hours { get; set; } 
    }

}
