//***********************************************************************************
//Program: SubheaderCommands.cs
//Description: Base class for subheader commands
//Date: Sep 9, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Database;
using PayorLedger.ViewModels;

namespace PayorLedger.Services.Actions.HeaderCommands
{
    public abstract class HeaderCommand : IUndoableCommand
    {
        /// <summary>
        /// Header entry being manipulated
        /// </summary>
        public HeaderEntry Header { get; }



        protected static readonly MainPageViewModel _mainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();
        protected static readonly ColumnsWindowViewModel _columnsWindowVM = App.ServiceProvider.GetRequiredService<ColumnsWindowViewModel>();
        private Dictionary<SubheaderEntry, ChangeState> _originalState = [];



        public HeaderCommand(HeaderEntry header)
        {
            Header = header;
        }



        /// <summary>
        /// Add header to the header list in the main page
        /// </summary>
        protected void AddHeader()
        {
            Header.State = ChangeState.Added;

            // Add header to the main page list if it doesnt exist yet
            if (!_mainPageVM.Headers.Any(h => h.Id == Header.Id))
                _mainPageVM.Headers.Add(Header);

            // Unmark subheaders as deleted if they were marked as such
            foreach (var kvp in _originalState)
                kvp.Key.State = kvp.Value;

            UpdateUI();
        }



        /// <summary>
        /// Mark header as deleted
        /// </summary>
        protected void DeleteHeader()
        {
            Header.State = ChangeState.Removed;
            _originalState.Clear();

            // Mark all its subheaders as deleted too
            foreach (SubheaderEntry subheader in Header.Subheaders)
            {
                _originalState[subheader] = subheader.State;
                subheader.State = ChangeState.Removed;
            }

            UpdateUI();
        }



        /// <summary>
        /// Edit the header's name and order
        /// </summary>
        /// <param name="newName"></param>
        /// <param name="newOrder"></param>
        protected void EditHeader(string newName, int newOrder)
        {
            Header.Name = newName;
            Header.Order = newOrder;
            Header.State = ChangeState.Edited;

            UpdateUI();
        }


        /// <summary>
        /// Update UI that are affected by this change
        /// </summary>
        private void UpdateUI()
        {
            _mainPageVM.UpdateTotals();
            _columnsWindowVM.UpdateUI();
        }



        // IUndoableCommand methods
        public abstract void Execute();
        public abstract void Undo();
    }
}
