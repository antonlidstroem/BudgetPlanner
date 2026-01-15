using System;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.Services.AbsenceService;
using BudgetPlanner.WPF.ViewModels.Base;

namespace BudgetPlanner.WPF.ViewModels.Summaries
{
    public class MonthlyForecastViewModel : ViewModelBase
    {
        private decimal _annualIncome;
        private decimal _annualWorkHours;

        private readonly AbsenceCalculator _calculator = new();

        // Samlingar för frånvaro
        public ObservableCollection<Absence> Absences { get; } = new();
        public ObservableCollection<AbsenceResult> AbsenceResults { get; } = new();

        // Årsinkomst
        public decimal AnnualIncome
        {
            get => _annualIncome;
            set
            {
                _annualIncome = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(MonthlyIncome));
                RaisePropertyChanged(nameof(HourlyRate));
                RecalculateAbsence(); // Uppdatera frånvaro när inkomsten ändras
            }
        }

        // Årsarbetstid (timmar)
        public decimal AnnualWorkHours
        {
            get => _annualWorkHours;
            set
            {
                _annualWorkHours = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(MonthlyIncome));
                RaisePropertyChanged(nameof(HourlyRate));
                RecalculateAbsence(); // Uppdatera frånvaro när arbetstiden ändras
            }
        }

        // Timlön
        public decimal HourlyRate =>
            AnnualWorkHours == 0 ? 0 : AnnualIncome / AnnualWorkHours;

        // Månadslön
        public decimal MonthlyIncome => AnnualIncome / 12m;

        // Totalt avdrag pga frånvaro
        public decimal AbsenceDeduction => AbsenceResults.Sum(r => r.Deduction);

        // Ersättning (80% av avdraget)
        public decimal AbsenceCompensation => AbsenceResults.Sum(r => r.Compensation);

        // Nettoeffekt av frånvaro
        public decimal AbsenceNetEffect => AbsenceCompensation - AbsenceDeduction;

        // Månadsprognos inklusive frånvaro
        public decimal MonthlyForecast => MonthlyIncome + AbsenceNetEffect;

        // Lägg till eller uppdatera frånvaror
        private void RecalculateAbsence()
        {
            AbsenceResults.Clear();

            foreach (var absence in Absences)
            {
                AbsenceResults.Add(
                    _calculator.Calculate(absence, AnnualIncome, (double)AnnualWorkHours)
                );
            }

            // Uppdatera bindningar
            RaisePropertyChanged(nameof(AbsenceDeduction));
            RaisePropertyChanged(nameof(AbsenceCompensation));
            RaisePropertyChanged(nameof(AbsenceNetEffect));
            RaisePropertyChanged(nameof(MonthlyForecast));
        }

        // Hjälpmetod för att lägga till frånvaro
        public void AddAbsence(Absence absence)
        {
            Absences.Add(absence);
            RecalculateAbsence();
        }

        // Hjälpmetod för att ta bort frånvaro
        public void RemoveAbsence(Absence absence)
        {
            Absences.Remove(absence);
            RecalculateAbsence();
        }
    }
}
