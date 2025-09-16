//***********************************************************************************
//Program: EditHeaderCommand.cs
//Description: Edit header command
//Date: Sep 9, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models.Columns;

namespace PayorLedger.Services.Actions.HeaderCommands
{
    public class EditHeaderCommand : HeaderCommand
    {
        private string _originalName;
        private string _newName;
        private int _originalOrder;
        private int _newOrder;



        public EditHeaderCommand(HeaderEntry header, string newName, int newOrder) : base(header)
        {
            _originalName = header.Name;
            _newName = newName;
            _originalOrder = header.Order;
            _newOrder = newOrder;
        }



        /// <summary>
        /// Edit the header in the main page
        /// </summary>
        public override void Execute() => EditHeader(_newName, _newOrder);



        /// <summary>
        /// Undo the command to edit the header
        /// </summary>
        public override void Undo() => EditHeader(_originalName, _originalOrder);
    }
}
