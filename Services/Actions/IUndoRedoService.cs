//***********************************************************************************
//Program: IUndoRedoService.cs
//Description: Interface to implement undo and redo functionality
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************



namespace PayorLedger.Services.Actions
{
    public interface IUndoRedoService
    {
        /// <summary>
        /// Fires when a execute, undo, or redo occurs
        /// </summary>
        public event EventHandler? ChangeOccured;



        /// <summary>
        /// Execute a command
        /// </summary>
        public void Execute(IUndoableCommand command);



        /// <summary>
        /// Undo a command
        /// </summary>
        public void Undo();



        /// <summary>
        /// Redo a command
        /// </summary>
        public void Redo();



        /// <summary>
        /// Is a undo available
        /// </summary>
        public bool CanUndo { get; }



        /// <summary>
        /// Is a redo available
        /// </summary>
        public bool CanRedo { get; }



        /// <summary>
        /// Execute, redo, or undo occured
        /// </summary>
        public void OnChangeOccured(bool allChangesSaved);
    }
}
