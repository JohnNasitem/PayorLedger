//***********************************************************************************
//Program: IDatabaseService.cs
//Description: Interface to interact with database
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Enums;
using PayorLedger.Models;
using PayorLedger.Models.Columns;

namespace PayorLedger.Services.Database
{
    public interface IDatabaseService
    {
        /// <summary>
        /// List of names that are not allowed to be used for payors, headers, or subheaders
        /// </summary>
        public string[] InvalidNames { get; }



        /// <summary>
        /// Are all chnages saved
        /// </summary>
        public bool AllChangesSaved { get; set; }



        /// <summary>
        /// Initialize database tables if they don't exist
        /// </summary>
        public void InitializeTables();



        /// <summary>
        /// Sync changes to the database
        /// </summary>
        public void SaveChanges();



        /// <summary>
        /// Get all the entries
        /// </summary>>
        /// <returns>Data for the specified year</returns>
        public (List<CellEntryToRow> Entries, List<PayorComments> Comments) GetData();



        /// <summary>
        /// Get the headers
        /// </summary>
        /// <returns>List of headers</returns>
        public List<HeaderEntry> GetHeaders();



        /// <summary>
        /// Get the payors
        /// </summary>
        /// <returns>List of payors</returns>
        public List<PayorEntry> GetPayors();
    }
}
