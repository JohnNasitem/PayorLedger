//***********************************************************************************
//Program: ColumnsWindow.cs
//Description: Columns window code-behind
//Date: Sep 4, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models.Columns;
using PayorLedger.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PayorLedger.Windows.Columns
{
    /// <summary>
    /// Interaction logic for ColumnsWindow.xaml
    /// </summary>
    public partial class ColumnsWindow : Window
    {
        private readonly ColumnsWindowViewModel _vm;
        private object? _selectColumn = null;



        public ColumnsWindow(ColumnsWindowViewModel viewmodel)
        {
            InitializeComponent();
            _vm = viewmodel;
            DataContext = _vm;
        }



        /// <summary>
        /// Handles the window closing event to prevent it from closing and instead hide it.
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">Event args</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If main application is exiting, allow the window to close
            if (_vm.ApplicationExit)
                return;

            // Prevent the window from closing
            e.Cancel = true;
            Hide();
        }



        /// <summary>
        /// Open add header dialog
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Event args</param>
        private void UI_AddHeader_Btn_Click(object sender, RoutedEventArgs e)
        {
            _vm.AddHeader();
            UI_Columns_Lbx.SelectedIndex = 0;
        }



        /// <summary>
        /// Open add subheader dialog
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Event args</param>
        private void UI_AddSubheader_Btn_Click(object sender, RoutedEventArgs e)
        {
            _vm.AddSubheader();
        }



        /// <summary>
        /// Open edit column dialog
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Event args</param>
        private void UI_EditColumn_Btn_Click(object sender, RoutedEventArgs e)
        {
            _vm.EditColumn(UI_Columns_Lbx.SelectedIndex);
        }



        /// <summary>
        /// Propmpt a confirmation then delete column
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Event args</param>
        private void UI_DeleteColumn_Btn_Click(object sender, RoutedEventArgs e)
        {
            int payorIndex = UI_Columns_Lbx.SelectedIndex;

            _vm.DeleteColumn(UI_Columns_Lbx.SelectedIndex);

            // Auto select next payor
            SelectPayor(payorIndex);
        }



        /// <summary>
        /// Select a item in the listbox
        /// </summary>
        /// <param name="payorIndex">index to select</param>
        public void SelectPayor(int payorIndex = 0)
        {
            if (payorIndex == -1 && _selectColumn != null)
            {
                UI_Columns_Lbx.SelectedItem = _selectColumn;

                // If its still null then select the closest index
                if (UI_Columns_Lbx.SelectedIndex == -1)
                    SelectClosestIndex();
            }
            else
                SelectClosestIndex();

            void SelectClosestIndex()
            {
                UI_Columns_Lbx.SelectedIndex = UI_Columns_Lbx.Items.Count > 0 ?
                (UI_Columns_Lbx.Items.Count - 1 >= payorIndex ?
                    payorIndex :
                    UI_Columns_Lbx.Items.Count - 1)
                : -1;
                _selectColumn = UI_Columns_Lbx.SelectedItem;
            }
        }



        /// <summary>
        /// Load column list
        /// Do it here instead of the constructor to ensure MainPageViewModel is initialized
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">Event args</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.UpdateUI();

            if (!UpdateButtonStates())
                UI_Columns_Lbx.SelectedIndex = 0;
        }



        /// <summary>
        /// Update the button states
        /// </summary>
        /// <returns>True if buttons are disabled</returns>
        public bool UpdateButtonStates()
        {
            if (_vm.Columns.Count == 0)
            {
                UI_AddSubheader_Btn.IsEnabled = false;
                UI_EditColumn_Btn.Content = "Edit ...";
                UI_DeleteColumn_Btn.Content = "Delete ...";
                UI_EditColumn_Btn.IsEnabled = false;
                UI_DeleteColumn_Btn.IsEnabled = false;
                return true;
            }

            UI_AddSubheader_Btn.IsEnabled = true;
            UI_EditColumn_Btn.IsEnabled = true;
            UI_DeleteColumn_Btn.IsEnabled = true;
            return false;

        }



        /// <summary>
        /// Update edit and delete button to specify if the selected item is a header or subheader
        /// </summary>
        /// <param name="sender">Listbox</param>
        /// <param name="e">Event args</param>
        private void UI_Columns_Lbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UpdateButtonStates())
                return;

            if (UI_Columns_Lbx.SelectedIndex < 0 || UI_Columns_Lbx.SelectedIndex > _vm.Columns.Count - 1)
                UI_Columns_Lbx.SelectedIndex = 0;

            if (_vm.Columns[UI_Columns_Lbx.SelectedIndex].Value is SubheaderEntry)
            {
                UI_EditColumn_Btn.Content = "Edit Subheader";
                UI_DeleteColumn_Btn.Content = "Delete Subheader";
            }
            else
            {
                UI_EditColumn_Btn.Content = "Edit Header";
                UI_DeleteColumn_Btn.Content = "Delete Header";
            }
        }
    }
}
