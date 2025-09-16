//***********************************************************************************
//Program: EditSubheaderCommand.cs
//Description: Edit subheader command
//Date: Sep 9, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models.Columns;

namespace PayorLedger.Services.Actions.SubheaderCommands
{
    public class EditSubheaderCommand : SubheaderCommand
    {
        private readonly string _originalName;
        private readonly string _newName;
        private readonly HeaderEntry _originalParentHeader;
        private readonly HeaderEntry _newParentHeader;
        private readonly int _originalOrder;
        private readonly int _newOrder;



        public EditSubheaderCommand(SubheaderEntry subheader, string newName, HeaderEntry newParentHeader, int newOrder) : base(subheader)
        {
            _originalName = subheader.Name;
            _newName = newName;
            _originalParentHeader = subheader.Header;
            _newParentHeader = newParentHeader;
            _originalOrder = subheader.Order;
            _newOrder = newOrder;
        }



        /// <summary>
        /// Change the parent header of the subheader in the main page
        /// </summary>
        public override void Execute() => EditSubheader(_newParentHeader, _newName, _newOrder);



        /// <summary>
        /// Undo the command to change the parent header
        /// </summary>
        public override void Undo() => EditSubheader(_originalParentHeader, _originalName, _originalOrder);
    }
}
