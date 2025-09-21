//***********************************************************************************
//Program: RowEntry.cs
//Description: Row entry
//Date: Sept 18, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Services.Database;

namespace PayorLedger.Models
{
    public class RowEntry : IDatabaseAction
    {
        /// <summary>
        /// Label of the row
        /// </summary>
        public RowLabel Label { get; set; }



        /// <summary>
        /// Date of row
        /// </summary>
        public string Date { get; set; }



        /// <summary>
        /// Or # of row
        /// </summary>
        public int OrNum { get; set; }



        /// <summary>
        /// Id of the payor for this row
        /// </summary>
        public long PayorId { get; set; }



        /// <summary>
        /// Month of this row
        /// </summary>
        public Month Month { get; }



        /// <summary>
        /// Year of this row
        /// </summary>
        public int Year { get; }



        /// <summary>
        /// Comment for this row
        /// </summary>
        public string Comment { get; set; }



        /// <summary>
        /// Cell entries in this row
        /// </summary>
        public List<CellEntryToRow> CellEntries { get; set; }



        /// <summary>
        /// State of the object in relation to the database
        /// </summary>
        public ChangeState State { get; set; }



        public RowEntry(RowLabel label, string date, int orNum, long payorId, Month month, int year, string comment, ChangeState state, List<CellEntryToRow> cellEntries)
        {
            Label = label;
            Date = date;
            OrNum = orNum;
            PayorId = payorId;
            Month = month;
            Year = year;
            Comment = comment;
            State = state;
            CellEntries = cellEntries;
        }



        public enum RowLabel
        {
            Depositor,
            Borrower,
            ShareHolder,
            Other
        }
    }
}
