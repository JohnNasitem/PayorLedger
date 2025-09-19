//***********************************************************************************
//Program: RowCommand.cs
//Description: Base class for row commands
//Date: Sep 18, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.ViewModels;
using PayorLedger.Services.Database;

namespace PayorLedger.Services.Actions.RowCommands
{
    public abstract class RowCommand : IUndoableCommand
    {
        protected readonly RowEntry _row;
        protected static readonly MainPageViewModel _mainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();



        public RowCommand(RowEntry row)
        {
            _row = row;
        }



        /// <summary>
        /// Add row to main list
        /// </summary>
        protected void AddRow()
        {
            _row.State = ChangeState.Added;

            // Add row to main list if it doesnt exist
            if (_mainPageVM.LedgerRows.Any(r => r.OrNum == _row.OrNum))
                _mainPageVM.LedgerRows.Add(_row);

            _mainPageVM.UpdateUI();
        }



        /// <summary>
        /// Mark row as removed
        /// </summary>
        protected void DeleteRow()
        {
            _row.State = ChangeState.Removed;

            _mainPageVM.UpdateUI();
        }




        /// <summary>
        /// Edit row
        /// </summary>
        /// <param name="newDate">New date of row</param>
        /// <param name="newComment">New comment of row</param>
        protected void EditRow(string newDate, string newComment)
        {
            _row.State = ChangeState.Edited;
            _row.Date = newDate;
            _row.Comment = newComment;
        }



        // IUndoableCommand methods
        public abstract void Execute();
        public abstract void Undo();
    }
}
