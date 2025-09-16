//***********************************************************************************
//Program: PayorEntry.cs
//Description: Entry in the Payor database table
//Date: May 5, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Enums;
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
        /// Payor label
        /// </summary>
        public PayorEnums.PayorLabel Label { get; set; }



        /// <summary>
        /// State of the object in relation to the database
        /// </summary>
        public ChangeState State { get; set; }



        public PayorEntry(long payorId, string payorName, PayorEnums.PayorLabel label, ChangeState state)
        {
            if (payorId == -1)
            {
                PayorId = _tempIdIncr;
                _tempIdIncr--;
            }
            else
                PayorId = payorId;

            PayorName = payorName;
            Label = label;
            State = state;
        }
    }
}
