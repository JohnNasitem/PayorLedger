//***********************************************************************************
//Program: AddCellCommand.cs
//Description: Add cell command
//Date: Sep 15, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;

namespace PayorLedger.Services.Actions.CellCommands
{
    internal class AddCellCommand : CellCommand
    {
        public AddCellCommand(CellEntryToRow cellEntry) : base(cellEntry) { }



        /// <summary>
        /// Add the cell to the main page
        /// </summary>
        public override void Execute() => AddCell();



        /// <summary>
        /// Remove the cell from the main page
        /// </summary>
        public override void Undo() => DeleteCell();
    }
}
