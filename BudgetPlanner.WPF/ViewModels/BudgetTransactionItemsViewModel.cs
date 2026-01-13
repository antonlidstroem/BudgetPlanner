using System;
using System.Collections.Generic;
using System.Text;
using BudgetPlanner.DAL.Models;

namespace BudgetPlanner.WPF.ViewModels
{
    public class BudgetTransactionItemsViewModel : ViewModelBase
    {
        private readonly BudgetTransaction model;
        public BudgetTransaction Model => model;

        public BudgetTransactionItemsViewModel(BudgetTransaction model)
        {
            this.model = model;
        }


        public DateTime Date
        {
            get { return model.Date; }
            set { model.Date = value;
                RaisePropertyChanged();
            } 
        }
        public Decimal Amount
        {
            get { return model.Amount; }
            set
            {
                model.Amount = value;
                RaisePropertyChanged();
            }
        }

        public string? Description
        {
            get { return model.Description; }
            set
            {
                model.Description = value;
                RaisePropertyChanged();
            }
        }

        public int CategoryId
        {
            get { return model.CategoryId; }
            set
            {
                model.CategoryId = value;
                RaisePropertyChanged();
            }
        }

        public int? Month
        {
            get { return model.Month; }
            set
            {
                model.Month = value;
                RaisePropertyChanged();
            }
        }

        public Category? Category
        {
            get { return model.Category; }
            set
            {
                model.Category = value;
                RaisePropertyChanged();
            }
        }

        public Recurrence Recurrence
        {
            get { return model.Recurrence; }
            set
            {
                model.Recurrence = value;
                RaisePropertyChanged();
            }
        }

        public bool IsActive
        {
            get { return model.IsActive; }
            set
            {
                model.IsActive = value;
                RaisePropertyChanged();
            }
        }

        public TransactionType Type
        {
            get { return model.Type; }
        }

        public void RefreshFromModel()
        {
            RaisePropertyChanged(nameof(Date));
            RaisePropertyChanged(nameof(Amount));
            RaisePropertyChanged(nameof(Category));
            RaisePropertyChanged(nameof(Recurrence));
            RaisePropertyChanged(nameof(Description));
        }
    }
}
