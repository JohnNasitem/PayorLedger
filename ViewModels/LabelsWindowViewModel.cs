//***********************************************************************************
//Program: LabelsWindowViewModel.cs
//Description: View model for labels window
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Windows.Columns;
using PayorLedger.Windows.Labels;
using System.Windows;

namespace PayorLedger.ViewModels
{
    public class LabelsWindowViewModel : WindowViewModel
    {
        /// <summary>
        /// Window the view model is associated to
        /// </summary>
        public override Window Window { get; protected set; }



        public LabelsWindowViewModel()
        {
            Window = new LabelsWindow(this);
        }
    }
}
