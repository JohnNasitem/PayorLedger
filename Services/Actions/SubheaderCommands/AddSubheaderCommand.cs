//***********************************************************************************
//Program: AddSubheaderCommand.cs
//Description: Add subheader command
//Date: Sep 9, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models.Columns;

namespace PayorLedger.Services.Actions.SubheaderCommands
{
    public class AddSubheaderCommand : SubheaderCommand
    {
        public AddSubheaderCommand(SubheaderEntry subheader) : base(subheader) { }



        /// <summary>
        /// Add the subheader to its parent header in the main page
        /// </summary>
        public override void Execute() => AddSubheader();



        /// <summary>
        /// Undo the command to add the subheader
        /// </summary>
        public override void Undo() => DeleteSubheader();
    }
}
