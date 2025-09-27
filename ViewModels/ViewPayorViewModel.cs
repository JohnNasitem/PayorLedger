//***********************************************************************************
//Program: ViewPayorViewModel.cs
//Description: View model for view payor page
//Date: Aug 21, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Database;
using PayorLedger.Windows.Payors.Pages;
using System.Collections.ObjectModel;
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
        /// Set payor name action to be set by the page
        /// </summary>

        public Action<PayorEntry>? SetPayorName;



        /// <summary>
        /// Payor accociated with this page
        /// </summary>
        public PayorEntry Payor { get; private set; } = null!;



        /// <summary>
        /// Total tab
        /// </summary>
        public ObservableCollection<TotalTab> Tabs { get; set; }



        public MainPageViewModel MainPageVM { get; set; } = null!;



        public ViewPayorViewModel()
        {
            SetPayorName = null;
            Page = new(this);

            // Create the total tabs
            Tabs = new ObservableCollection<TotalTab>(
                    Enum.GetValues<RowEntry.RowLabel>()
                    .Select(m => new TotalTab(m)));
        }



        /// <summary>
        /// Get a new copy of the list of visible columns
        /// </summary>
        /// <returns>List of data columns</returns>
        public List<DataColumn> GetColumnList(string newColName)
        {
            // Create columns
            List<DataColumn> columns = MainPageViewModel.CreateColumns(new(MainPageVM.Headers.Where(h => h.State != ChangeState.Removed)), true);

            // Remove unused columns
            columns.RemoveAll(c => c.ColumnName == "Payor");
            columns.RemoveAll(c => c.ColumnName == "Label");
            columns.RemoveAll(c => c.ColumnName == "Comments");
            columns.RemoveAll(c => c.ColumnName == "Date");
            columns.RemoveAll(c => c.ColumnName == "OR #");

            // Add a new column at the start
            DataColumn yearCol = new(newColName) { AllowDBNull = false, ReadOnly = true };
            yearCol.ExtendedProperties.Add("IsDefault", true);
            columns.Insert(0, yearCol);

            return columns;
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
            Page.RefreshTabs();
        }



        /// <summary>
        /// Get all entries associated with this payor
        /// </summary>
        /// <returns>Dictionary with Year being the key and List of entries being the value</returns>
        private Dictionary<int, List<CellEntryToRow>> GetEntries()
        {
            // Get entries from main page view model
            return MainPageVM.LedgerRows
                .Where(r => r.State != ChangeState.Removed)
                .SelectMany(r => r.CellEntries)
                .Where(ce => ce.Row.PayorId == Payor.PayorId && ce.State != ChangeState.Removed)
                .GroupBy(ce => ce.Row.Year)
                .ToDictionary(g => g.Key, g => g.ToList());
        }



        /// <summary>
        /// Populate table with columns and rows
        /// </summary>
        private void PopulateTable()
        {
            if (MainPageVM == null)
                MainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();

            Dictionary<int, List<CellEntryToRow>> entries = GetEntries();
            List<DataColumn> columns = GetColumnList("Year");

            foreach (TotalTab tab in Tabs)
            {
                tab.Content.Columns.Clear();
                tab.Content.Rows.Clear();

                // Add Columns
                foreach (DataColumn col in GetColumnList("Year"))
                    tab.Content.Columns.Add(col);

                // Add Rows
                foreach (DataRow row in CreateRows(columns, tab.Content, entries.SelectMany(r => r.Value)
                                                .Where(ce => ce.Row.Label == tab.Label)
                                                .GroupBy(ce => ce.Row.Year)
                                                .ToDictionary(g => g.Key, g => g.ToList())))
                    tab.Content.Rows.Add(row);
            }
        }



        /// <summary>
        /// Create the rows for the table
        /// </summary>
        /// <returns>List of rows</returns>
        private List<DataRow> CreateRows(List<DataColumn> columns, DataTable table, Dictionary<int, List<CellEntryToRow>> entries)
        {
            List<DataRow> rows = [];
            decimal payorTotal = 0;
            Dictionary<string, decimal> columnTotals = [];
            List<SubheaderEntry> subheaders = MainPageVM.GetSubheaders(MainPageVM.Headers, true);

            // Initialize column totals
            foreach (DataColumn col in columns)
                columnTotals.Add(col.ColumnName, 0);

            // Create rows for each payor
            foreach (int year in entries.Keys)
            {
                DataRow row = table.NewRow();
                decimal yearTotal = 0;

                // Initialize the row cells
                foreach (DataColumn col in columns)
                    row[col.ColumnName] = 0;
                row["Year"] = year;

                // Populate the cells
                foreach (CellEntryToRow entry in entries[year])
                {
                    SubheaderEntry subheader = subheaders.FirstOrDefault(s => s.Id == entry.SubheaderId)!;

                    string colName = $"{subheader.Header.Name}\n{subheader.Name}";
                    row[colName] = entry.Amount;
                    yearTotal += entry.Amount;
                    columnTotals[colName] += entry.Amount;
                }

                // Populate comments and total column
                row["Total"] = yearTotal;
                payorTotal += yearTotal;

                rows.Add(row);
            }

            // Add total row
            DataRow totalRow = table.NewRow();
            foreach (DataColumn col in columns)
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
