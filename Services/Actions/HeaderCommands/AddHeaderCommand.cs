//***********************************************************************************
//Program: AddHeaderCommand.cs
//Description: Add header command
//Date: Sep 9, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models.Columns;

namespace PayorLedger.Services.Actions.HeaderCommands
{
    public class AddHeaderCommand : HeaderCommand
    {
        public AddHeaderCommand(HeaderEntry header) : base(header) { }



        /// <summary>
        /// Add the header to the main page
        /// </summary>
        public override void Execute() => AddHeader();



        /// <summary>
        /// Undo the command to add the header
        /// </summary>
        public override void Undo() => DeleteHeader();
    }
}
