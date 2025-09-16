//***********************************************************************************
//Program: PayorsWindow.cs
//Description: Payors window code-behind
//Date: Aug 21, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.ViewModels;
using System.Windows;
namespace PayorLedger.Windows.Payors
{
    /// <summary>
    /// Interaction logic for PayorsWindow.xaml
    /// </summary>
    public partial class PayorsWindow : Window
    {
        private readonly PayorWindowViewModel _vm;



        public PayorsWindow(PayorWindowViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
        }



        /// <summary>
        /// Open the manage payors page
        /// </summary>
        public void OpenManagePayorsPage()
        {
            PayorFrame.Navigate(App.ServiceProvider.GetRequiredService<ManagePayorsViewModel>().Page);
        }



        /// <summary>
        /// Open the view payor page
        /// </summary>
        /// <returns>View payor view model</returns>
        public ViewPayorViewModel OpenViewPayorPage()
        {
            ViewPayorViewModel viewPayorViewModel = App.ServiceProvider.GetRequiredService<ViewPayorViewModel>();
            PayorFrame.Navigate(viewPayorViewModel.Page);
            return viewPayorViewModel;
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
    }
}
