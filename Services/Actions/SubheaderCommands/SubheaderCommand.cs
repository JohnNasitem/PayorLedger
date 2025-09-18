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

namespace PayorLedger.Services.Actions.SubheaderCommands
{
    public abstract class SubheaderCommand : IUndoableCommand
    {
        /// <summary>
        /// Subheader entry being manipulated
        /// </summary>
        public SubheaderEntry Subheader { get; }



        protected static readonly MainPageViewModel _mainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();
        protected static readonly ColumnsWindowViewModel _columnsWindowVM = App.ServiceProvider.GetRequiredService<ColumnsWindowViewModel>();



        public SubheaderCommand(SubheaderEntry subheader)
        {
            Subheader = subheader;
        }



        /// <summary>
        /// Add subheader to the main page under its parent header
        /// </summary>
        protected void AddSubheader()
        {
            Subheader.State = ChangeState.Added;

            HeaderEntry? parentHeaderInMainPage = _mainPageVM.Headers.FirstOrDefault(h => h.Id == Subheader.Header.Id);

            // Add subheader to the main page list if it doesnt exist yet
            if (parentHeaderInMainPage != null && !parentHeaderInMainPage.Subheaders.Any(s => s.Id == Subheader.Id))
                parentHeaderInMainPage.Subheaders.Add(Subheader);

            UpdateUI();
        }



        /// <summary>
        /// Mark subheader as deleted
        /// </summary>
        protected void DeleteSubheader()
        {
            Subheader.State = ChangeState.Removed;

            UpdateUI();
        }



        /// <summary>
        /// Edit the subheader's parent header, name, and order
        /// </summary>
        /// <param name="newParentHeader">New parent header</param>
        /// <param name="newName">New subheader name</param>
        /// <param name="newOrder">New subheader order</param>
        protected void EditSubheader(HeaderEntry newParentHeader, string newName, int newOrder)
        {
            // Remove from old parent
            Subheader.Header.Subheaders.Remove(Subheader);

            // Add to new parent
            newParentHeader.Subheaders.Add(Subheader);
            Subheader.Header = newParentHeader;

            Subheader.Name = newName;
            Subheader.Order = newOrder;
            Subheader.State = ChangeState.Edited;

            UpdateUI();
        }



        /// <summary>
        /// Update UI that are affected by this change
        /// </summary>
        private void UpdateUI()
        {
            _mainPageVM.UpdateUI();
            _columnsWindowVM.UpdateUI();
        }



        // IUndoableCommand methods
        public abstract void Execute();
        public abstract void Undo();
    }
}
