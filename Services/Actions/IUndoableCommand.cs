//***********************************************************************************
//Program: IUndoableCommand.cs
//Description: Command that can be undone
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Services.Database;

namespace PayorLedger.Services.Actions
{
    public interface IUndoableCommand
    {
        /// <summary>
        /// Execute command
        /// </summary>
        public void Execute();


        /// <summary>
        /// Undo command
        /// </summary>
        public void Undo();
    }
}
