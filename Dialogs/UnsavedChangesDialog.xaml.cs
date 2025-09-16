//***********************************************************************************
//Program: UnsavedChangesDialog.cs
//Description: Unsaved Changes dialog code
//Date: Sep 3, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Windows;

namespace PayorLedger.Dialogs
{
    /// <summary>
    /// Interaction logic for UnsavedChangesDialog.xaml
    /// </summary>
    public partial class UnsavedChangesDialog : Window
    {
        /// <summary>
        /// Does the user want to save before exiting?
        /// </summary>
        public bool Save { get; private set; } = false;



        /// <summary>
        /// Does the user want to exit?
        /// </summary>
        public bool Exit { get; private set; } = false;




        public UnsavedChangesDialog()
        {
            InitializeComponent();
        }



        /// <summary>
        /// Handler for buttons
        /// </summary>
        /// <param name="sender">object that called the code</param>
        /// <param name="e">event args</param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == UI_Save_Btn)
            {
                Save = true;
                Exit = true;
                DialogResult = true;
            }
            else if (sender == UI_Exit_Btn)
            {
                Exit = true;
                DialogResult = true;
            }
            else if (sender == UI_Cancel_Btn)
                DialogResult = false;
        }
    }
}
