//***********************************************************************************
//Program: AddRowDialog.cs
//Description: Add row dialog code
//Date: Sept 18, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.Services.Database;
using PayorLedger.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PayorLedger.Dialogs
{
    /// <summary>
    /// Interaction logic for AddRowDialog.xaml
    /// </summary>
    public partial class AddRowDialog : Window
    {
        /// <summary>
        /// Label of row
        /// </summary>
        public RowEntry.RowLabel RowLabel { get; private set; } = RowEntry.RowLabel.Other;



        /// <summary>
        /// Date of row
        /// </summary>
        public string RowDate { get; private set; } = string.Empty;



        /// <summary>
        /// Payor of the row
        /// </summary>
        public long RowPayorId { get; private set; } = -1;



        /// <summary>
        /// Or # of the row
        /// </summary>
        public int RowOrNum { get; private set; } = -1;



        private RowEntry? _existingRow = null;
        private MainPageViewModel _mainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();



        public AddRowDialog(int day, Month month, int year)
        {
            InitializeComponent();
            InitComboBoxes();
            UI_RowPayor_Cmb.SelectedIndex = UI_RowPayor_Cmb.Items.Count - 1;
            RowDate = $"{(day != -1 ? day.ToString("00") : "??")}/{(int)month:00}/{year:0000}";
            UI_RowDate_Tbx.Text = RowDate;
            UI_RowOrNum_Tbx.Text = (_mainPageVM.LedgerRows.Count > 0 ? _mainPageVM.LedgerRows.Max(r => r.OrNum) + 1 : 1).ToString();
        }



        public AddRowDialog(RowEntry existingRow)
        {
            InitializeComponent();

            InitComboBoxes();
            PayorEntry payor = _mainPageVM.Payors.Find(p => p.PayorId == existingRow.PayorId)!;
            UI_RowPayor_Cmb.SelectedItem = new DropDownItemPayor { Name = payor.PayorName, Value = payor.PayorId };

            _existingRow = existingRow;
            UI_RowDate_Tbx.Text = existingRow.Date;
            UI_RowOrNum_Tbx.Text = existingRow.OrNum.ToString();

            UI_Title_Lbl.Content = "Edit Row";
            UI_AddRow_Btn.Content = "Save Changes";

            UpdateButton();
        }




        /// <summary>
        /// Initialize the combo boxes
        /// </summary>
        public void InitComboBoxes()
        {
            UI_RowPayor_Cmb.ItemsSource = App.ServiceProvider.GetRequiredService<MainPageViewModel>().Payors.Select(p => new DropDownItemPayor { Name = p.PayorName, Value = p.PayorId });
            UI_RowPayor_Cmb.DisplayMemberPath = "Name";

            UI_RowLabel_Cmb.ItemsSource = Enum.GetValues<RowEntry.RowLabel>()
                                                .Select(x => new DropDownItemLabel { Name = x.ToString(), Value = x })
                                                .ToList();
            UI_RowLabel_Cmb.DisplayMemberPath = "Name";
        }



        /// <summary>
        /// Update the state of the Add button based on the input fields.
        /// </summary>
        public void UpdateButton()
        {
            if (!int.TryParse(UI_RowOrNum_Tbx.Text, out int orNum) || orNum < 0)
            {
                UI_Status_Lbl.Text = "Or # must be a positive integer";
                UI_Status_Lbl.Visibility = Visibility.Visible;
                UI_AddRow_Btn.IsEnabled = false;
                return;
            }

            if (_existingRow?.OrNum != orNum && _mainPageVM.LedgerRows.Select(r => r.OrNum).Contains(orNum))
            {
                UI_Status_Lbl.Text = "A row with this Or # already exists";
                UI_Status_Lbl.Visibility = Visibility.Visible;
                UI_AddRow_Btn.IsEnabled = false;
                return;
            }

            UI_Status_Lbl.Visibility = Visibility.Hidden;

            UI_AddRow_Btn.IsEnabled =
                UI_RowDate_Tbx.Text.Trim().Length > 0 &&
                UI_RowPayor_Cmb.SelectedIndex > -1 &&
                UI_RowLabel_Cmb.SelectedIndex > -1;
        }



        /// <summary>
        /// Click event handler for the Add button.
        /// </summary>
        /// <param name="sender">Object that called the method</param>
        /// <param name="e">Event args</param>
        private void UI_AddRow_Btn_Click(object sender, RoutedEventArgs e)
        {
            RowLabel = ((DropDownItemLabel)UI_RowLabel_Cmb.SelectedItem).Value;
            RowDate = UI_RowDate_Tbx.Text.Trim();
            RowPayorId = ((DropDownItemPayor)UI_RowPayor_Cmb.SelectedItem).Value;
            RowOrNum = int.Parse(UI_RowOrNum_Tbx.Text);
            DialogResult = true;
        }



        /// <summary>
        /// Check for enter
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">Event args</param>
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                UI_AddRow_Btn_Click(sender, e);
        }



        // Update button state on change
        private void UI_RowDate_Tbx_TextChanged(object sender, TextChangedEventArgs e) => UpdateButton();
        private void UI_RowOrNum_Tbx_TextChanged(object sender, TextChangedEventArgs e) => UpdateButton();
        private void UI_RowLabel_Cmb_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateButton();



        /// <summary>
        /// Struct to hold the payor drop down item values
        /// </summary>
        private struct DropDownItemPayor
        {
            public string Name { get; set; }
            public long Value { get; set; }
        }



        /// <summary>
        /// Struct to hold the label drop down item values
        /// </summary>
        private struct DropDownItemLabel
        {
            public string Name { get; set; }
            public RowEntry.RowLabel Value { get; set; }
        }
    }
}
