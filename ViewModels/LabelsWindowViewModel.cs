//***********************************************************************************
//Program: LabelsWindowViewModel.cs
//Description: View model for labels window
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Database;
using PayorLedger.Windows.Labels;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace PayorLedger.ViewModels
{
    public class LabelsWindowViewModel : WindowViewModel
    {
        /// <summary>
        /// Window the view model is associated to
        /// </summary>
        public override Window Window { get; protected set; }



        /// <summary>
        /// Total tab
        /// </summary>
        public ObservableCollection<TotalTab> Tabs { get; set; }



        /// <summary>
        /// Currently selected year
        /// </summary>
        public int Year { get; set; }



        private readonly ViewPayorViewModel _viewPayorVM;
        private Month[] _usedMonths = Enum.GetValues<Month>().Where(m => m != Month.All).ToArray();



        public LabelsWindowViewModel(ViewPayorViewModel viewPayorVM)
        {
            _viewPayorVM = viewPayorVM;

            // Create the total tabs
            Tabs = new ObservableCollection<TotalTab>(
                    Enum.GetValues<RowEntry.RowLabel>()
                    .Select(m => new TotalTab(m)));

            Window = new LabelsWindow(this);
        }



        /// <summary>
        /// Update ui with the new year
        /// </summary>
        /// <param name="newYear">New year</param>
        public void SetYear(int newYear)
        {
            Year = newYear;
            UpdateUI();
        }



        /// <summary>
        /// Update the table with new values
        /// </summary>
        public void UpdateUI()
        {
            PopulateTabs();
            ((LabelsWindow)Window).RefreshTabs();
        }



        /// <summary>
        /// Get all entries with the ones marked as removed filtered out 
        /// </summary>
        /// <returns>Entries grouped by label then by month</returns>
        private Dictionary<RowEntry.RowLabel, Dictionary<Month, List<CellEntryToRow>>> GetEntries()
        {
            Dictionary<RowEntry.RowLabel, Dictionary<Month, List<CellEntryToRow>>> dict = [];

            // Create empty dict will all the labels and months
            foreach (RowEntry.RowLabel label in Enum.GetValues<RowEntry.RowLabel>())
            {
                Dictionary<Month, List<CellEntryToRow>> monthGroupedEntries = [];
                foreach (Month month in _usedMonths)
                    monthGroupedEntries[month] = [];
                dict[label] = monthGroupedEntries;
            }

            // Add entries to the dict
            foreach (RowEntry row in _viewPayorVM.MainPageVM.LedgerRows.Where(r => r.State != ChangeState.Removed && r.Year == Year))
                dict[row.Label][row.Month].AddRange(row.CellEntries.Where(ce => ce.State != ChangeState.Removed));

            return dict;
        }




        /// <summary>
        /// Populate the tabs
        /// </summary>
        private void PopulateTabs()
        {
            if (_viewPayorVM.MainPageVM == null)
                _viewPayorVM.MainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();

            Dictionary<RowEntry.RowLabel, Dictionary<Month, List<CellEntryToRow>>> entries = GetEntries();
            List<DataColumn> columns = _viewPayorVM.GetColumnList("Month");

            foreach (TotalTab tab in Tabs)
            {
                DataTable table = tab.Content;

                table.Columns.Clear();
                table.Rows.Clear();

                // Add Columns
                foreach (DataColumn col in _viewPayorVM.GetColumnList("Month"))
                    table.Columns.Add(col);

                foreach(DataRow row in CreateRows(columns, table, entries[tab.Label]))
                    table.Rows.Add(row);
            }
        }



        /// <summary>
        /// Create the rows for the table
        /// </summary>
        /// <returns>List of rows</returns>
        private List<DataRow> CreateRows(List<DataColumn> columns, DataTable table, Dictionary<Month, List<CellEntryToRow>> entries)
        {
            List<DataRow> rows = [];
            decimal labelTotal = 0;
            Dictionary<string, decimal> columnTotals = [];
            List<SubheaderEntry> subheaders = _viewPayorVM.MainPageVM.GetSubheaders(_viewPayorVM.MainPageVM.Headers, true);

            // Initialize column totals
            foreach (DataColumn col in columns)
                columnTotals.Add(col.ColumnName, 0);

            // Create rows for each month
            foreach (Month month in _usedMonths)
            {
                DataRow row = table.NewRow();
                decimal yearTotal = 0;

                // Initialize the row cells
                foreach (DataColumn col in columns)
                    row[col.ColumnName] = 0;
                row["Month"] = month.ToString();

                // Populate the cells
                foreach (CellEntryToRow entry in entries[month])
                {
                    SubheaderEntry subheader = subheaders.FirstOrDefault(s => s.Id == entry.SubheaderId)!;

                    string colName = $"{subheader.Header.Name}\n{subheader.Name}";
                    row[colName] = decimal.Parse(row[colName].ToString()!) + entry.Amount;
                    yearTotal += entry.Amount;
                    columnTotals[colName] += entry.Amount;
                }

                // Populate comments and total column
                row["Total"] = yearTotal;
                labelTotal += yearTotal;

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
            totalRow["Month"] = "Total";
            totalRow["Total"] = labelTotal;
            rows.Add(totalRow);

            return rows;
        }
    }
}
