//***********************************************************************************
//Program: DeleteSubheaderCommand.cs
//Description: Delete subheader command
//Date: Sep 9, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Database;

namespace PayorLedger.Services.Actions.SubheaderCommands
{
    public class DeleteSubheaderCommand : SubheaderCommand
    {
        private List<CellEntryToRow> _subheaderEntries = [];
        private Dictionary<CellEntryToRow, ChangeState> _originalStates = [];


        public DeleteSubheaderCommand(SubheaderEntry subheader) : base(subheader) { }



        /// <summary>
        /// Remove the subheader from its parent header in the main page
        /// </summary>
        public override void Execute()
        {
            // Get all the cell entries related to the subheader
            _subheaderEntries.Clear();
            _originalStates.Clear();
            _subheaderEntries.AddRange(_mainPageVM.LedgerEntries.Where(e => e.SubheaderId == Subheader.Id));

            // Remove cell entries related to the subheader
            foreach (CellEntryToRow entry in _subheaderEntries)
            {
                _originalStates.Add(entry, entry.State);
                entry.State = ChangeState.Removed;
            }

            DeleteSubheader();
        }



        /// <summary>
        /// Undo the command to remove the subheader
        /// </summary>
        public override void Undo()
        {
            // Add back removed entries
            foreach (CellEntryToRow entry in _subheaderEntries)
                entry.State = _originalStates[entry];

            AddSubheader();
        }
    }
}
