//***********************************************************************************
//Program: ViewPayor.cs
//Description: View payors page code-behind
//Date: Aug 21, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.ViewModels;
using System.Windows.Controls;

namespace PayorLedger.Windows.Payors.Pages
{
    /// <summary>
    /// Interaction logic for ViewPayor.xaml
    /// </summary>
    public partial class ViewPayor : Page
    {
        public ViewPayorViewModel _vm;

        public ViewPayor(ViewPayorViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            _vm = vm;
            _vm.SetPayorName = SetPayorName;
        }



        /// <summary>
        /// Set the name of the payor
        /// </summary>
        /// <param name="payor">Payor</param>
        private void SetPayorName(PayorEntry payor)
        {
            UI_PayorName_Tbk.Text = $"{payor.PayorName} - {payor.Label}";
        }



        /// <summary>
        /// Clear the active instance in the PayorWindowViewModel
        /// </summary>
        /// <param name="sender">Page</param>
        /// <param name="e">Event args</param>
        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            PayorWindowViewModel vm = App.ServiceProvider.GetRequiredService<PayorWindowViewModel>();

            // Only clear if the instance 
            if (_vm == vm.ActiveViewPayorViewModel)
                vm.ActiveViewPayorViewModel = null;
        }



        /// <summary>
        /// Go back to manage payors page
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Event args</param>
        private void UI_Back_Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ServiceProvider.GetRequiredService<PayorWindowViewModel>().OpenWindow();
        }
    }
}
