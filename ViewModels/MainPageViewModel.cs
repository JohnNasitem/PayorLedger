//***********************************************************************************
//Program: MainPageViewModel.cs
//Description: View model for main page
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************

// TODO: Rework rows, rows should be entries, not each payor. also add date and an incrementing or#

using CommunityToolkit.Mvvm.Input;
using PayorLedger.Dialogs;
using PayorLedger.Models;
using PayorLedger.Models.Columns;
using PayorLedger.Pages;
using PayorLedger.Services.Actions;
using PayorLedger.Services.Actions.CellCommands;
using PayorLedger.Services.Actions.HeaderCommands;
using PayorLedger.Services.Actions.PayorCommands;
using PayorLedger.Services.Actions.RowCommands;
using PayorLedger.Services.Actions.SubheaderCommands;
using PayorLedger.Services.Database;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PayorLedger.ViewModels
{
    public record CellEditInfo(string ColumnName, int OrNum, decimal NewValue);
    public record RowEditInfo(int OrNum, string NewComment, string NewDate);

    public class MainPageViewModel
    {
        // Used in the Page view to display the tabs
        public ObservableCollection<MonthTab> Tabs { get { return _tabs; } }

        // Used in the Page view to redraw table
        public Action? RedrawTable;


        // Public Properties
        public int Year { get; set; }
        public List<DataColumn> TableColumns { get; private set; } = null!;
        public List<RowEntry> LedgerRows { get; set; } = null!;
        public List<HeaderEntry> Headers { get; set; } = null!;
        public List<PayorEntry> Payors { get; set; }= null!;

        public MainPage Page { get; }

        // Private members
        private ObservableCollection<MonthTab> _tabs = null!;
        private List<MonthTotal> _totals = [];
        private readonly PayorWindowViewModel _payorWindowVM;
        private readonly ColumnsWindowViewModel _columnsWindowVM;

        // Services
        private readonly IUndoRedoService _undoRedoService;
        private readonly IDatabaseService _dbService;




        #region CommandProperties
        public ICommand SaveLedgerCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand AddPayorCommand { get; }
        public ICommand AddHeaderCommand { get; }
        public ICommand AddSubheaderCommand { get; }
        public ICommand DeleteSubheaderCommand { get; }
        public ICommand CellEdittedCommand { get; }
        public ICommand RowEdittedCommand { get; }
        public ICommand ViewPayorCommand { get; }
        public ICommand OpenPayorWindowCommand { get; }
        public ICommand OpenColumnsWindowCommand { get; }
        #endregion



        public MainPageViewModel(IUndoRedoService undoRedoService, IDatabaseService dbService, PayorWindowViewModel payorWindowVM, ColumnsWindowViewModel columnsWindowVM)
        {
            Year = DateTime.Now.Year;
            Page = new MainPage(this);
            _undoRedoService = undoRedoService;
            _dbService = dbService;
            _payorWindowVM = payorWindowVM;
            _columnsWindowVM = columnsWindowVM;

            _undoRedoService.ChangeOccured += UndoRedoService_ChangeOccured;

            // Set up commands
            SaveLedgerCommand = new RelayCommand(ExecuteSaveLedger);
            UndoCommand = new RelayCommand(ExecuteUndo);
            RedoCommand = new RelayCommand(ExecuteRedo);
            AddPayorCommand = new RelayCommand(ExecuteAddPayor);
            AddHeaderCommand = new RelayCommand(ExecuteAddHeader);
            AddSubheaderCommand = new RelayCommand(ExecuteAddSubheader);
            DeleteSubheaderCommand = new RelayCommand<string>(ExecuteDeleteSubheader);
            CellEdittedCommand = new RelayCommand<CellEditInfo>(ExecuteCellEditted);
            RowEdittedCommand = new RelayCommand<RowEditInfo>(ExecuteRowEditted);
            ViewPayorCommand = new RelayCommand<string>(ExecuteViewPayor);
            OpenPayorWindowCommand = new RelayCommand(ExecuteOpenPayorWindow);
            OpenColumnsWindowCommand = new RelayCommand(ExecuteOpenColumnsWindow);

            // Populate global vars
            GetDatabaseValues();

            // Calculate totals
            SetYear(Year);
        }



        /// <summary>
        /// Populate global vars with database values
        /// </summary>
        private void GetDatabaseValues()
        {
            LedgerRows = _dbService.GetRows();

            _tabs = new ObservableCollection<MonthTab>(
                    Enum.GetValues<Month>()
                    .Cast<Month>()
                    .Select(m => new MonthTab(m)));

            Payors = [.. _dbService.GetPayors().OrderBy(p => p.PayorName)];
            Headers = _dbService.GetHeaders();
        }



        /// <summary>
        /// Get all subheaders from a list of headers
        /// </summary>
        /// <param name="headers">Headers to get subheaders from</param>
        /// <param name="filteredOutRemoved">True if the returned list should be free of all items marked as removed</param>
        /// <returns>List of subheaders</returns>
        public List<SubheaderEntry> GetSubheaders(List<HeaderEntry> headers, bool filteredOutRemoved)
        {
            return filteredOutRemoved ? 
                headers.Where(h => h.State != ChangeState.Removed).SelectMany(h => h.Subheaders).Where(h => h.State != ChangeState.Removed).ToList() : 
                headers.SelectMany(h => h.Subheaders).ToList();
        }



        /// <summary>
        /// Set the year and set up the year dependant varaibles
        /// Then update the UI
        /// </summary>
        /// <param name="year">Selected year</param>
        public void SetYear(int year)
        {
            Year = year;
            _totals = CalculateTotals();
            PopulateTabs();
        }



        #region UI Methods
        /// <summary>
        /// Populate UI tabs
        /// </summary>
        private void PopulateTabs()
        {
            // Populate ui
            foreach (MonthTab tab in _tabs)
                PopulateTab(tab);

            RedrawTable?.Invoke();
        }



        /// <summary>
        /// Populate the tab with columns and rows
        /// </summary>
        /// <param name="tab">tab to populate</param>
        /// <returns>Populated tab</returns>
        private void PopulateTab(MonthTab tab)
        {
            DataTable table = tab.Content;

            table.Columns.Clear();
            table.Rows.Clear();

            table.TableName = $"{tab.Month}-{Year}";

            TableColumns = CreateColumns(Headers, tab.Month == Month.All);

            List<RowEntry> monthData = tab.Month == Month.All ?
                LedgerRows.Where(e => e.Year == Year && e.State != ChangeState.Removed).ToList() :
                LedgerRows.Where(e => e.Year == Year && e.Month == tab.Month && e.State != ChangeState.Removed).ToList();

            MonthTotal? monthTotal = _totals.FirstOrDefault(mt => mt.Month == tab.Month && mt.Year == Year);
            if (monthTotal == null)
            {
                if (tab.Month == Month.All)
                    monthTotal = new MonthTotal(Year, tab.Month, _totals.Where(mt => mt.Year == Year).ToList());
                else
                    monthTotal = new MonthTotal(Year, tab.Month);
            }

            // Add columns
            foreach (DataColumn col in TableColumns)
                table.Columns.Add(col);

            // Add rows
            foreach (DataRow row in CreateRows(tab, TableColumns, monthData, monthTotal))
                table.Rows.Add(row);
        }



        /// <summary>
        /// Create columns for the DataTable
        /// </summary>
        /// <param name="headers">List of headers used to make the columns</param>
        /// <param name="isReadOnly">True if this is the table is read only, false otherwise</param>
        /// <returns>List of columns for the DataTable</returns>
        public static List<DataColumn> CreateColumns(List<HeaderEntry> headers, bool isReadOnly)
        {
            List<DataColumn> columns = [];

            columns.Add(MarkColumnAsDefault(new DataColumn("Date") { AllowDBNull = false}));
            columns.Add(MarkColumnAsDefault(new DataColumn("OR #") { AllowDBNull = false}));
            columns.Add(MarkColumnAsDefault(new DataColumn("Payor") { AllowDBNull = false, ReadOnly = true }));

            // Order columns
            headers = [.. headers.Where(h => h.State != ChangeState.Removed).OrderBy(h => h.Order)];
            foreach(HeaderEntry header in headers)
                header.Subheaders = [.. header.Subheaders.Where(s => s.State != ChangeState.Removed).OrderBy(s => s.Order)];

            // Add DataColumns
            foreach (HeaderEntry header in headers)
                foreach (SubheaderEntry subheader in header.Subheaders)
                {
                    DataColumn column = new($"{header.Name}\n{subheader.Name}");
                    // Make columns read only if the tab is the total tab
                    if (isReadOnly)
                        column.ReadOnly = true;
                    columns.Add(column);
                }

            // Add Total and Comments columns
            columns.Add(MarkColumnAsDefault(new DataColumn("Total") { AllowDBNull = false, ReadOnly = true }));
            columns.Add(MarkColumnAsDefault(new DataColumn("Comments") { AllowDBNull = false, ReadOnly = false }));

            return columns;

            // Mark a column as default
            static DataColumn MarkColumnAsDefault(DataColumn column)
            {
                column.ExtendedProperties.Add("IsDefault", true);
                return column;
            }
        }



        /// <summary>
        /// Create rows for the DataTable
        /// </summary>
        /// <param name="tab">Month tab to add columns to</param>
        /// <param name="columns">List of  columns</param>
        /// <param name="monthData">Cell data</param>
        /// <returns>List of rows for the DataTable</returns>
        private List<DataRow> CreateRows(MonthTab tab, List<DataColumn> columns, List<RowEntry> monthData, MonthTotal monthTotal)
        {
            // TODO: Add date and or#

            List<DataRow> rows = [];
            List<SubheaderEntry> subheaders = GetSubheaders(Headers, true);

            // Create rows
            foreach (RowEntry dataRow in monthData)
            {
                DataRow row = tab.Content.NewRow();

                // Initialize the row cells
                foreach (DataColumn col in columns)
                    row[col.ColumnName] = 0;
                row["Payor"] = Payors.Find(p => p.PayorId == dataRow.PayorId)!.PayorName;
                row["Date"] = dataRow.Date;
                row["Or #"] = dataRow.OrNum;

                // Populate the cells
                foreach (CellEntryToRow data in dataRow.CellEntries)
                {
                    SubheaderEntry subheader = subheaders.FirstOrDefault(s => s.Id == data.SubheaderId && s.State != ChangeState.Removed)!;

                    string colName = $"{subheader.Header.Name}\n{subheader.Name}";
                    row[colName] = decimal.Parse(row[colName].ToString()!) + data.Amount;
                }

                // Populate comments and total column
                row["Total"] = monthTotal.GetRowTotal(dataRow.OrNum);
                row["Comments"] = dataRow.Comment;

                rows.Add(row);
            }

            // Add total row
            DataRow totalRow = tab.Content.NewRow();
            foreach (DataColumn col in columns)
            {
                if (col.ExtendedProperties.ContainsKey("IsDefault"))
                    continue;

                totalRow[col.ColumnName] = monthTotal.GetColumnTotal(subheaders.First(s => s.Header.Name == col.ColumnName.Split('\n')[0] && s.Name == col.ColumnName.Split('\n')[1]).Id);
            }
            totalRow["Payor"] = "Total";
            totalRow["Date"] = "";
            totalRow["Or #"] = "";
            totalRow["Total"] = monthTotal.GetOverallTotal();
            totalRow["Comments"] = "";
            rows.Add(totalRow);

            return rows;
        }



        /// <summary>
        /// Calculate totals for months, year, and payors
        /// </summary>
        private List<MonthTotal> CalculateTotals()
        {
            List<MonthTotal> monthTotals = [];

            foreach (CellEntryToRow entry in LedgerRows.Where(e => e.State != ChangeState.Removed).SelectMany(r => r.CellEntries).Where(e => e.State != ChangeState.Removed))
            {
                // Get month total instance
                MonthTotal? monthTotal = monthTotals.FirstOrDefault(mt => mt.Month == entry.Row.Month && mt.Year == entry.Row.Year);

                // Create an instance if it doesnt exist
                if (monthTotal == null)
                {
                    monthTotal = new MonthTotal(entry.Row.Year, entry.Row.Month);
                    monthTotals.Add(monthTotal);
                }

                // Add to totals
                monthTotal.AddToColumnTotal(entry.SubheaderId, entry.Amount);
                monthTotal.AddToRowTotal(entry.Row.OrNum, entry.Amount);
            }

            return monthTotals;
        }



        /// <summary>
        /// Recalculates totals then populates the tabs with the new totals
        /// </summary>
        public void UpdateUI()
        {
            // Calculate totals
            _totals = CalculateTotals();

            // Populate ui
            PopulateTabs();
        }
        #endregion



        #region Commands
        /// <summary>
        /// Adds a payor to the database and refreshes the UI
        /// </summary>
        public void ExecuteAddPayor()
        {
            AddPayorDialog page = new();

            if (page.ShowDialog() != true)
                return;

            // Create a new payor with the data from the dialog
            _undoRedoService.Execute(new AddPayorCommand(new PayorEntry(-1, page.PayorName, page.PayorLabel, ChangeState.Added)));
        }



        /// <summary>
        /// Adds a header to the database
        /// </summary>
        public void ExecuteAddHeader()
        {
            AddHeaderDialog page = new();

            if (page.ShowDialog() != true)
                return;

            // Create a new header with the data from the dialog
            _undoRedoService.Execute(new AddHeaderCommand(new HeaderEntry(-1, page.HeaderName, page.HeaderOrder, ChangeState.Added)));
        }



        /// <summary>
        /// Adds a subheader to the database and refreshes the UI
        /// </summary>
        public void ExecuteAddSubheader()
        {
            AddSubheaderDialog page = new();

            if (page.ShowDialog() != true)
                return;

            HeaderEntry parentHeader = Headers.Find(h => h.Id == page.ParentHeader.Id)!;

            // Create a new subheader with the data from the dialog
            _undoRedoService.Execute(new AddSubheaderCommand(new SubheaderEntry(-1, page.SubheaderName, page.SubheaderOrder, parentHeader, ChangeState.Added)));
        }



        /// <summary>
        /// Save changes to database
        /// </summary>
        private void ExecuteSaveLedger()
        {
            _dbService.SaveChanges();
        }



        /// <summary>
        /// Undo a command
        /// </summary>
        private void ExecuteUndo()
        {
            _undoRedoService.Undo();
        }



        /// <summary>
        /// Redo a command
        /// </summary>
        private void ExecuteRedo()
        {
            _undoRedoService.Redo();
        }



        /// <summary>
        /// Delete a subheader from the database and refresh the UI
        /// </summary>
        /// <param name="columnName">Column name of the subheader being deleted</param>
        private void ExecuteDeleteSubheader(string? columnName)
        {
            if (columnName == null)
                return;

            // Extra header name and subheader name
            string[] nameParts = columnName.Split('\n');
            string headerName = nameParts[0].Trim();
            string subheaderName = nameParts[1].Trim();

            ConfirmationDialog dlg = new("Delete Confirmation", $"Are you sure you want to delete the \"{subheaderName}\" subheader from the \"{headerName}\" header?", new SolidColorBrush(Colors.Red));

            if (dlg.ShowDialog() != true)
                return;

            _undoRedoService.Execute(new DeleteSubheaderCommand(Headers.Find(h => h.Name == headerName)?.Subheaders.Find(s => s.Name == subheaderName)!));
        }



        /// <summary>
        /// Adds edit to the undo/redo stack and edits the cell entry or adds one if it doesn't exist
        /// </summary>
        /// <param name="cellInfo">Edit info</param>
        private void ExecuteCellEditted(CellEditInfo? cellInfo)
        {
            if (cellInfo == null)
                return;

            // Extract ids from cell info
            string[] columnNameParts = cellInfo.ColumnName.Split('\n');
            long subheaderId = Headers.Find(h => h.Name == columnNameParts[0])!.Subheaders.Find(s => s.Name == columnNameParts[1])!.Id;

            // Find associated entry
            CellEntryToRow? entry = LedgerRows.SelectMany(r => r.CellEntries).FirstOrDefault(e => e.Row.OrNum == cellInfo.OrNum);

            // Create a new entry if it doesn't exist
            if (entry == null)
            {
                entry = new CellEntryToRow(LedgerRows.Find(r => r.OrNum == cellInfo.OrNum)!, subheaderId, cellInfo.NewValue, ChangeState.Added);
                _undoRedoService.Execute(new AddCellCommand(entry));
            }
            // Delete entry if value is 0
            else if (cellInfo.NewValue == 0)
                _undoRedoService.Execute(new DeleteCellCommand(entry));
            // Update entry
            else
                _undoRedoService.Execute(new EditCellCommand(entry, cellInfo.NewValue));

        }



        /// <summary>
        /// Adds edit to the undo/redo stack and edits the cell comment or adds one if it doesn't exist
        /// </summary>
        /// <param name="rowInfo">Edit info</param>
        private void ExecuteRowEditted(RowEditInfo? rowInfo)
        {
            if (rowInfo == null)
                return;

            // Find associated row
            RowEntry entry = LedgerRows.Find(e => e.OrNum == rowInfo.OrNum)!;

            // Update entry
            _undoRedoService.Execute(new EditRowCommand(entry, rowInfo.NewDate, rowInfo.NewComment));
        }



        /// <summary>
        /// Open view payor page 
        /// </summary>
        /// <param name="payorName">Name of payor being viewed</param>
        private void ExecuteViewPayor(string? payorName)
        {
            if (payorName == null || payorName == "Total")
                return;

            _payorWindowVM.ViewPayor(payorName);
        }



        /// <summary>
        /// Open payor window
        /// </summary>
        private void ExecuteOpenPayorWindow()
        {
            _payorWindowVM.OpenWindow();
        }



        /// <summary>
        /// Open columns window
        /// </summary>
        private void ExecuteOpenColumnsWindow()
        {
            _columnsWindowVM.OpenWindow();
        }
        #endregion



        /// <summary>
        /// Update unsaved changes status
        /// </summary>
        /// <param name="sender">Undo redo service</param>
        /// <param name="e">Event args</param>
        private void UndoRedoService_ChangeOccured(object? sender, EventArgs e)
        {
            Page.SetUnsavedChangesStatus(!_dbService.AllChangesSaved);
        }
    }
}
