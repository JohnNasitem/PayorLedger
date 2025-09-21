//***********************************************************************************
//Program: DatabaseService.cs
//Description: Interact with the database
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Actions;
using PayorLedger.Services.Logger;
using PayorLedger.ViewModels;
using System.Data.SQLite;

namespace PayorLedger.Services.Database
{
    public class DatabaseService : IDatabaseService
    {
        /// <summary>
        /// List of names that are not allowed to be used for payors, headers, or subheaders
        /// </summary>
        public string[] InvalidNames { get; } = ["", "label", "total", "payor", "comments", "date", "or #"];



        /// <summary>
        /// Are all chnages saved
        /// </summary>
        public bool AllChangesSaved { get; set; } = true;



        // Connection to database
        private readonly SQLiteConnection _sqlConnection;



        /// <summary>
        /// Initialize a new instance of <see cref="DatabaseService"/>
        /// </summary>
        public DatabaseService()
        {
            _sqlConnection = new SQLiteConnection(@"Data Source=payorledger.db");
            _sqlConnection.Open();

            // Enable foreign keys
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Initialize tables if they dont exist
        /// </summary>
        public void InitializeTables()
        {
            // Initialize Payor table
            if (!DoesTableExist(DatabaseTables.Payor))
            {
                using SQLiteCommand cmd = _sqlConnection.CreateCommand();
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.Payor)}(
                                        PayorId INTEGER PRIMARY KEY AUTOINCREMENT, 
                                        PayorName TEXT NOT NULL
                                        )";
                cmd.ExecuteNonQuery();
            }

