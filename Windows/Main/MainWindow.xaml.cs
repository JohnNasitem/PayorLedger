//***********************************************************************************
//Program: MainWindow.cs
//Description: Host pages
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Dialogs;
using PayorLedger.Pages;
using PayorLedger.Services.Actions;
using PayorLedger.Services.Database;
using PayorLedger.ViewModels;
using System.Windows;

namespace PayorLedger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainPage mainPage = App.ServiceProvider.GetRequiredService<MainPageViewModel>().Page;
            mainPage.RegisterMenuShortcuts(this);

            // Load main page
            MainFrame.Navigate(mainPage);
        }


        // Close the payor window when this window closes
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IDatabaseService dbService = App.ServiceProvider.GetRequiredService<IDatabaseService>();

            if (!dbService.AllChangesSaved)
            {
                UnsavedChangesDialog dlg = new();
                dlg.ShowDialog();

                // Cancel exit
                if (!dlg.Exit && !dlg.Save)
                {
                    e.Cancel = true;
                    return;
                }

                if (dlg.Save)
                    dbService.SaveChanges();
            }

            App.ServiceProvider.GetRequiredService<PayorWindowViewModel>().Exit();
            App.ServiceProvider.GetRequiredService<ColumnsWindowViewModel>().Exit();
        }
    }
}