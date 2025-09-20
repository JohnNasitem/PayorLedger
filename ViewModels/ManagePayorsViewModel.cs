//***********************************************************************************
//Program: ManagePayorsViewModel.cs
//Description: View model for manage payors page
//Date: Aug 21, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Dialogs;
using PayorLedger.Enums;
using PayorLedger.Models;
using PayorLedger.Services.Actions;
using PayorLedger.Services.Actions.PayorCommands;
using PayorLedger.Services.Database;
using PayorLedger.Services.Logger;
using PayorLedger.Windows.Payors.Pages;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace PayorLedger.ViewModels
{
    public class ManagePayorsViewModel
    {
        /// <summary>
        /// Page associated with this view model
        /// </summary>
        public ManagePayors Page { get; }



        /// <summary>
        /// Collection of payor names to display in the UI.
        /// </summary>
        public ObservableCollection<string> Payors { get; set; }


        private readonly IUndoRedoService _undoRedoService;
        private readonly ILogger _logger;
        private readonly PayorWindowViewModel _payorWindowVM;



        public ManagePayorsViewModel(PayorWindowViewModel payorWindowVM, IUndoRedoService undoRedoService, ILogger logger)
        {
            // Initialize global vars
            _undoRedoService = undoRedoService;
            _logger = logger;
            _payorWindowVM = payorWindowVM;
            Payors = [];

            Page = new(this);

            UpdateUI();
        }



        #region PageButtonMethods
        /// <summary>
        /// Edit the selected payor
        /// </summary>
        /// <param name="payorName">Name of specified payor</param>
        public void EditPayor(string? payorName)
        {
            PayorEntry? payor = PayorNameToPayorEntry(payorName);
            if (payor == null)
                return;

            AddPayorDialog page = new(payor);

            if (page.ShowDialog() != true || (payor.PayorName == page.PayorName && payor.Label == page.PayorLabel))
                return;

            _logger.AddLog($"Attempting to edit a payor. Name: \"{page.PayorName}\" - Label: \"{Enum.GetName<PayorEnums.PayorLabel>(page.PayorLabel)}\"", Logger.LogType.PreAction);

            // Create a new payor with the data from the dialog
            _undoRedoService.Execute(new EditPayorCommand(payor, page.PayorName, page.PayorLabel));
        }



        /// <summary>
        /// View the specified payor
        /// </summary>
        /// <param name="payorName">Name of specified payor</param>
        public void ViewPayorBalance(string? payorName)
        {
            if (payorName == null)  
                return;

            _payorWindowVM.ViewPayor(payorName);
        }
        


        /// <summary>
        /// Delete the specified payor from the database
        /// </summary>
        /// <param name="payorName">name of specified payor</param>
        public void DeletePayor(string? payorName)
        {
            PayorEntry? payor = PayorNameToPayorEntry(payorName);
            if (payor == null)
                return;

            ConfirmationDialog confirmationDlg = new(
                "Delete Payor",
                $"Are you sure you want to delete the payor '{payor.PayorName}'?",
                Brushes.Red);

            bool? result = confirmationDlg.ShowDialog();

            // Delete payor and their entries
            if (result == true)
            {
                _logger.AddLog($"Attempting to delete a payor.", Logger.LogType.PreAction);
                _undoRedoService.Execute(new DeletePayorCommand(payor));
                UpdateUI();
            }
        }



        /// <summary>
        /// Call AddPayor method from the main page view model
        /// </summary>
        public void AddNewPayor()
        {
            App.ServiceProvider.GetRequiredService<MainPageViewModel>().ExecuteAddPayor();
            UpdateUI();
        }
        #endregion



        /// <summary>
        /// Converts a payor name to a PayorEntry object by searching the database.
        /// </summary>
        /// <param name="payorName">payor name to search</param>
        /// <returns>PayorEntry if an entry with the name exists, otherwise null</returns>
        private PayorEntry? PayorNameToPayorEntry(string? payorName)
        {
            if (payorName == null)
                return null;

            PayorEntry? payor = App.ServiceProvider.GetRequiredService<MainPageViewModel>().Payors.Where(p => p.State != ChangeState.Removed).FirstOrDefault(e => e.PayorName == payorName);

            // Seperate the if statements just in case a payor name is null
            if (payor == null)
                return null;

            return payor;
        }



        /// <summary>
        /// Populates the Payors collection with payor names from the database.
        /// </summary>
        public void UpdateUI()
        {
            MainPageViewModel mainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();

            Payors.Clear();
            foreach (PayorEntry payor in mainPageVM.Payors.Where(p => p.State != ChangeState.Removed).OrderBy(e => e.PayorName))
                Payors.Add(payor.PayorName);

            Page.UpdateButtonStates();
            mainPageVM.Page.UpdateAddRowButtonState();
        }
    }
}
