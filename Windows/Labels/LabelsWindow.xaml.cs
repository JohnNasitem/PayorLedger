//***********************************************************************************
//Program: LabelsWindow.cs
//Description: Labels window code-behind
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Dialogs;
using PayorLedger.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PayorLedger.Windows.Labels
{
    /// <summary>
    /// Interaction logic for LabelsWindow.xaml
    /// </summary>
    public partial class LabelsWindow : Window
    {
        private LabelsWindowViewModel _vm;



        public LabelsWindow(LabelsWindowViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }



        /// <summary>
        /// Refresh tabs
        /// </summary>
        public void RefreshTabs()
        {
            int selectedIndex = UI_LabelTypes_Tbc.SelectedIndex;
            UI_LabelTypes_Tbc.ItemsSource = null;
            UI_LabelTypes_Tbc.ItemsSource = _vm.Tabs;
            UI_LabelTypes_Tbc.SelectedIndex = selectedIndex == -1 ? 0 : selectedIndex;
        }



        /// <summary>
        /// Load in data once window is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.SetYear(DateTime.Now.Year);
            UI_SelectYear_Tbx.Text = _vm.Year.ToString();
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



        #region YearControls
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



        /// <summary>
        /// Validate the new year
        /// </summary>
        /// <param name="sender">Textbox</param>
        /// <param name="e">Event args</param>
        private void UI_SelectYear_Tbx_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                YearTextChanged();
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
        #endregion
    }
}
