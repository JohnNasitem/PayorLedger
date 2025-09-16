//***********************************************************************************
//Program: IDatabaseAction.cs
//Description: Interface for database actions
//Date: Sep 15, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Services.Database;

namespace PayorLedger.Models
{
    public interface IDatabaseAction
    {
        /// <summary>
        /// State of the object in relation to the database
        /// </summary>
        public ChangeState State { get; set; }
    }
}
