//***********************************************************************************
//Program: CellEntryToRow.cs
//Description: Entry in the CellEntryToRow database table
//Date: May 5, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Services.Database;

namespace PayorLedger.Models
{
    public class CellEntryToRow : IDatabaseAction
    {
        /// <summary>
        /// Row this cell entry belongs to
        /// </summary>
        public RowEntry Row { get; set; }



        /// <summary>
        /// Id of the column
        /// </summary>
        public long SubheaderId { get; set; }



        /// <summary>
        /// Amount owed
        /// </summary>
        public decimal Amount { get; set; }



        /// <summary>
        /// State of the object in relation to the database
        /// </summary>
        public ChangeState State { get; set; }



        public CellEntryToRow(RowEntry row, long subheaderId, decimal amount, ChangeState state)
        {
            Row = row;
            SubheaderId = subheaderId;
            Amount = amount;
            State = state;
        }



        public CellEntryToRow(CellEntryToRow instanceToClone)
        {
            SubheaderId = instanceToClone.SubheaderId;
            Row = instanceToClone.Row;
            Amount = instanceToClone.Amount;
            State = instanceToClone.State;
        }
    }
}
