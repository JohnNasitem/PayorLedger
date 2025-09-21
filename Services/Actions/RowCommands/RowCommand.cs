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
            if (!_mainPageVM.LedgerRows.Any(r => r.OrNum == _row.OrNum))
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
        /// <param name="label">New label of row</param>
        /// <param name="newPayorId">New payor id of row</param>
        /// <param name="newOrNum">New Or # of row</param>
        /// <param name="newDate">New date of row</param>
        /// <param name="newComment">New comment of row</param>
        protected void EditRow(RowEntry.RowLabel label, long newPayorId, int newOrNum, string newDate, string newComment)
        {
            _row.State = ChangeState.Edited;
            _row.Label = label;
            _row.PayorId = newPayorId;
            _row.OrNum = newOrNum;
            _row.Date = newDate;
            _row.Comment = newComment;

            _mainPageVM.UpdateUI();
        }



        // IUndoableCommand methods
        public abstract void Execute();
        public abstract void Undo();
    }
}
