//***********************************************************************************
//Program: AddPayorCommand.cs
//Description: Add payor command
//Date: Sep 9, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;

namespace PayorLedger.Services.Actions.PayorCommands
{
    public class AddPayorCommand : PayorCommand
    {
        public AddPayorCommand(PayorEntry payor) : base(payor) { }


        /// <summary>
        /// Execute the command to add a payor
        /// </summary>
        public override void Execute() => AddPayor();



        /// <summary>
        /// Undo the command to add a payor
        /// </summary>
        public override void Undo() => DeletePayor();
    }
}
