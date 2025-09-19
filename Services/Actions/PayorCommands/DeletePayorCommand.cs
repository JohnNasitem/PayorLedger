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
        private List<RowEntry> _payorRows = [];
        private Dictionary<RowEntry, ChangeState> _originalStates = [];



        public DeletePayorCommand(PayorEntry payor) : base(payor) { }



        /// <summary>
        /// Execute the command to remove a payor
        /// </summary>
        public override void Execute()
        {
            // Get all the rows associated with the payor
            _payorRows.Clear();
            _originalStates.Clear();
            _payorRows = _mainPageVM.LedgerRows.Where(e => e.PayorId == _payor.PayorId).ToList();

            // Remove rows from row list
            foreach (RowEntry row in _payorRows)
            {
                _originalStates.Add(row, row.State);
                row.State = ChangeState.Removed;
            }

            DeletePayor();
        }



        /// <summary>
        /// Undo the command to remove a payor
        /// </summary>
        public override void Undo()
        {
            // Add back removed entries
            foreach (RowEntry row in _payorRows)
                row.State = _originalStates[row];

            AddPayor();
        }
    }
}
