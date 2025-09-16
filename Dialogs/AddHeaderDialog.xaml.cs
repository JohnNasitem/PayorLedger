//***********************************************************************************
//Program: AddHeaderDialog.cs
//Description: Add header dialog code
//Date: May 29, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Database;
using PayorLedger.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PayorLedger.Dialogs
{
    /// <summary>
    /// Interaction logic for AddHeaderDialog.xaml
    /// </summary>
    public partial class AddHeaderDialog : Window
    {
        /// <summary>
        /// Name of header
        /// </summary>
        public string HeaderName { get; private set; } = string.Empty;



        /// <summary>
        /// Order of the header
        /// </summary>
        public int HeaderOrder { get; private set; } = -1;



        private HeaderEntry? _existingHeader = null;
        private MainPageViewModel _mainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();
        private string[] _invalidNames = App.ServiceProvider.GetRequiredService<IDatabaseService>().InvalidNames;



        public AddHeaderDialog()
        {
            InitializeComponent();
            PopulateAvailableOrders();
            UI_HeaderOrder_Cmb.SelectedIndex = UI_HeaderOrder_Cmb.Items.Count - 1;
        }



        public AddHeaderDialog(HeaderEntry existingEntry)
        {
            InitializeComponent();

            PopulateAvailableOrders();
            UI_HeaderOrder_Cmb.SelectedItem = existingEntry.Order;

            _existingHeader = existingEntry;
            UI_HeaderName_Tbx.Text = existingEntry.Name;

            UI_Title_Lbl.Content = "Edit Header";
            UI_AddHeader_Btn.Content = "Save Changes";

            UpdateButton();
        }



        /// <summary>
        /// Populate order combo box
        /// </summary>
        private void PopulateAvailableOrders()
        {
            List<HeaderEntry> headers = App.ServiceProvider.GetRequiredService<MainPageViewModel>().Headers;
            int orderCount = 1;

            if (headers.Count > 0)
                orderCount = headers.Select(h => h.Order).Max() + 1;

            UI_HeaderOrder_Cmb.ItemsSource = Enumerable.Range(1, orderCount).ToList();
        }



        /// <summary>
        /// Update the state of the Add button based on the input fields.
        /// </summary>
        /// <param name="sender">Object that called the method</param>
        /// <param name="e">Event args</param>
        private void UI_HeaderName_Tbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateButton();
        }



        /// <summary>
        /// Update the state of the Add button based on the input fields.
        /// </summary>
        public void UpdateButton()
        {
            // Check if name already exists
            bool nameTaken = _mainPageVM.Headers.Where(h => h.State != ChangeState.Removed).Select(h => h.Name.ToLower()).Contains(UI_HeaderName_Tbx.Text.Trim().ToLower());
            if (UI_HeaderName_Tbx.Text.Trim().ToLower() != _existingHeader?.Name?.ToLower() && nameTaken)
            {
                UI_Status_Lbl.Visibility = Visibility.Visible;
                UI_Status_Lbl.Text = _invalidNames.Contains(UI_HeaderName_Tbx.Text.Trim().ToLower()) ? "This name is not allowed as a header!" : "Header with this name already exists!";
                UI_AddHeader_Btn.IsEnabled = false;
                return;
            }
            UI_Status_Lbl.Visibility = Visibility.Hidden;

            // Enable button if the header field isnt empty
            UI_AddHeader_Btn.IsEnabled = UI_HeaderName_Tbx.Text.Trim().Length > 0;
        }



        /// <summary>
        /// Click event handler for the Add button.
        /// </summary>
        /// <param name="sender">Object that called the method</param>
        /// <param name="e">Event args</param>
        private void UI_AddHeader_Btn_Click(object sender, RoutedEventArgs e)
        {
            HeaderName = UI_HeaderName_Tbx.Text.Trim();
            HeaderOrder = (int)UI_HeaderOrder_Cmb.SelectedItem;
            DialogResult = true;
        }
    }
}
