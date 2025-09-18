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
        /// Date of row entry
        /// </summary>
        public string Date { get; set; }



        /// <summary>
        /// Or # of row entry
        /// </summary>
        public int OrNum { get; set; }



        /// <summary>
        /// Id of the payor
        /// </summary>
        public long PayorId { get; set; }



        /// <summary>
        /// Month of the entry
        /// </summary>
        public Month Month { get; }



        /// <summary>
        /// Year of the entry
        /// </summary>
        public int Year { get; }



        /// <summary>
        /// Cell entries in this row
        /// </summary>
        public List<CellEntryToRow> CellEntries { get; set; }   



        /// <summary>
        /// State of the object in relation to the database
        /// </summary>
        public ChangeState State { get; set; }



        public RowEntry(string date, int orNum, long payorId, Month month, int year, ChangeState state, List<CellEntryToRow> cellEntries)
        {
            Date = date;
            OrNum = orNum;
            PayorId = payorId;
            Month = month;
            Year = year;
            State = state;
            CellEntries = cellEntries;
        }
    }
}
