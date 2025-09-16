//***********************************************************************************
//Program: ManagePayors.cs
//Description: Manage payors page code-behind
//Date: Aug 21, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.ViewModels;
using System.Windows.Controls;

namespace PayorLedger.Windows.Payors.Pages
{
    /// <summary>
    /// Interaction logic for ManagePayors.xaml
    /// </summary>
    public partial class ManagePayors : Page
    {
        private readonly ManagePayorsViewModel _vm;
        private object? _selectedPayor = null;



        public ManagePayors(ManagePayorsViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = vm;
            UI_PayorListBox_Lbx.SelectedIndex = 0;
            UpdateButtonStates();
        }



        #region ButtonMethods
        /// <summary>
        /// Prompt user to edit the selected payor
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Event args</param>
        private void UI_EditPayor_Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm.EditPayor(UI_PayorListBox_Lbx.SelectedItem.ToString());
        }



        /// <summary>
        /// Prompt user to view the selected payor
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Event args</param>
        private void UI_ViewPayorBalance_Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm.ViewPayorBalance(UI_PayorListBox_Lbx.SelectedItem.ToString());
        }



        /// <summary>
        /// Prompt user to delete the selected payor
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Event args</param>
        private void UI_DeletePayor_Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int payorIndex = UI_PayorListBox_Lbx.SelectedIndex;

            _vm.DeletePayor(UI_PayorListBox_Lbx.SelectedItem.ToString());

            // Auto select next payor
            SelectPayor(payorIndex);
        }



        /// <summary>
        /// Prompt user to add a payor
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Event args</param>
        private void UI_AddPayor_Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _selectedPayor = UI_PayorListBox_Lbx.SelectedItem;

            _vm.AddNewPayor();

            // Reselect a payor as adding a new one causes them to be unselected
            UI_PayorListBox_Lbx.SelectedItem = _selectedPayor;
        }
        #endregion



        /// <summary>
        /// Select a payor in the list box. If no index is specified then it will try to select the previously selected payor.
        /// </summary>
        /// <param name="payorIndex"></param>
        public void SelectPayor(int payorIndex = -1)
        {
            if (payorIndex == -1 && _selectedPayor != null)
            {
                UI_PayorListBox_Lbx.SelectedItem = _selectedPayor;

                // If its still null then select the closest index
                if (UI_PayorListBox_Lbx.SelectedIndex == -1)
                    SelectClosestIndex();
            }
            else
                SelectClosestIndex();

            void SelectClosestIndex()
            {
                UI_PayorListBox_Lbx.SelectedIndex = UI_PayorListBox_Lbx.Items.Count > 0 ?
                (UI_PayorListBox_Lbx.Items.Count - 1 >= payorIndex ?
                    payorIndex :
                    UI_PayorListBox_Lbx.Items.Count - 1)
                : -1;
                _selectedPayor = UI_PayorListBox_Lbx.SelectedItem;
            }
        }



        /// <summary>
        /// Update the button enabled states
        /// </summary>
        public void UpdateButtonStates()
        {
            if (_vm.Payors.Count == 0)
            {
                UI_EditPayor_Btn.IsEnabled = false;
                UI_DeletePayor_Btn.IsEnabled = false;
                UI_ViewPayorBalance_Btn.IsEnabled = false;
            }
            else
            {
                UI_EditPayor_Btn.IsEnabled = true;
                UI_DeletePayor_Btn.IsEnabled = true;
                UI_ViewPayorBalance_Btn.IsEnabled = true;
            }
        }
    }
}
