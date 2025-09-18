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
        /// Id of the column
        /// </summary>
        public long SubheaderId { get; set; }



        /// <summary>
        /// Id of the payor
        /// </summary>
        public long PayorId { get; set; }



        /// <summary>
        /// Amount owed
        /// </summary>
        public decimal Amount { get; set; }



        /// <summary>
        /// Month of the entry
        /// </summary>
        public Month Month { get; }



        /// <summary>
        /// Year of the entry
        /// </summary>
        public int Year { get; }



        /// <summary>
        /// State of the object in relation to the database
        /// </summary>
        public ChangeState State { get; set; }



        public CellEntryToRow(long subheaderId, long payorId, decimal amount, Month month, int year, ChangeState state)
        {
            SubheaderId = subheaderId;
            PayorId = payorId;
            Amount = amount;
            Month = month;
            Year = year;
            State = state;
        }



        public CellEntryToRow(CellEntryToRow instanceToClone)
        {
            SubheaderId = instanceToClone.SubheaderId;
            PayorId = instanceToClone.PayorId;
            Amount = instanceToClone.Amount;
            Month = instanceToClone.Month;
            Year = instanceToClone.Year;
            State = instanceToClone.State;
        }
    }
}
