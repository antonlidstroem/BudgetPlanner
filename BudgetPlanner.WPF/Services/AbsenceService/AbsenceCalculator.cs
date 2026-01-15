using System;
using System.Collections.Generic;
using System.Text;
using BudgetPlanner.DAL.Models;

namespace BudgetPlanner.WPF.Services.AbsenceService
{
    public class AbsenceCalculator
    {
        private const decimal VabIncomeCap = 410_000m;

        public AbsenceResult Calculate(
            Absence absence,
            decimal yearlyIncome,
            double yearlyWorkHours)
        {
            decimal cappedIncome = yearlyIncome;

            if (absence.Type == AbsenceType.Vab && yearlyIncome > VabIncomeCap)
                cappedIncome = VabIncomeCap;

            decimal hourlyWage = cappedIncome / (decimal)yearlyWorkHours;
            decimal deduction = hourlyWage * (decimal)absence.Hours;
            decimal compensation = deduction * 0.8m;

            return new AbsenceResult
            {
                Deduction = deduction,
                Compensation = compensation
            };
        }
    }

}
