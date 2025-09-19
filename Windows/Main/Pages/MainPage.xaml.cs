//***********************************************************************************
//Program: MainPage.cs
//Description: Main page for editting ledger
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Dialogs;
using PayorLedger.Models;
using PayorLedger.Services.Database;
using PayorLedger.ViewModels;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace PayorLedger.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private MainPageViewModel _vm;



        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _vm = viewModel;

            UI_MonthlyLedgers_Tbc.ItemsSource = viewModel.Tabs;
            viewModel.RedrawTable += () =>
            {
                int index = UI_MonthlyLedgers_Tbc.SelectedIndex;
                UI_MonthlyLedgers_Tbc.ItemsSource = null;
                UI_MonthlyLedgers_Tbc.ItemsSource = viewModel.Tabs;
                UI_MonthlyLedgers_Tbc.SelectedIndex = index;
            };
            UI_SelectYear_Tbx.Text = _vm.Year.ToString();
        }



        /// <summary>
        /// Registers all the menu shortcuts
        /// </summary>
        /// <param name="window">Window the page is attached to</param>
        public void RegisterMenuShortcuts(MainWindow window)
        {
            MainPageViewModel vm = (MainPageViewModel)DataContext;
            RegisterMenuShortcut(window, UI_SaveLedger_Mni, vm.SaveLedgerCommand, Key.S, ModifierKeys.Control);
            RegisterMenuShortcut(window, UI_Undo_Mni, vm.UndoCommand, Key.Z, ModifierKeys.Control);
            RegisterMenuShortcut(window, UI_Redo_Mni, vm.RedoCommand, Key.Y, ModifierKeys.Control);
            RegisterMenuShortcut(window, UI_AddPayor_Mni, vm.AddPayorCommand, Key.P, ModifierKeys.Control | ModifierKeys.Shift);
            RegisterMenuShortcut(window, UI_AddHeader_Mni, vm.AddHeaderCommand, Key.D1, ModifierKeys.Control | ModifierKeys.Shift);
            RegisterMenuShortcut(window, UI_AddSubheader_Mni, vm.AddSubheaderCommand, Key.D2, ModifierKeys.Control | ModifierKeys.Shift);
            RegisterMenuShortcut(window, UI_Payors_Mni, vm.OpenPayorWindowCommand, Key.P, ModifierKeys.Control | ModifierKeys.Alt);
            RegisterMenuShortcut(window, UI_Columns_Mni, vm.OpenColumnsWindowCommand, Key.C, ModifierKeys.Control | ModifierKeys.Alt);
        }



        /// <summary>
        /// Register a new menu shortcut
        /// </summary>
        /// <param name="item">Menu item</param>
        /// <param name="command">Command to execute</param>
        /// <param name="key">Shortcut key</param>
        /// <param name="modifiers">Shortcut modifiers</param>
        private static void RegisterMenuShortcut(MainWindow window, MenuItem item, ICommand command, Key key, ModifierKeys modifiers)
        {
            // Set the command
            item.Command = command;

            // Set display text for menu item
            item.InputGestureText = GetGestureText(key, modifiers);

            // Add the key binding
            window.InputBindings.Add(new KeyBinding(command, key, modifiers));
        }



        /// <summary>
        /// Get the input gesture text for the menu item
        /// </summary>
        /// <param name="key">Key pressed</param>
        /// <param name="modifiers">Shortcut modifiers</param>
        /// <returns>Input gesture text</returns>
        private static string GetGestureText(Key key, ModifierKeys modifiers)
        {
            string gestureText = "";

            if (modifiers.HasFlag(ModifierKeys.Control)) gestureText += "Ctrl+";
            if (modifiers.HasFlag(ModifierKeys.Shift)) gestureText += "Shift+";
            if (modifiers.HasFlag(ModifierKeys.Alt)) gestureText += "Alt+";
            gestureText += key.ToString();

            return gestureText;
        }



        /// <summary>
        /// Bind context menu to generated columns
        /// </summary>
        /// <param name="sender">Object that called the code</param>
        /// <param name="e">Event args</param>
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            DataTable dataTable = _vm.Tabs[1].Content;

            // Check if the column is a default column
            if (dataTable.Columns.Contains(e.PropertyName) && (dataTable.Columns[e.PropertyName]!.ExtendedProperties["IsDefault"] as bool? ?? false))
            {
                // If the its not the payor column, don't add a context menu
                if (e.PropertyName != "Payor")
                    return;

                // Set up right click context menu
                var cellStyle = new Style(typeof(DataGridCell));
                var cellMenu = new ContextMenu();
                var menuItem = new MenuItem
                {
                    Header = "View Payor",
                    Command = _vm.ViewPayorCommand,
                    // This gets the cell value
                    CommandParameter = e.Column.Header.ToString()
                };
                menuItem.SetBinding(MenuItem.CommandParameterProperty, new Binding(e.PropertyName));
                cellMenu.Items.Add(menuItem);
                cellStyle.Setters.Add(new Setter(ContextMenuProperty, cellMenu));
                e.Column.CellStyle = cellStyle;
            }
            else
            {
                // Set up right click context menu
                DataGridColumnHeader header = new()
                {
                    Content = e.Column.Header,
                    ContextMenu = new ContextMenu()
                };

                header.ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Delete Column",
                    Command = _vm.DeleteSubheaderCommand,
                    CommandParameter = e.Column.Header.ToString()

                });

                e.Column.Header = header;
            }
        }



        /// <summary>
        /// Validates the cell value before its processed
        /// </summary>
        /// <param name="sender">Data grid table</param>
        /// <param name="e">Event args</param>
        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Comments" || e.Column.Header.ToString() == "Date")
            {
                // Send update to view model
                ((MainPageViewModel)DataContext).RowEdittedCommand.Execute(
                    new RowEditInfo(
                        int.Parse(((DataRowView)e.Row.Item)!.Row["Or #"].ToString()!),
                        ((DataRowView)e.Row.Item)!.Row["Comments"].ToString()!,
                        ((DataRowView)e.Row.Item)!.Row["Date"].ToString()!
                    ));
            }
            else
            {
                decimal newVal = 0;

                // Don't update if new cell value is not a decimal
                if (((TextBox)e.EditingElement).Text != "" && !decimal.TryParse(((TextBox)e.EditingElement).Text, out newVal))
                {
                    e.Cancel = true;

                    MessageBox.Show("Please enter a valid decimal number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Force the DataGrid to reselect the same cell
                    var dataGrid = sender as DataGrid;
                    // Because DataGrid_CellEditEnding runs before the edit is committed, we need to use Dispatcher to delay the action
                    dataGrid?.Dispatcher?.InvokeAsync(() =>
                    {
                        dataGrid.CurrentCell = new DataGridCellInfo(e.Row.Item, e.Column);
                        dataGrid.BeginEdit();
                    });
                    return;
                }

                // Send update to view model
                ((MainPageViewModel)DataContext).CellEdittedCommand.Execute(
                    new CellEditInfo(
                        ((DataGridColumnHeader)e.Column.Header).Content.ToString()!,
                        int.Parse(((DataRowView)e.Row.Item)!.Row["Or #"].ToString()!),
                        newVal
                    ));
            }
        }



        /// <summary>
        /// Prevent editing of total row
        /// </summary>
        /// <param name="sender">Data table</param>
        /// <param name="e">Event Args</param>
        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (((DataRowView)e.Row.Item!).Row["Payor"].ToString() == "Total")
                e.Cancel = true;
        }



        /// <summary>
        /// Clear cell if its 0 when editing starts
        /// </summary>
        /// <param name="sender">Data table</param>
        /// <param name="e">Event args</param>
        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement is TextBox cell && cell.Text == "0")
                cell.Clear();
        }



        /// <summary>
        /// Set the saved changes status
        /// </summary>
        /// <param name="changesUnsaved">True if unsaved changes exist, otherwise false</param>
        public void SetUnsavedChangesStatus(bool changesUnsaved)
        {
            UI_ChangesSavedStatus_Tbk.Text = changesUnsaved ? "Unsaved Changes" : "All Changes Saved";
        }



        /// <summary>
        /// Validate the new year
        /// </summary>
        /// <param name="sender">Textbox</param>
        /// <param name="e">Event args</param>
        private void UI_SelectYear_Tbx_LostFocus(object sender, RoutedEventArgs e)
        {
            YearTextChanged();
        }



        /// <summary>
        /// Validate the new year
        /// </summary>
        /// <param name="sender">Textbox</param>
        /// <param name="e">Event args</param>
        private void UI_SelectYear_Tbx_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                YearTextChanged();
        }



        /// <summary>
        /// Change year to the new selected year
        /// </summary>
        private void YearTextChanged()
        {
            // Use current selected year if user empties text box
            if (UI_SelectYear_Tbx.Text == "")
            {
                UI_SelectYear_Tbx.Text = _vm.Year.ToString();
                return;
            }

            if (!int.TryParse(UI_SelectYear_Tbx.Text, out int year))
            {
                MessageBox.Show($"\"{UI_SelectYear_Tbx.Text}\" is an invalid year! Please input a valid year or clear the text to use the last valid year.", "Error");
                return;
            }

            if (_vm.Year == year)
                return;
            else if (year < 0)
            {
                MessageBox.Show($"Cannot select a negative year!", "Error");
                return;
            }
            // Check with the user if they are sure they want to change the year to the a year much farther than the currently selected one
            else if (Math.Abs(year - _vm.Year) > 100)
            {
                bool? dlgResult = new ConfirmationDialog("Confirm Year Selection", $"Are you sure you want to select the year \"{year}\"?, it is very far from your current year \"{_vm.Year}\"").ShowDialog();
                if (dlgResult != true)
                {
                    UI_SelectYear_Tbx.Text = _vm.Year.ToString();
                    return;
                }
            }

            _vm.SetYear(year);
        }



        /// <summary>
        /// Increment/Deincrement the currently selected year
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Event args</param>
        private void YearEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == UI_IncreaseYear_Btn)
                _vm.SetYear(_vm.Year + 1);
            else if (sender == UI_DecreaseYear_Btn && _vm.Year > 0)
                _vm.SetYear(_vm.Year - 1);

            UI_SelectYear_Tbx.Text = _vm.Year.ToString();
        }
    }
}
