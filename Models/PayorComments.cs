//***********************************************************************************
//Program: PayorComments.cs
//Description: Entry in the PayorComments database table
//Date: July 24, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Services.Database;

namespace PayorLedger.Models
{
    public class PayorComments : IDatabaseAction
    {
        /// <summary>
        /// Id of the payor
        /// </summary>
        public long PayorId { get; set; }



        /// <summary>
        /// Payor note
        /// </summary>
        public string Comment { get; set; }



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



        public PayorComments(long payorId, string comment, Month month, int year, ChangeState state)
        {
            PayorId = payorId;
            Comment = comment;
            Month = month;
            Year = year;
            State = state;
        }
    }
}
