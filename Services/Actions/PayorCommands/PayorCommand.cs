//***********************************************************************************
//Program: PayorCommand.cs
//Description: Base class for payor commands
//Date: Sep 9, 2025
//Author: John Nasitem
//***********************************************************************************


using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.Services.Database;
using PayorLedger.ViewModels;

namespace PayorLedger.Services.Actions.PayorCommands
{
    public abstract class PayorCommand : IUndoableCommand
    {
        protected readonly PayorEntry _payor;
        protected List<CellEntryToRow> _entries;
        protected static readonly MainPageViewModel _mainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();
        protected static readonly ManagePayorsViewModel _managePayorsVM = App.ServiceProvider.GetRequiredService<ManagePayorsViewModel>();



        public PayorCommand(PayorEntry payor)
        {
            _payor = payor;
            _entries = [];
        }



        /// <summary>
        /// Add a payor and all its associated entries (if any exist) to the main page
        /// </summary>
        protected void AddPayor()
        {
            _payor.State = ChangeState.Added;

            // Add the payor to the payor list if it doesnt exist
            if (!_mainPageVM.Payors.Any(p => p.PayorId == _payor.PayorId))
                _mainPageVM.Payors.Add(_payor);

            UpdateUI();
        }



        /// <summary>
        /// Mark payor as deleted
        /// </summary>
        protected void DeletePayor()
        {
            _payor.State = ChangeState.Removed;

            UpdateUI();
        }



        /// <summary>
        /// Edit a payor's name and label
        /// </summary>
        /// <param name="newName">New name</param>
        protected void EditPayor(string newName)
        {
            _payor.PayorName = newName;
            _payor.State = ChangeState.Edited;

            UpdateUI();
        }



        /// <summary>
        /// Updates UI in other pages
        /// </summary>
        protected void UpdateUI()
        {
            _mainPageVM.UpdateUI();
            _managePayorsVM.UpdateUI();
            _managePayorsVM.Page.SelectPayor(0);
        }



        // IUndoableCommand methods
        public abstract void Execute();
        public abstract void Undo();
    }
}
