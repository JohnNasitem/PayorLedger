//***********************************************************************************
//Program: WindowViewModel.cs
//Description: Base class for window view models
//Date: Sep 4, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Windows;

namespace PayorLedger.ViewModels
{
    public abstract class WindowViewModel
    {
        /// <summary>
        /// Is the main application exiting
        /// </summary>
        public bool ApplicationExit { get; protected set; }



        /// <summary>
        /// Window associated with the view model
        /// </summary>
        public abstract Window Window { get; protected set; }



        /// <summary>
        /// Closes the window
        /// </summary>
        public virtual void Exit()
        {
            ApplicationExit = true;
            Window.Close();
        }



        /// <summary>
        /// Opens the window to the main page
        /// </summary>
        public virtual void OpenWindow()
        {
            Window.Show();
            Window.Activate();
        }
    }
}