            // Initialize Header table
            if (!DoesTableExist(DatabaseTables.Header))
            {
                using SQLiteCommand cmd = _sqlConnection.CreateCommand();
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.Header)}(
                                        HeaderId INTEGER PRIMARY KEY AUTOINCREMENT, 
                                        HeaderName TEXT NOT NULL,
                                        HeaderOrder INTEGER NOT NULL
                                        )";
                cmd.ExecuteNonQuery();
            }

            // Initialize Subheader table
            if (!DoesTableExist(DatabaseTables.Subheader))
            {
                using SQLiteCommand cmd = _sqlConnection.CreateCommand();
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.Subheader)}(
                                        HeaderID INTEGER NOT NULL,
                                        SubHeaderId INTEGER PRIMARY KEY AUTOINCREMENT, 
                                        SubHeaderName TEXT NOT NULL,
                                        SubHeaderOrder INTEGER NOT NULL,
                                        FOREIGN KEY (HeaderID) REFERENCES {Enum.GetName(DatabaseTables.Header)}(HeaderID)
                                        )";
                cmd.ExecuteNonQuery();
            }

            // Initialize Rows table
            if (!DoesTableExist(DatabaseTables.Rows))
            {
                using SQLiteCommand cmd = _sqlConnection.CreateCommand();
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.Rows)}(
                                        Label TEXT NOT NULL,
                                        Date TEXT NOT NULL,
                                        OrNum INTEGER NOT NULL PRIMARY KEY,
                                        PayorId INTEGER NOT NULL,
                                        Comment TEXT NOT NULL,
                                        Month INTEGER NOT NULL,
                                        Year INTEGER NOT NULL,
                                        FOREIGN KEY (PayorId) REFERENCES {Enum.GetName(DatabaseTables.Payor)}(PayorId)
                                        )";
                cmd.ExecuteNonQuery();
            }

            // Initialize CellEntryToRow table
            if (!DoesTableExist(DatabaseTables.CellEntryToRow))
            {
                using SQLiteCommand cmd = _sqlConnection.CreateCommand();
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.CellEntryToRow)}(
                                        OrNum INTEGER NOT NULL, 
                                        SubHeaderId INTEGER NOT NULL,
                                        Amount NUMERIC NOT NULL,
                                        FOREIGN KEY (SubHeaderId) REFERENCES {Enum.GetName(DatabaseTables.Subheader)}(SubHeaderId),
                                        FOREIGN KEY (OrNum) REFERENCES {Enum.GetName(DatabaseTables.Rows)}(OrNum),
                                        Primary Key (OrNum, SubHeaderId)
                                        )";
                cmd.ExecuteNonQuery();
            }
        }



        /// <summary>
        /// Sync changes to the database
        /// </summary>
        public void SaveChanges()
        {
            ILogger logger = App.ServiceProvider.GetRequiredService<ILogger>();

            // Get view model data
            MainPageViewModel mainPageVM = App.ServiceProvider.GetRequiredService<MainPageViewModel>();
            List<PayorEntry> payors = mainPageVM.Payors;
            List<HeaderEntry> headers = mainPageVM.Headers;
            List<RowEntry> rows = mainPageVM.LedgerRows;

            List<object> objectsToRemoveFromLists = [];

            // Sync payor data
            foreach (PayorEntry payor in payors)
            {
                switch (payor.State)
                {
                    case ChangeState.Added:
                        long payorId = AddPayor(payor.PayorName);

                        // Replace temp id with new id
                        foreach (RowEntry entry in rows.Where(e => e.PayorId == payor.PayorId))
                            entry.PayorId = payorId;

                        payor.PayorId = payorId;
                        break;
                    case ChangeState.Removed:
                        DeletePayor(payor.PayorId);
                        objectsToRemoveFromLists.Add(payor);
                        break;
                    case ChangeState.Edited:
                        EditPayorEntry(payor);
                        break;
                }

                payor.State = ChangeState.Unchanged;
            }

            // Remove deleted payors from list
            foreach (PayorEntry payor in objectsToRemoveFromLists.Cast<PayorEntry>())
                payors.RemoveAll(p => p.PayorId == payor.PayorId);
            objectsToRemoveFromLists.Clear();

            logger.AddLog("Finished saving payor changes!", Logger.Logger.LogType.Action);

            // Sync headers
            foreach (HeaderEntry header in headers)
            {
                switch (header.State)
                {
                    case ChangeState.Added:
                        long headerId = AddHeader(header.Name, header.Order);

                        // Replace temp id with new id
                        header.Id = headerId;
                        break;
                    case ChangeState.Removed:
                        DeleteHeader(header.Id);
                        objectsToRemoveFromLists.Add(header);
                        break;
                    case ChangeState.Edited:
                        EditHeaderEntry(header);
                        break;
                }

                header.State = ChangeState.Unchanged;
            }

            // Remove deleted headers from list
            foreach (HeaderEntry header in objectsToRemoveFromLists.Cast<HeaderEntry>())
                headers.RemoveAll(h => h.Id == header.Id);
            objectsToRemoveFromLists.Clear();

            logger.AddLog("Finished saving header changes!", Logger.Logger.LogType.Action);

            // Sync subheaders
            foreach (SubheaderEntry subheader in headers.SelectMany(h => h.Subheaders))
            {
                switch (subheader.State)
                {
                    case ChangeState.Added:
                        long subheaderId = AddSubheader(subheader);

                        // Replace temp id with new id
                        foreach (CellEntryToRow entry in rows.SelectMany(r => r.CellEntries).Where(e => e.SubheaderId == subheader.Id))
                            entry.SubheaderId = subheaderId;

                        subheader.Id = subheaderId;
                        break;
                    case ChangeState.Removed:
                        // Dont need to save to delete later as DeleteHeader() will handle subheaders as well
                        subheader.Header.Subheaders.RemoveAll(s => s.Id == subheader.Id);
                        break;
                    case ChangeState.Edited:
                        EditSubheaderEntry(subheader);
                        break;
                }

                subheader.State = ChangeState.Unchanged;
            }

            logger.AddLog("Finished saving subheader changes!", Logger.Logger.LogType.Action);

            // Sync rows
            foreach (RowEntry row in rows)
            {
                switch (row.State)
                {
                    case ChangeState.Added:
                        AddRowEntry(row);
                        break;
                    case ChangeState.Removed:
                        DeleteRow(row);
                        objectsToRemoveFromLists.Add(row);
                        break;
                    case ChangeState.Edited:
                        EditRowEntry(row);
                        break;
                }

                row.State = ChangeState.Unchanged;
            }

            // Remove deleted rows from list
            foreach (RowEntry row in objectsToRemoveFromLists.Cast<RowEntry>())
                rows.RemoveAll(r=> r.OrNum == row.OrNum);
            objectsToRemoveFromLists.Clear();

            logger.AddLog("Finished saving row changes!", Logger.Logger.LogType.Action);

            // Sync cell entries
            foreach (CellEntryToRow cellEntry in rows.SelectMany(r => r.CellEntries).Where(c => c.State != ChangeState.Unchanged))
            {
                switch (cellEntry.State)
                {
                    case ChangeState.Added:
                        AddCellEntry(cellEntry);
                        break;
                    case ChangeState.Removed:
                        DeleteCellEntry(cellEntry, null);
                        objectsToRemoveFromLists.Add(cellEntry);
                        break;
                    case ChangeState.Edited:
                        EditCellEntry(cellEntry);
                        break;
                }

                cellEntry.State = ChangeState.Unchanged;
            }

            // Remove deleted cells from the row
            foreach (CellEntryToRow cellEntry in objectsToRemoveFromLists.Cast<CellEntryToRow>())
                cellEntry.Row.CellEntries.RemoveAll(c => c.SubheaderId == cellEntry.SubheaderId);

            logger.AddLog("Finished saving cell entry changes!", Logger.Logger.LogType.Action);

            AllChangesSaved = true;
            App.ServiceProvider.GetRequiredService<IUndoRedoService>().OnChangeOccured(true);
            logger.AddLog("All changes saved!", Logger.Logger.LogType.Action);
        }



        #region GetMethods
        /// <summary>
        /// Get all the entries
        /// </summary>>
        /// <returns>Row entries</returns>
        public List<RowEntry>  GetRows()
        {
            List<RowEntry> rows = [];

            // Get rows
            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.Rows)}";

                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rows.Add(new RowEntry(
                        label: Enum.Parse<RowEntry.RowLabel>(reader["Label"].ToString()!),
                        date: reader["Date"].ToString()!,
                        orNum: int.Parse(reader["OrNum"].ToString()!),
                        payorId: long.Parse(reader["PayorId"].ToString()!),
                        month: (Month)int.Parse(reader["Month"].ToString()!),
                        comment: reader["Comment"].ToString()!,
                        year: int.Parse(reader["Year"].ToString()!),
                        state: ChangeState.Unchanged,
                        cellEntries: []
                    ));
                }
            }

            // Get cell entries
            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.CellEntryToRow)}";

                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int orNum = int.Parse(reader["OrNum"].ToString()!);
                    RowEntry parentRow = rows.Find(r => r.OrNum == orNum)!;

                    parentRow.CellEntries.Add(new CellEntryToRow(
                        row: parentRow,
                        subheaderId: long.Parse(reader["SubheaderId"].ToString()!),
                        amount: decimal.Parse(reader["Amount"].ToString()!),
                        state: ChangeState.Unchanged
                    ));
                }
            }

            return rows;
        }



        /// <summary>
        /// Get the headers
        /// </summary>
        /// <returns>List of headers</returns>
        public List<HeaderEntry> GetHeaders()
        {
            List<HeaderEntry> headers = [];

            // Get headers
            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.Header)}";

                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    headers.Add(new HeaderEntry(
                        headerId: long.Parse(reader["HeaderID"].ToString()!),
                        headerName: reader["HeaderName"].ToString()!,
                        headerOrder: int.Parse(reader["HeaderOrder"].ToString()!),
                        state: ChangeState.Unchanged
                    ));
                }
            }

            // Get subheaders
            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.Subheader)}";

                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    HeaderEntry parentHeader = headers.First(h => h.Id == long.Parse(reader["HeaderID"].ToString()!));

                    parentHeader.Subheaders.Add(new SubheaderEntry(
                        subheaderId: long.Parse(reader["SubheaderID"].ToString()!),
                        subheaderName: reader["SubheaderName"].ToString()!,
                        subheaderOrder: int.Parse(reader["SubheaderOrder"].ToString()!),
                        header: parentHeader,
                        state: ChangeState.Unchanged
                    ));
                }
            }

            return headers;
        }



        /// <summary>
        /// Get all the ids of the subheaders connected to a header
        /// </summary>
        /// <param name="headerId">id of header</param>
        /// <returns>list of subheaders</returns>
        private List<long> GetSubheaderIds(long headerId)
        {
            List<long> subheadersIds = [];

            // Get subheaders
            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.Subheader)} WHERE HeaderId = @headerId";
                cmd.Parameters.AddWithValue("@headerId", headerId);

                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    subheadersIds.Add(long.Parse(reader["SubheaderID"].ToString()!));
                }
            }

            return subheadersIds;
        }



        /// <summary>
        /// Get the payors
        /// </summary>
        /// <returns>List of payors</returns>
        public List<PayorEntry> GetPayors()
        {
            List<PayorEntry> payors = [];

            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.Payor)}";

                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    payors.Add(new PayorEntry(
                        payorId: long.Parse(reader["PayorId"].ToString()!),
                        payorName: reader["PayorName"].ToString()!,
                        state: ChangeState.Unchanged
                    ));
                }
            }

            return payors;
        }
        #endregion



        #region DeleteMethods
        /// <summary>
        /// Delete a payor from the database<br/>
        /// Also deletes all entries and comments associated with the payor
        /// </summary>
        /// <param name="payorId">id of payor being deleted</param>
        private void DeletePayor(long payorId)
        {
            using var transaction = _sqlConnection.BeginTransaction();
            using var cmd = _sqlConnection.CreateCommand();
            cmd.Transaction = transaction;

            cmd.Parameters.AddWithValue("@payorId", payorId);

            // Delete cell entries associated with the payor
            cmd.CommandText = $@"DELETE FROM {Enum.GetName(DatabaseTables.Rows)} WHERE PayorId = @payorId";
            cmd.ExecuteNonQuery();

            // Delete comment entries associated with the payor
            cmd.CommandText = $@"DELETE FROM {Enum.GetName(DatabaseTables.Rows)} WHERE PayorId = @payorId";
            cmd.ExecuteNonQuery();

            // Delete entry from the Payor table
            cmd.CommandText = $@"DELETE FROM {Enum.GetName(DatabaseTables.Payor)} WHERE PayorId = @payorId";
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }



        /// <summary>
        /// Delete a header and its subheaders from the database<br/>
        /// Also adjusts the total owed from the affected payors
        /// </summary>
        /// <param name="headerId">id of header being deleted</param>
        private void DeleteHeader(long headerId)
        {
            using SQLiteTransaction transaction = _sqlConnection.BeginTransaction();
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.Transaction = transaction;

            // Remove all subheaders
            List<long> subheadersIds = GetSubheaderIds(headerId);
            foreach (long subheaderId in subheadersIds)
                DeleteSubheader(subheaderId, cmd);

            // Delete entry from the Header table
            cmd.Parameters.Clear();
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.Header)} WHERE HeaderId = @headerId";
            cmd.Parameters.AddWithValue("@headerId", headerId);
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }



        /// <summary>
        /// Delete a subheader from the database<br/>
        /// Also adjusts the total owed from the affected payors
        /// </summary>
        /// <param name="subheaderId">id of subheader being deleted</param>
        /// <param name="cmd">Existing SQLiteCommand</param>
        private void DeleteSubheader(long subheaderId, SQLiteCommand cmd)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@subheaderId", subheaderId);

            // Delete cell entries associated with the subheader
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.Rows)} WHERE SubheaderId = @subheaderId";
            cmd.ExecuteNonQuery();

            // Delete entry from the Subheader table
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.Subheader)} WHERE SubheaderId = @subheaderId";
            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Delete a row and its cell entries
        /// </summary>
        /// <param name="row">Row entry</param>
        public void DeleteRow(RowEntry row)
        {
            using SQLiteTransaction transaction = _sqlConnection.BeginTransaction();
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();

            // Delete cell entries first
            foreach (CellEntryToRow cellEntry in row.CellEntries)
                DeleteCellEntry(cellEntry, cmd);

            cmd.Parameters.Clear();
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.Rows)} WHERE OrNum = @orNum";
            cmd.Parameters.AddWithValue("@orNum", row.OrNum);
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }



        /// <summary>
        /// Delete a cell entry from the database
        /// </summary>
        /// <param name="entry">Entry to delete</param>
        /// <param name="cmd">Existing SQLiteCommand</param>
        private void DeleteCellEntry(CellEntryToRow entry, SQLiteCommand? cmd)
        {
            cmd ??= _sqlConnection.CreateCommand();

            cmd.Parameters.Clear();
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.CellEntryToRow)} WHERE OrNum = @orNum AND SubHeaderId = @subheaderId";
            cmd.Parameters.AddWithValue("@orNum", entry.Row.OrNum);
            cmd.Parameters.AddWithValue("@subheaderId", entry.SubheaderId);
            cmd.ExecuteNonQuery();
        }
        #endregion



        #region AddMethods
        /// <summary>
        /// Add a payor to the database
        /// </summary>
        /// <param name="payorName">Name of payor</param>
        /// <returns>Id of the new payor</returns>
        private long AddPayor(string payorName)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.Payor)} (PayorName) VALUES (@payorName)";
            cmd.Parameters.AddWithValue("@payorName", payorName);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            return (long)cmd.ExecuteScalar();

        }



        /// <summary>
        /// Add a header to the database
        /// </summary>
        /// <param name="header">Header to add</param>
        /// <returns>Id of the new header</returns>
        private long AddHeader(HeaderEntry header)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.Header)} (HeaderName, HeaderOrder) VALUES (@headerName, @headerOrder)";
            cmd.Parameters.AddWithValue("@headerName", header.Name);
            cmd.Parameters.AddWithValue("@headerOrder", header.Order);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            return (long)cmd.ExecuteScalar();
        }



        /// <summary>
        /// Add a subheader to the database
        /// </summary>
        /// <param name="subheader">Subheader to add</param>
        /// <returns>Id of the new subheader</returns>
        private long AddSubheader(SubheaderEntry subheader)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.Subheader)} (SubheaderName, HeaderId, SubHeaderOrder) VALUES (@subheaderName, @headerId, @subHeaderOrder)";
            cmd.Parameters.AddWithValue("@subheaderName", subheader.Name);
            cmd.Parameters.AddWithValue("@headerId", subheader.Header.Id);
            cmd.Parameters.AddWithValue("@subHeaderOrder", subheader.Order);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            return (long)cmd.ExecuteScalar();
        }



        /// <summary>
        /// Add an entry to the Row table
        /// </summary>
        /// <param name="row">Row to add</param>
        private void AddRowEntry(RowEntry entry)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.Rows)} (Label, Date, OrNum, PayorId, Comment, Month, Year) VALUES (@label, @date, @orNum, @payorId, @comment, @month, @year)";
            cmd.Parameters.AddWithValue("@label", entry.Label.ToString());
            cmd.Parameters.AddWithValue("@date", entry.Date);
            cmd.Parameters.AddWithValue("@orNum", entry.OrNum);
            cmd.Parameters.AddWithValue("@payorId", entry.PayorId);
            cmd.Parameters.AddWithValue("@comment", entry.Comment);
            cmd.Parameters.AddWithValue("@month", entry.Month);
            cmd.Parameters.AddWithValue("@year", entry.Year);
            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Add a entry to the CellEntryToRow table
        /// </summary>
        /// <param name="entry">Entry to add</param>
        private void AddCellEntry(CellEntryToRow entry)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.CellEntryToRow)} (OrNum, SubHeaderId, Amount) VALUES (@orNum, @subheaderId, @amount)";
            cmd.Parameters.AddWithValue("@orNum", entry.Row.OrNum);
            cmd.Parameters.AddWithValue("@subheaderId", entry.SubheaderId);
            cmd.Parameters.AddWithValue("@amount", entry.Amount);
            cmd.ExecuteNonQuery();
        }
        #endregion



        #region EditMethods
        /// <summary>
        /// Edit an existing payor <br/>
        /// Adds a payor if it doesnt edit an existing
        /// </summary>
        /// <param name="payor">Payor to edit</param>
        private void EditPayorEntry(PayorEntry payor)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.Payor)} SET PayorName = @payorName WHERE PayorId = @payorId";
            cmd.Parameters.AddWithValue("@payorName", payor.PayorName);
            cmd.Parameters.AddWithValue("@payorId", payor.PayorId);
            int affectedRows = cmd.ExecuteNonQuery();

            if (affectedRows == 0)
                AddPayor(payor.PayorName);
        }



        /// <summary>
        /// Update a cell entry <br/>
        /// Adds a cell entry if it doesnt edit an existing entry
        /// </summary>
        /// <param name="entry">New entry values</param>
        private void EditCellEntry(CellEntryToRow entry)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.CellEntryToRow)} SET Amount = @newAmount WHERE OrNum = @orNum AND SubHeaderId = @subheaderId";
            cmd.Parameters.AddWithValue("@orNum", entry.Row.OrNum);
            cmd.Parameters.AddWithValue("@subheaderId", entry.SubheaderId);
            cmd.Parameters.AddWithValue("@newAmount", entry.Amount);
            int affectedRows = cmd.ExecuteNonQuery();

            if (affectedRows == 0)
                AddCellEntry(entry);
        }



        /// <summary>
        /// Update a row entry <br/>
        /// Adds a row if it doesnt edit an existing entry
        /// </summary>
        /// <param name="entry">New row values</param>
        private void EditRowEntry(RowEntry entry)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.Rows)} SET OrNum = @orNum, Label = @label, PayorId = @payorId, Date = @date, Comment = @comment WHERE OrNum = @orNum";
            cmd.Parameters.AddWithValue("@label", entry.Label.ToString());
            cmd.Parameters.AddWithValue("@comment", entry.Comment);
            cmd.Parameters.AddWithValue("@date", entry.Date);
            cmd.Parameters.AddWithValue("@payorId", entry.PayorId);
            cmd.Parameters.AddWithValue("@orNum", entry.OrNum);
            int affectedRows = cmd.ExecuteNonQuery();

            if (affectedRows == 0)
                AddRowEntry(entry);
        }



        /// <summary>
        /// Update a subheader entry <br/>
        /// Adds a subheader if it doesnt edit an existing entry
        /// </summary>
        /// <param name="subheader">Subheader being editted</param>
        private void EditSubheaderEntry(SubheaderEntry subheader)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.Subheader)} SET HeaderID = @headerId, SubHeaderName = @subheaderName, SubHeaderOrder = @subheaderOrder WHERE SubHeaderId = @subheaderId";
            cmd.Parameters.AddWithValue("@headerId", subheader.Header.Id);
            cmd.Parameters.AddWithValue("@subheaderName", subheader.Name);
            cmd.Parameters.AddWithValue("@subheaderOrder", subheader.Order);
            cmd.Parameters.AddWithValue("@subheaderId", subheader.Id);
            int affectedRows = cmd.ExecuteNonQuery();

            if (affectedRows == 0)
                AddSubheader(subheader);
        }



        /// <summary>
        /// Update a header entry
        /// </summary>
        /// <param name="header">Header being editted</param>
        private void EditHeaderEntry(HeaderEntry header)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.Header)} SET HeaderName = @headerName, HeaderOrder = @headerOrder WHERE HeaderId = @headerId";
            cmd.Parameters.AddWithValue("@headerName", header.Name);
            cmd.Parameters.AddWithValue("@headerOrder", header.Order);
            cmd.Parameters.AddWithValue("@headerId", header.Id);
            int affectedRows = cmd.ExecuteNonQuery();

            if (affectedRows == 0)
                AddHeader(header);
        }
        #endregion



        #region ValidationMethods
        /// <summary>
        /// Checks if a table exists
        /// </summary>
        /// <param name="table">table to check</param>
        /// <returns>true if the table exists, otherwise false</returns>
        private bool DoesTableExist(DatabaseTables table)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name=@name";
            cmd.Parameters.AddWithValue("@name", Enum.GetName(table));
            using SQLiteDataReader reader = cmd.ExecuteReader();
            return reader.HasRows;
        }
        #endregion
    }


    /// <summary>
    /// Database tables
    /// </summary>
    public enum DatabaseTables
    {
        MonthlyEntry,
        Payor,
        Header,
        Subheader,
        Rows,
        CellEntryToRow
    }



    /// <summary>
    /// Months of the year
    /// </summary>
    public enum Month
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12,
        All = 13 // Used to represent the year
    }



    public enum ChangeState
    {
        Added,
        Removed,
        Edited,
        Unchanged
    }

}
