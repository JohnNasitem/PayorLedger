//***********************************************************************************
//Program: ViewPayorViewModel.cs
//Description: View model for view payor page
//Date: Aug 21, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Database;
using PayorLedger.Windows.Payors.Pages;
using System.Data;

namespace PayorLedger.ViewModels
{
    public class ViewPayorViewModel
    {
        /// <summary>
        /// Page associated with this view model
        /// </summary>
        public ViewPayor Page { get; }



        /// <summary>
        /// DataTable for this payor's total
        /// </summary>
        public DataTable PayorTable { get; }



        /// <summary>
        /// Set payor name action to be set by the page
        /// </summary>

        public Action<PayorEntry>? SetPayorName;



        /// <summary>
        /// Payor accociated with this page
        /// </summary>
        public PayorEntry Payor { get; private set;  } = null!;



        private readonly MainPageViewModel _mainPageVM;
        private readonly List<DataColumn> _columns;



        public ViewPayorViewModel(MainPageViewModel mainPageVM)
        {
            SetPayorName = null;
            Page = new(this);
            _mainPageVM = mainPageVM;
            PayorTable = new();

            // Create columns
            _columns = MainPageViewModel.CreateColumns(new(_mainPageVM.Headers.Where(h => h.State != ChangeState.Removed)), true);

            // Remove unused columns
            _columns.RemoveAll(c => c.ColumnName == "Payor");
            _columns.RemoveAll(c => c.ColumnName == "Comments");

            // Add a new Year column at the start
            DataColumn yearCol = new("Year") { AllowDBNull = false, ReadOnly = true };
            yearCol.ExtendedProperties.Add("IsDefault", true);
            _columns.Insert(0, yearCol);

        }



        /// <summary>
        /// Set up the payor
        /// </summary>
        /// <param name="payor"></param>
        public void SetPayor(PayorEntry payor)
        {
            Payor = payor;
            SetPayorName?.Invoke(payor);
            PopulateTable();
        }



        /// <summary>
        /// Update the table with new values
        /// </summary>
        public void UpdateTable()
        {
            PopulateTable();
        }



        /// <summary>
        /// Get all entries associated with this payor
        /// </summary>
        /// <returns>Dictionary with Year being the key and List of entries being the value</returns>
        private Dictionary<int, List<CellEntryToRow>> GetEntries()
        {
            // Get entries from main page view model
            return _mainPageVM.LedgerEntries
                .Where(pe => pe.PayorId == Payor.PayorId && pe.State != ChangeState.Removed)
                .GroupBy(pe => pe.Year)
                .ToDictionary(g => g.Key, g => g.ToList());
        }



        /// <summary>
        /// Populate table with columns and rows
        /// </summary>
        private void PopulateTable()
        {
            PayorTable.Columns.Clear();
            PayorTable.Rows.Clear();

            // Add Columns
            foreach (DataColumn col in _columns)
                PayorTable.Columns.Add(col);

            // Add Rows
            foreach (DataRow row in CreateRows(GetEntries()))
                PayorTable.Rows.Add(row);
        }



        /// <summary>
        /// Create the rows for the table
        /// </summary>
        /// <returns>List of rows</returns>
        private List<DataRow> CreateRows(Dictionary<int, List<CellEntryToRow>> entries)
        {
            List<DataRow> rows = [];
            decimal payorTotal = 0;
            Dictionary<string, decimal> columnTotals = [];
            List<SubheaderEntry> subheaders = _mainPageVM.GetSubheaders(_mainPageVM.Headers, true);

            // Initialize column totals
            foreach (DataColumn col in _columns)
                columnTotals.Add(col.ColumnName, 0);

            // Create rows for each payor
            foreach (int year in entries.Keys)
            {
                DataRow row = PayorTable.NewRow();
                decimal yearTotal = 0;

                // Initialize the row cells
                foreach (DataColumn col in _columns)
                    row[col.ColumnName] = 0;
                row["Year"] = year;

                // Populate the cells
                foreach (CellEntryToRow entry in entries[year])
                {
                    SubheaderEntry subheader = subheaders.FirstOrDefault(s => s.Id == entry.SubheaderId)!;

                    string colName = $"{subheader.Header.Name}\n{subheader.Name}";
                    row[colName] = decimal.Parse(row[colName].ToString()!) + entry.Amount;
                    yearTotal += entry.Amount;
                    columnTotals[colName] += entry.Amount;
                }

                // Populate comments and total column
                row["Total"] = yearTotal;
                payorTotal += yearTotal;

                rows.Add(row);
            }

            // Add total row
            DataRow totalRow = PayorTable.NewRow();
            foreach (DataColumn col in _columns)
            {
                if (col.ExtendedProperties.ContainsKey("IsDefault"))
                    continue;

                totalRow[col.ColumnName] = columnTotals[col.ColumnName];
            }
            totalRow["Year"] = "Total";
            totalRow["Total"] = payorTotal;
            rows.Add(totalRow);

            return rows;
        }
    }
}
