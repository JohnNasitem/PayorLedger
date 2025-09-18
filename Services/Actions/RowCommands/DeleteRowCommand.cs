//***********************************************************************************
//Program: DeleteRowCommand.cs
//Description: Add row command
//Date: Sep 18, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;

namespace PayorLedger.Services.Actions.RowCommands
{
    public class DeleteRowCommand : RowCommand
    {
        public DeleteRowCommand(RowEntry row) : base(row) { }


        /// <summary>
        /// Execute the command to add a row
        /// </summary>
        public override void Execute() => DeleteRow();



        /// <summary>
        /// Undo the command to add a row
        /// </summary>
        public override void Undo() => AddRow();
    }
}
