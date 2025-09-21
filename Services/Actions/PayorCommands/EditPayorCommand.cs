//***********************************************************************************
//Program: EditPayorCommand.cs
//Description: Edit payor command
//Date: Sep 15, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;

namespace PayorLedger.Services.Actions.PayorCommands
{
    public class EditPayorCommand : PayorCommand
    {
        private string _originalName;
        private string _newName;



        public EditPayorCommand(PayorEntry payor, string newName) : base(payor)
        {
            _originalName = payor.PayorName;
            _newName = newName;
        }



        /// <summary>
        /// Execute the command to edit a payor
        /// </summary>
        public override void Execute() => EditPayor(_newName);



        /// <summary>
        /// Undo the command to edit a payor
        /// </summary>
        public override void Undo() => EditPayor(_originalName);
    }
}
