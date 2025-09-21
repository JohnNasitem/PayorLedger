//***********************************************************************************
//Program: MainPageViewModel.cs
//Description: Add payor dialog code
//Date: May 26, 2025
//Author: John Nasitem
//***********************************************************************************


using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.Services.Database;
using PayorLedger.ViewModels;


namespace PayorLedger.Dialogs
{
    /// <summary>
    /// Interaction logic for AddPayorDialog.xaml
    /// </summary>
    public partial class AddPayorDialog : Window
    {
        /// <summary>
        /// Name of payor
        /// </summary>
        public string PayorName { get; private set; } = string.Empty;



        private readonly PayorEntry? _existingPayor = null;
        private MainPageViewModel _mainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();
        private string[] _invalidNames = App.ServiceProvider.GetRequiredService<IDatabaseService>().InvalidNames;


        /// <summary>
        /// Prompt the user to add a new payor
        /// </summary>
        public AddPayorDialog()
        {
            InitializeComponent();
        }



        /// <summary>
        /// Prompt the user to edit an existing payor
        /// </summary>
        /// <param name="existingPayor">Payor to edit</param>
        public AddPayorDialog(PayorEntry existingPayor)
        {
            InitializeComponent();
            UI_PayorName_Tbx.Text = existingPayor.PayorName;
            UI_WindowPrompt_Lbl.Content = "Edit Payor";
            UI_AddPayor_Btn.Content = "Save Changes";
            _existingPayor = existingPayor;
            UpdateButton();
        }



        /// <summary>
        /// Update the state of the Add button based on the input fields.
        /// </summary>
        private void UpdateButton()
        {
            // Check if name already exists
            bool nameTaken = _mainPageVM.Payors.Where(p => p.State != ChangeState.Removed).Select(p => p.PayorName.ToLower()).Contains(UI_PayorName_Tbx.Text.Trim().ToLower());
            if (UI_PayorName_Tbx.Text.Trim().ToLower() != _existingPayor?.PayorName?.ToLower() && nameTaken)
            {
                UI_Status_Lbl.Visibility = Visibility.Visible;
                UI_Status_Lbl.Text = _invalidNames.Contains(UI_PayorName_Tbx.Text.Trim().ToLower()) ? "This name is not allowed as a payor!" : "Payor with this name already exists!";
                UI_AddPayor_Btn.IsEnabled = false;
                return;
            }
            UI_Status_Lbl.Visibility = Visibility.Hidden;

            // Enable button if both fields have a valid input
            UI_AddPayor_Btn.IsEnabled = UI_PayorName_Tbx.Text.Trim().Length > 0;
        }



        /// <summary>
        /// Close dialog
        /// </summary>
        /// <param name="sender">Objecet that called the method</param>
        /// <param name="e">Event args</param>
        private void UI_AddPayor_Btn_Click(object sender, RoutedEventArgs e)
        {
            PayorName = UI_PayorName_Tbx.Text.Trim();
            DialogResult = true;
        }



        /// <summary>
        /// Update the Add button when the payor name or the selected label changes.
        /// </summary>
        private void UI_PayorName_Tbx_TextChanged(object sender, TextChangedEventArgs e) => UpdateButton();
    }
}
