//***********************************************************************************
//Program: TotalTab.cs
//Description: Tab content for total type
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Data;



namespace PayorLedger.Models
{
    public class TotalTab
    {
        /// <summary>
        /// Name of the total type
        /// </summary>
        public string Name { get ; set; }



        /// <summary>
        /// Datatable of the tab
        /// </summary>
        public DataTable Content { get; set; } = new();



        public TotalTab(RowEntry.RowLabel type)
        {
            Name = Enum.GetName(type)!;

            if (Name.StartsWith("Depositor"))
                Name = Name.Insert("Depositor".Length - 1, " ");
        }
    }
}
