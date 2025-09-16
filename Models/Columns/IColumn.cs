//***********************************************************************************
//Program: IColumn.cs
//Description: Interface for a column
//Date: May 22, 2025
//Author: John Nasitem
//***********************************************************************************



namespace PayorLedger.Models.Columns
{
    public interface IColumn
    {
        /// <summary>
        /// Column Id
        /// </summary>
        public long Id { get; }



        /// <summary>
        /// Column Name
        /// </summary>
        public string Name { get; }



        /// <summary>
        /// Column Order
        /// </summary>
        public int Order { get; }
    }
}
