//***********************************************************************************
//Program: AddCellCommand.cs
//Description: Delete cell command
//Date: Sep 15, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;

namespace PayorLedger.Services.Actions.CellCommands
{
    internal class DeleteCellCommand : CellCommand
    {
        public DeleteCellCommand(PayorToColumnEntry cellEntry) : base(cellEntry) { }



        /// <summary>
        /// Delete the cell from the main page
        /// </summary>
        public override void Execute() => DeleteCell();



        /// <summary>
        /// Add the cell back to the main page
        /// </summary>
        public override void Undo() => AddCell();
    }
}
