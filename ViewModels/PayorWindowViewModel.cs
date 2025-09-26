//***********************************************************************************
//Program: PayorWindowViewModel.cs
//Description: View model for payors window
//Date: Aug 21, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.Services.Database;
using PayorLedger.Windows.Payors;
using System.Windows;

namespace PayorLedger.ViewModels
{
    public class PayorWindowViewModel : WindowViewModel
    {
        /// <summary>
        /// View model of the active view payor page. Null if no page is active
        /// </summary>
        public ViewPayorViewModel? ActiveViewPayorViewModel { get; set; } = null;



        /// <summary>
        /// Window the view model is associated to
        /// </summary>
        public override Window Window { get; protected set; }



        public PayorWindowViewModel()
        {
            Window = new PayorsWindow(this);
        }



        /// <summary>
        /// Opens the payors window and navigates to the Manage Payors page.
        /// </summary>
        public override void OpenWindow()
        {
            ((PayorsWindow)Window).OpenManagePayorsPage();
            base.OpenWindow();
        }



        /// <summary>
        /// Opens the payors window and navigates to the View Payor page.
        /// </summary>
        /// <param name="payorToView">Payor to view</param>
        public void ViewPayor(string payorName)
        {
            PayorEntry? payor = App.ServiceProvider.GetRequiredService<MainPageViewModel>().Payors.Where(p => p.State != ChangeState.Removed).FirstOrDefault(p => p.PayorName == payorName);
            if (payor == null)
            {
                MessageBox.Show("Problem occured when trying to view this payor!", "Error");
                return;
            }

            ActiveViewPayorViewModel = ((PayorsWindow)Window).OpenViewPayorPage();
            ActiveViewPayorViewModel.SetPayor(payor);
            ActiveViewPayorViewModel.Page.RefreshTabs();
            Window.Show();
            Window.Activate();
        }
    }
}
