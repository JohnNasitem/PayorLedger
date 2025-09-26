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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
