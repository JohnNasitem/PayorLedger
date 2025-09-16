//***********************************************************************************
//Program: DatabaseService.cs
//Description: Tab content for a month
//Date: May 5, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Services.Database;
using System.Data;

namespace PayorLedger.Models
{
    public class MonthTab
    {
        /// <summary>
        /// Month the tab is representing
        /// </summary>
        public Month Month { get; }



        /// <summary>
        /// First 3 letters of the month
        /// </summary>
        public string Name { get; }



        /// <summary>
        /// Tab contents
        /// </summary>
        public DataTable Content { get; set; } = new();



        public MonthTab(Month month)
        {
            Month = month;
            string monthName = Enum.GetName(month)!.Substring(0, 3);
            Name = monthName == "All" ? "Year Total" : monthName;
        }
    }
}
