//***********************************************************************************
//Program: CellCommand.cs
//Description: Base class for cell commands
//Date: Sep 15, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.Services.Database;
using PayorLedger.ViewModels;

namespace PayorLedger.Services.Actions.CellCommands
{
    public abstract class CellCommand : IUndoableCommand
    {
        /// <summary>
        /// Cell entry being manipulated
        /// </summary>
        public CellEntryToRow Cell { get; }



        protected static readonly MainPageViewModel _mainPageViewModel = App.ServiceProvider.GetRequiredService<MainPageViewModel>();
        protected static readonly PayorWindowViewModel _payorWindowViewModel = App.ServiceProvider.GetRequiredService<PayorWindowViewModel>();



        public CellCommand(CellEntryToRow cell)
        {
            Cell = cell;
        }



        /// <summary>
        /// Add a cell to the main page
        /// </summary>
        protected void AddCell()
        {
            Cell.State = ChangeState.Added;
            RowEntry row = _mainPageViewModel.LedgerRows.Find(r => r.OrNum == Cell.Row.OrNum)!;

            // Add it if it doesnt exist already
            if (!row.CellEntries.Any(e => e.SubheaderId == Cell.SubheaderId))
                row.CellEntries.Add(Cell);

            UpdateTables();
        }



        /// <summary>
        /// Mark cell as deleted
        /// </summary>
        protected void DeleteCell()
        {
            Cell.State = ChangeState.Removed;

            UpdateTables();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="newAmount"></param>
        protected void EditCell(decimal newAmount)
        {
            Cell.Amount = newAmount;
            Cell.State = ChangeState.Edited;

            UpdateTables();
        }



        /// <summary>
        /// Call other view models to update their tables
        /// </summary>
        private void UpdateTables()
        {
            _mainPageViewModel.UpdateUI();

            if (_payorWindowViewModel.ActiveViewPayorViewModel != null && _payorWindowViewModel.ActiveViewPayorViewModel.Payor.PayorId == Cell.Row.PayorId)
                _payorWindowViewModel.ActiveViewPayorViewModel.UpdateTable();
        }



        // IUndoableCommand methods
        public abstract void Execute();
        public abstract void Undo();
    }
}
