//***********************************************************************************
//Program: UndoRedoService.cs
//Description: Manage actions
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Services.Database;

namespace PayorLedger.Services.Actions
{
    public class UndoRedoService : IUndoRedoService
    {
        public event EventHandler? ChangeOccured;

        private readonly Stack<IUndoableCommand> _undoStack = [];
        private readonly Stack<IUndoableCommand> _redoStack = [];

        private static readonly IDatabaseService _dbService = App.ServiceProvider.GetRequiredService<IDatabaseService>();



        /// <summary>
        /// Is an undo available
        /// </summary>
        public bool CanUndo { get { return _undoStack.Count > 0; } }



        /// <summary>
        /// Is a redo available
        /// </summary>
        public bool CanRedo { get { return _redoStack.Count > 0; } }



        /// <summary>
        /// Is there any unsaved changes
        /// </summary>
        public bool UnsavedChanges { get; set; }
        


        /// <summary>
        /// Execute a command
        /// </summary>
        /// <param name="command"></param>
        public void Execute(IUndoableCommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
            OnChangeOccured(false);
        }



        /// <summary>
        /// Redo a command
        /// </summary>
        public void Redo()
        {
            if (_redoStack.TryPop(out var command))
            {
                command.Execute();
                _undoStack.Push(command);
                OnChangeOccured(false);
            }
        }



        /// <summary>
        /// Undo a command
        /// </summary>
        public void Undo()
        {
            if (_undoStack.TryPop(out var command))
            {
                command.Undo();
                _redoStack.Push(command);
                OnChangeOccured(false);
            }
        }



        /// <summary>
        /// Execute, redo, or undo occured
        /// </summary>
        public void OnChangeOccured(bool allChangesSaved)
        {
            _dbService.AllChangesSaved = allChangesSaved;
            ChangeOccured?.Invoke(this, EventArgs.Empty);
        }
    }
}
