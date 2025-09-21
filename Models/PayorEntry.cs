//***********************************************************************************
//Program: PayorEntry.cs
//Description: Entry in the Payor database table
//Date: May 5, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Services.Database;

namespace PayorLedger.Models
{
    public class PayorEntry : IDatabaseAction
    {
        private static int _tempIdIncr = -1;



        /// <summary>
        /// Id of the payor
        /// </summary>
        public long PayorId { get; set; }



        /// <summary>
        /// Name of the payor
        /// </summary>
        public string PayorName { get; set; }



        /// <summary>
        /// State of the object in relation to the database
        /// </summary>
        public ChangeState State { get; set; }



        public PayorEntry(long payorId, string payorName, ChangeState state)
        {
            if (payorId == -1)
            {
                PayorId = _tempIdIncr;
                _tempIdIncr--;
            }
            else
                PayorId = payorId;

            PayorName = payorName;
            State = state;
        }
    }
}
