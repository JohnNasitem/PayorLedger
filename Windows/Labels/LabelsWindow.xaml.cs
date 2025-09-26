//***********************************************************************************
//Program: LabelsWindow.cs
//Description: Labels window code-behind
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.ViewModels;
using System.Windows;

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



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

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
