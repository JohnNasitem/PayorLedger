//***********************************************************************************
//Program: EditPayorCommand.cs
//Description: Edit payor command
//Date: Sep 15, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Enums;
using PayorLedger.Models;

namespace PayorLedger.Services.Actions.PayorCommands
{
    public class EditPayorCommand : PayorCommand
    {
        private string _originalName;
        private string _newName;
        private PayorEnums.PayorLabel _originalLabel;
        private PayorEnums.PayorLabel _newLabel;



        public EditPayorCommand(PayorEntry payor, string newName, PayorEnums.PayorLabel newLabel) : base(payor)
        {
            _originalLabel = payor.Label;
            _originalName = payor.PayorName;
            _newLabel = newLabel;
            _newName = newName;
        }



        /// <summary>
        /// Execute the command to edit a payor
        /// </summary>
        public override void Execute() => EditPayor(_newName, _newLabel);



        /// <summary>
        /// Undo the command to edit a payor
        /// </summary>
        public override void Undo() => EditPayor(_originalName, _originalLabel);
    }
}
