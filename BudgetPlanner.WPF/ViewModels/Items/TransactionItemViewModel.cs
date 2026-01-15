using System;
using System.Collections.Generic;
using System.Text;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;

namespace BudgetPlanner.WPF.ViewModels.Items
{
    public class TransactionItemViewModel : ViewModelBase
    {
        private readonly BudgetTransaction model;
        public BudgetTransaction Model => model;

        public TransactionItemViewModel(BudgetTransaction model)
        {
            this.model = model;
        }


        public DateTime StartDate
        {
            get { return model.StartDate; }
            set { model.StartDate = value;
                RaisePropertyChanged();
            } 
        }
        public DateTime? EndDate
        {
            get { return model.EndDate; }
            set
            {
                model.EndDate = value;
                RaisePropertyChanged();
            }
        }

        public decimal NetAmount 
        {
            get { return model.NetAmount; }
            set
            {
                model.NetAmount = value;
                RaisePropertyChanged();
            }
        }

        public decimal? GrossAmount
        {
            get => model.GrossAmount;
            set
            {
                model.GrossAmount = value;
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

        public decimal? Rate 
        {
            get { return model.Rate; }
            set
            {
                model.Rate = value;
                RaisePropertyChanged();
            }
        }




        public Category? Category
        {
            get { return model.Category; }
            set
            {
                model.Category = value;
                model.CategoryId = value?.Id ?? 0;
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
            set
            {
                if (model.Type != value)
                {
                    model.Type = value;
                    RaisePropertyChanged();
                }
            }
        }

      
            public void RefreshFromModel()
        {
            RaisePropertyChanged(nameof(StartDate));
            RaisePropertyChanged(nameof(EndDate));
            RaisePropertyChanged(nameof(NetAmount));
            RaisePropertyChanged(nameof(GrossAmount));
            RaisePropertyChanged(nameof(Category));
            RaisePropertyChanged(nameof(Recurrence));
            RaisePropertyChanged(nameof(Description));
            RaisePropertyChanged(nameof(Month));
            RaisePropertyChanged(nameof(Type));
            RaisePropertyChanged(nameof(Rate));
            RaisePropertyChanged(nameof(IsActive));
            RaisePropertyChanged(nameof(Month));
        }

        public void RefreshProperties()
        {
            RaisePropertyChanged(nameof(NetAmount));
            RaisePropertyChanged(nameof(GrossAmount));
        }

    }
}

