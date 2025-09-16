//***********************************************************************************
//Program: EditCellCommand.cs
//Description: Edit cell command
//Date: July 16, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;

namespace PayorLedger.Services.Actions.CellCommands
{
    internal class EditCellCommand : CellCommand
    {
        private decimal _originalAmount;
        private decimal _newAmount;



        public EditCellCommand(PayorToColumnEntry cellEntry, decimal newAmount) : base(cellEntry)
        {
            _originalAmount = cellEntry.Amount;
            _newAmount = newAmount;
        }



        /// <summary>
        /// Edit the cell on the main page
        /// </summary>
        public override void Execute() => EditCell(_newAmount);



        /// <summary>
        /// Revert the cell to its original amount on the main page
        /// </summary>
        public override void Undo() => EditCell(_originalAmount);
    }
}
