//***********************************************************************************
//Program: AddSubheaderDialog.cs
//Description: Add Subheader dialog code
//Date: May 29, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Database;
using PayorLedger.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PayorLedger.Dialogs
{
    /// <summary>
    /// Interaction logic for AddSubheaderDialog.xaml
    /// </summary>
    public partial class AddSubheaderDialog : Window
    {
        /// <summary>
        /// Name of the subheader
        /// </summary>
        public string SubheaderName { get; private set; } = string.Empty;



        /// <summary>
        /// Order of the subheader
        /// </summary>
        public int SubheaderOrder { get; private set; } = -1;



        /// <summary>
        /// Parent header
        /// </summary>
        public HeaderEntry ParentHeader { get; private set; } = null!;



        private readonly SubheaderEntry? _existingSubheader = null;



        /// <summary>
        /// Add a new subheader
        /// </summary>
        public AddSubheaderDialog()
        {
            InitializeComponent();
            PopulateHeaderList();

            UI_SubheaderOrder_Cmb.IsEnabled = false;
        }



        /// <summary>
        /// Edit a existing subheader
        /// </summary>
        /// <param name="existingEntry"></param>
        public AddSubheaderDialog(SubheaderEntry existingEntry)
        {
            InitializeComponent();

            _existingSubheader = existingEntry;

            PopulateHeaderList();
            UI_ParentHeader_Cmb.SelectedIndex = ((List<DropDownItem>)UI_ParentHeader_Cmb.ItemsSource).FindIndex(ddi => ddi.Value.Id == _existingSubheader.Header.Id);
            UI_SubheaderOrder_Cmb.SelectedItem = _existingSubheader.Order;
            UI_SubheaderName_Tbx.Text = _existingSubheader.Name;

            UI_Title_Lbl.Content = "Edit Subheader";
            UI_AddSubheader_Btn.Content = "Save Changes";
        }



        /// <summary>
        /// Populate the parent header combo box
        /// </summary>
        public void PopulateHeaderList()
        {
            // Populate the parent header dropdown with headers from the database
            UI_ParentHeader_Cmb.ItemsSource = App.ServiceProvider.GetRequiredService<MainPageViewModel>()
                                                .Headers
                                                .Where(h => h.State != ChangeState.Removed)
                                                .Select(x => new DropDownItem { Name = x.Name, Value = x })
                                                .ToList();
            UI_ParentHeader_Cmb.DisplayMemberPath = "Name";
        }



        /// <summary>
        /// Populate order combo box
        /// </summary>
        private void PopulateAvailableOrders()
        {
            int orderCount = 1;
            List<SubheaderEntry> subheaders = ((DropDownItem)UI_ParentHeader_Cmb.SelectedItem).Value.Subheaders;

            if (subheaders.Count > 0)
                orderCount = subheaders.Select(s => s.Order).Max() + 1;

            UI_SubheaderOrder_Cmb.ItemsSource = Enumerable.Range(1, orderCount).ToList();
        }



        /// <summary>
        /// Update the state of the Add button based on the input fields.
        /// </summary>
        private void UpdateButton()
        {
            // Disable button if no header is selected
            if (UI_ParentHeader_Cmb.SelectedItem == null)
            {
                UI_Status_Lbl.Visibility = Visibility.Hidden;
                UI_AddSubheader_Btn.IsEnabled = false;
                return;
            }

            bool nameTaken = ((DropDownItem)UI_ParentHeader_Cmb.SelectedItem).Value
                                    .Subheaders.Select(s => s.Name.ToLower())
                                    .Contains(UI_SubheaderName_Tbx.Text.Trim().ToLower());
            bool isSelectedHeaderIsCurrentHeader = ((DropDownItem)UI_ParentHeader_Cmb.SelectedItem).Value.Id == _existingSubheader?.Header?.Id;
            bool isNameUnchanged = UI_SubheaderName_Tbx.Text.Trim().ToLower() == _existingSubheader?.Name?.ToLower();
            bool isAdding = _existingSubheader == null;

            // Disable button if name is invalid
            if ((isAdding && nameTaken) || (!isAdding && nameTaken && ((isSelectedHeaderIsCurrentHeader && !isNameUnchanged) || !isSelectedHeaderIsCurrentHeader)))
            {
                UI_Status_Lbl.Visibility = Visibility.Visible;
                UI_Status_Lbl.Text = App.ServiceProvider.GetRequiredService<IDatabaseService>().InvalidNames.Contains(UI_SubheaderName_Tbx.Text.Trim().ToLower()) ? "This name is not allowed as a subheader!" : "Subheader with this name already exists!";
                UI_AddSubheader_Btn.IsEnabled = false;
                return;
            }
            UI_Status_Lbl.Visibility = Visibility.Hidden;

            // Enable button if both fields have a valid input
            UI_AddSubheader_Btn.IsEnabled = UI_SubheaderName_Tbx.Text.Trim().Length > 0;
        }



        /// <summary>
        /// Close dialog
        /// </summary>
        /// <param name="sender">Objecet that called the method</param>
        /// <param name="e">Event args</param>
        private void UI_AddSubheader_Btn_Click(object sender, RoutedEventArgs e)
        {
            SubheaderName = UI_SubheaderName_Tbx.Text.Trim();
            SubheaderOrder = (int)UI_SubheaderOrder_Cmb.SelectedItem;
            ParentHeader = ((DropDownItem)UI_ParentHeader_Cmb.SelectedItem).Value;
            DialogResult = true;
        }



        /// <summary>
        /// Update the Add button when the payor name or the selected label changes.
        /// </summary>
        private void UI_SubheaderName_Tbx_TextChanged(object sender, TextChangedEventArgs e) => UpdateButton();
        private void UI_ParentHeader_Cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButton();

            if (UI_ParentHeader_Cmb.SelectedIndex != -1)
            {
                UI_SubheaderOrder_Cmb.IsEnabled = true;
                PopulateAvailableOrders();
                UI_SubheaderOrder_Cmb.SelectedIndex = _existingSubheader != null && ((DropDownItem)UI_ParentHeader_Cmb.SelectedItem).Value.Id == _existingSubheader.Id ? _existingSubheader.Order : UI_SubheaderOrder_Cmb.Items.Count - 1;
            }
        }



        /// <summary>
        /// Struct to hold the drop down item values
        /// </summary>
        private struct DropDownItem
        {
            public string Name { get; set; }
            public HeaderEntry Value { get; set; }
        }



        /// <summary>
        /// Allow users to press enter 
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">Event args</param>
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && UI_AddSubheader_Btn.IsEnabled)
                UI_AddSubheader_Btn_Click(sender, e);
        }
    }
}
