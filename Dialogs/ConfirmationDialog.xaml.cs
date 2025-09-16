//***********************************************************************************
//Program: ConfirmationDialog.cs
//Description: Confirmation dialog code
//Date: June 2, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Windows;
using System.Windows.Media;

namespace PayorLedger.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : Window
    {
        /// <summary>
        /// Initialize a new instance of <see cref="ConfirmationDialog"/> class.
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="content">Dialog content</param>
        public ConfirmationDialog(string title, string content, Brush? yesColor = null, Brush? noColor = null)
        {
            InitializeComponent();
            // Set title
            Title = title;
            UI_Title_Lbl.Content = title;

            // Set content
            UI_Content_Tbk.Text = content;

            // Set background colours
            UI_Yes_Btn.Background = yesColor ?? SystemColors.ControlBrush;
            UI_No_Btn.Background = noColor ?? SystemColors.ControlBrush;
        }



        /// <summary>
        /// Handler for both buttons
        /// </summary>
        /// <param name="sender">object that called the code</param>
        /// <param name="e">event args</param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = sender == UI_Yes_Btn;
        }
    }
}
