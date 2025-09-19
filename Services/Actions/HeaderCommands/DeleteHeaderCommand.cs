//***********************************************************************************
//Program: DeleteHeaderCommand.cs
//Description: Delete header command
//Date: Sep 9, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Database;

namespace PayorLedger.Services.Actions.HeaderCommands
{
    internal class DeleteHeaderCommand : HeaderCommand
    {
        private List<CellEntryToRow> _headerEntries = [];
        private Dictionary<CellEntryToRow, ChangeState> _originalStates = [];



        public DeleteHeaderCommand(HeaderEntry header) : base(header) { }



        /// <summary>
        /// Delete the header from the main page
        /// </summary>
        public override void Execute()
        {
            // Get all the cell entries related to the header
            _headerEntries.Clear();
            _originalStates.Clear();
            _headerEntries.AddRange(_mainPageVM.LedgerRows.SelectMany(r => r.CellEntries).Where(e => Header.Subheaders.Select(s => s.Id).Contains(e.SubheaderId)));

            // Remove cell entries related to the subheader
            foreach (CellEntryToRow entry in _headerEntries)
            {
                _originalStates.Add(entry, entry.State);
                entry.State = ChangeState.Removed;
            }

            DeleteHeader();
        }



        /// <summary>
        /// Undo the command to delete the header
        /// </summary>
        public override void Undo()
        {
            // Add back removed entries
            foreach (CellEntryToRow entry in _headerEntries)
                entry.State = _originalStates[entry];

            AddHeader();
        }
    }
}
