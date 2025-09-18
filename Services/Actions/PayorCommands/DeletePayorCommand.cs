//***********************************************************************************
//Program: DeletePayorCommand.cs
//Description: Delete payor command
//Date: Aug 30, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;
using PayorLedger.Services.Database;

namespace PayorLedger.Services.Actions.PayorCommands
{
    public class DeletePayorCommand : PayorCommand
    {
        private List<CellEntryToRow> _payorEntries = [];
        private Dictionary<CellEntryToRow, ChangeState> _originalStates = [];



        public DeletePayorCommand(PayorEntry payor) : base(payor) { }



        /// <summary>
        /// Execute the command to remove a payor
        /// </summary>
        public override void Execute()
        {
            // Get all the entries associated with the payor
            _payorEntries.Clear();
            _originalStates.Clear();
            _payorEntries = _mainPageVM.LedgerEntries.Where(e => e.PayorId == _payor.PayorId).ToList();

            // Remove entries from entry list
            foreach (CellEntryToRow entry in _payorEntries)
            {
                _originalStates.Add(entry, entry.State);
                entry.State = ChangeState.Removed;
            }

            DeletePayor();
        }



        /// <summary>
        /// Undo the command to remove a payor
        /// </summary>
        public override void Undo()
        {
            // Add back removed entries
            foreach (CellEntryToRow entry in _payorEntries)
                entry.State = _originalStates[entry];

            AddPayor();
        }
    }
}
