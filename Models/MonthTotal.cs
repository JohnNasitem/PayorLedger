//***********************************************************************************
//Program: MonthTotal.cs
//Description: Totals for the month
//Date: Aug 25, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Services.Database;

namespace PayorLedger.Models
{
    public class MonthTotal
    {
        /// <summary>
        /// Year of the total
        /// </summary>
        public int Year { get; }



        /// <summary>
        /// Month of the total
        /// </summary>
        public Month Month { get; }



        private Dictionary<int, decimal> rowTotals;
        private Dictionary<long, decimal> columnTotals;



        public MonthTotal(int year, Month month)
        {
            Year = year;
            Month = month;
            rowTotals = [];
            columnTotals = [];
        }



        /// <summary>
        /// Create a month total from a list of month totals (for aggregation)
        /// </summary>
        /// <param name="totals">List of <see cref="MonthTotal"/></param>
        public MonthTotal(int year, Month month, List<MonthTotal> totals) :
            this(year, month)
        {
            foreach (MonthTotal monthTotal in totals)
            {
                foreach (var kvp in monthTotal.rowTotals)
                    AddToRowTotal(kvp.Key, kvp.Value);
                foreach (var kvp in monthTotal.columnTotals)
                    AddToColumnTotal(kvp.Key, kvp.Value);
            }
        }



        /// <summary>
        /// Add to the total for a specific payor
        /// </summary>
        /// <param name="orNum">Or # of the row</param>
        /// <param name="amount">Amount to add</param>
        public void AddToRowTotal(int orNum, decimal amount)
        {
            if (!rowTotals.ContainsKey(orNum))
                rowTotals[orNum] = 0;
            rowTotals[orNum] += amount;
        }



        /// <summary>
        /// Add to the total for a specific column
        /// </summary>
        /// <param name="columnId">Id of column to add to</param>
        /// <param name="amount">Amount to add</param>
        public void AddToColumnTotal(long columnId, decimal amount)
        {
            if (!columnTotals.ContainsKey(columnId))
                columnTotals[columnId] = 0;
            columnTotals[columnId] += amount;
        }



        /// <summary>
        /// Get the total for a specific row
        /// </summary>
        /// <param name="orNum">Or # of the row</param>
        /// <returns>Total</returns>
        public decimal GetRowTotal(int orNum)
        {
            if (rowTotals.TryGetValue(orNum, out decimal total))
                return total;
            return 0;
        }



        /// <summary>
        /// Get the total for a specific column
        /// </summary>
        /// <param name="columnId">Id of column</param>
        /// <returns>Total</returns>
        public decimal GetColumnTotal(long columnId)
        {
            if (columnTotals.TryGetValue(columnId, out decimal total))
                return total;
            return 0;
        }



        /// <summary>
        /// Get the overall total for the month
        /// </summary>
        /// <returns>Overall total</returns>
        public decimal GetOverallTotal()
        {
            return rowTotals.Values.Sum();
        }
    }
}
