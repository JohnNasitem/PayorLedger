//***********************************************************************************
//Program: DatabaseService.cs
//Description: Interact with the database
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Enums;
using PayorLedger.Models;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Actions;
using PayorLedger.ViewModels;
using System.Data.SQLite;

namespace PayorLedger.Services.Database
{
    public class DatabaseService : IDatabaseService
    {
        /// <summary>
        /// List of names that are not allowed to be used for payors, headers, or subheaders
        /// </summary>
        public string[] InvalidNames { get; } = ["", "total", "payor", "comments"];



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
                                        PayorName TEXT NOT NULL,
                                        Label TEXT NOT NULL
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

            // Initialize PayorToColumnTable table
            if (!DoesTableExist(DatabaseTables.PayorToColumn))
            {
                using SQLiteCommand cmd = _sqlConnection.CreateCommand();
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.PayorToColumn)}(
                                        SubHeaderId INTEGER NOT NULL,
                                        PayorId INTEGER NOT NULL,
                                        Amount NUMERIC NOT NULL,
                                        Month INTEGER NOT NULL,
                                        Year INTEGER NOT NULL,
                                        FOREIGN KEY (SubHeaderId) REFERENCES {Enum.GetName(DatabaseTables.Subheader)}(SubHeaderId),
                                        FOREIGN KEY (PayorId) REFERENCES {Enum.GetName(DatabaseTables.Payor)}(PayorId),
                                        PRIMARY KEY (PayorId, SubHeaderId, Month, Year)
                                        )";
                cmd.ExecuteNonQuery();
            }



            // Initialize PayorCommentTable table
            if (!DoesTableExist(DatabaseTables.PayorComments))
            {
                using SQLiteCommand cmd = _sqlConnection.CreateCommand();
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {Enum.GetName(DatabaseTables.PayorComments)}(
                                        PayorId INTEGER NOT NULL,
                                        Comment TEXT NOT NULL,
                                        Month INTEGER NOT NULL,
                                        Year INTEGER NOT NULL,
                                        FOREIGN KEY (PayorId) REFERENCES {Enum.GetName(DatabaseTables.Payor)}(PayorId),
                                        PRIMARY KEY (PayorId, Month, Year)
                                        )";
                cmd.ExecuteNonQuery();
            }
        }



        /// <summary>
        /// Sync changes to the database
        /// </summary>
        public void SaveChanges()
        {
            // Get view model data
            (List<PayorEntry> payors, List<HeaderEntry> headers, List<PayorToColumnEntry> entries, List<PayorComments> comments) = App.ServiceProvider.GetRequiredService<MainPageViewModel>().GetVMData();

            List<object> objectsToRemove = [];

            // Sync payor data
            foreach (PayorEntry payor in payors)
            {
                switch (payor.State)
                {
                    case ChangeState.Added:
                        long payorId = AddPayor(payor.PayorName, payor.Label);

                        // Replace temp id with new id
                        foreach (PayorToColumnEntry entry in entries.Where(e => e.PayorId == payor.PayorId))
                            entry.PayorId = payorId;
                        foreach (PayorComments comment in comments.Where(c => c.PayorId == payor.PayorId))
                            comment.PayorId = payorId;

                        payor.PayorId = payorId;
                        break;
                    case ChangeState.Removed:
                        DeletePayor(payor.PayorId);
                        objectsToRemove.Add(payor);
                        break;
                    case ChangeState.Edited:
                        EditPayorEntry(payor, payor.PayorName, payor.Label);
                        break;
                }

                payor.State = ChangeState.Unchanged;
            }

            // Remove deleted payors from list
            foreach (PayorEntry payor in objectsToRemove.Cast<PayorEntry>())
                payors.RemoveAll(p => p.PayorId == payor.PayorId);
            objectsToRemove.Clear();

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
                        objectsToRemove.Add(header);
                        break;
                    case ChangeState.Edited:
                        EditHeaderEntry(header, header.Name, header.Order);
                        break;
                }

                header.State = ChangeState.Unchanged;
            }

            // Remove deleted headers from list
            foreach (HeaderEntry header in objectsToRemove.Cast<HeaderEntry>())
                headers.RemoveAll(h => h.Id == header.Id);
            objectsToRemove.Clear();

            // Sync subheaders
            foreach (SubheaderEntry subheader in headers.SelectMany(h => h.Subheaders))
            {
                switch (subheader.State)
                {
                    case ChangeState.Added:
                        long subheaderId = AddSubheader(subheader.Name, subheader.Header.Id, subheader.Order);

                        // Replace temp id with new id
                        foreach (PayorToColumnEntry entry in entries.Where(e => e.SubheaderId == subheader.Id))
                            entry.SubheaderId = subheaderId;

                        subheader.Id = subheaderId;
                        break;
                    case ChangeState.Removed:
                        // Dont need to save to delete later as DeleteHeader() will handle subheaders as well
                        subheader.Header.Subheaders.RemoveAll(s => s.Id == subheader.Id);
                        break;
                    case ChangeState.Edited:
                        EditSubheaderEntry(subheader, subheader.Name, subheader.Header.Id, subheader.Order);
                        break;
                }

                subheader.State = ChangeState.Unchanged;
            }

            // Sync cell entries
            foreach (PayorToColumnEntry entry in entries)
            {
                switch (entry.State)
                {
                    case ChangeState.Added:
                        AddCellEntry(entry);
                        break;
                    case ChangeState.Removed:
                        DeleteCellEntry(entry);
                        objectsToRemove.Add(entry);
                        break;
                    case ChangeState.Edited:
                        EditCellEntry(entry);
                        break;
                }

                entry.State = ChangeState.Unchanged;
            }

            // Remove deleted entries from list
            foreach (PayorToColumnEntry entry in objectsToRemove.Cast<PayorToColumnEntry>())
                entries.RemoveAll(e => e.PayorId == entry.PayorId && e.SubheaderId == entry.SubheaderId && e.Month == entry.Month && e.Year == entry.Year);
            objectsToRemove.Clear();

            // Synce comments
            foreach (PayorComments comment in comments)
            {
                switch (comment.State)
                {
                    case ChangeState.Added:
                        AddCommentEntry(comment);
                        break;
                    case ChangeState.Removed:
                        DeleteComment(comment);
                        objectsToRemove.Add(comment);
                        break;
                    case ChangeState.Edited:
                        EditCommentEntry(comment);
                        break;
                }
                comment.State = ChangeState.Unchanged;
            }

            // Remove deleted comments from list
            foreach (PayorComments comment in objectsToRemove.Cast<PayorComments>())
                comments.RemoveAll(c => c.PayorId == comment.PayorId && c.Month == comment.Month && c.Year == comment.Year);

            AllChangesSaved = true;
            App.ServiceProvider.GetRequiredService<IUndoRedoService>().OnChangeOccured(true);
        }



        #region GetMethods
        /// <summary>
        /// Get all the entries
        /// </summary>>
        /// <returns>Data for the specified year</returns>
        public (List<PayorToColumnEntry> Entries, List<PayorComments> Comments)  GetData()
        {
            List<PayorToColumnEntry> entries = [];
            List<PayorComments> comments = [];

            // Get cell entries
            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.PayorToColumn)}";

                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    entries.Add(new PayorToColumnEntry(
                        subheaderId: long.Parse(reader["SubHeaderId"].ToString()!),
                        payorId: long.Parse(reader["PayorId"].ToString()!),
                        amount: decimal.Parse(reader["Amount"].ToString()!),
                        month: (Month)int.Parse(reader["Month"].ToString()!),
                        year: int.Parse(reader["Year"].ToString()!),
                        state: ChangeState.Unchanged
                    ));
                }
            }

            // Get comments
            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                cmd.CommandText = $@"SELECT * FROM {Enum.GetName(DatabaseTables.PayorComments)}";

                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    comments.Add(new PayorComments(
                        payorId: long.Parse(reader["PayorId"].ToString()!),
                        comment: reader["Comment"].ToString()!,
                        month: (Month)int.Parse(reader["Month"].ToString()!),
                        year: int.Parse(reader["Year"].ToString()!),
                        state: ChangeState.Unchanged
                    ));
                }
            }

            return (entries, comments);
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
                        label: Enum.Parse<PayorEnums.PayorLabel>(reader["Label"].ToString()!),
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
            cmd.CommandText = $@"DELETE FROM {Enum.GetName(DatabaseTables.PayorToColumn)} WHERE PayorId = @payorId";
            cmd.ExecuteNonQuery();

            // Delete comment entries associated with the payor
            cmd.CommandText = $@"DELETE FROM {Enum.GetName(DatabaseTables.PayorComments)} WHERE PayorId = @payorId";
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
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.PayorToColumn)} WHERE SubheaderId = @subheaderId";
            cmd.ExecuteNonQuery();

            // Delete entry from the Subheader table
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.Subheader)} WHERE SubheaderId = @subheaderId";
            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Delete a cell entry from the database
        /// </summary>
        /// <param name="entry">Entry to delete</param>
        private void DeleteCellEntry(PayorToColumnEntry entry)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.PayorToColumn)} WHERE PayorId = @payorId AND SubHeaderId = @subheaderId AND Month = @month AND Year = @year";
            cmd.Parameters.AddWithValue("@payorId", entry.PayorId);
            cmd.Parameters.AddWithValue("@subheaderId", entry.SubheaderId);
            cmd.Parameters.AddWithValue("@month", (int)entry.Month);
            cmd.Parameters.AddWithValue("@year", entry.Year);
            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Delete a comment entry from the database
        /// </summary>
        /// <param name="comment">Entry to delete</param>
        private void DeleteComment(PayorComments comment)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"DELETE from {Enum.GetName(DatabaseTables.PayorComments)} WHERE PayorId = @payorId AND Month = @month AND Year = @year";
            cmd.Parameters.AddWithValue("@payorId", comment.PayorId);
            cmd.Parameters.AddWithValue("@month", (int)comment.Month);
            cmd.Parameters.AddWithValue("@year", comment.Year);
            cmd.ExecuteNonQuery();
        }
        #endregion



        #region AddMethods
        /// <summary>
        /// Add a payor to the database
        /// </summary>
        /// <param name="payorName">Name of payor</param>
        /// <param name="label">Label of the payor</param>
        /// <returns>Id of the new payor</returns>
        private long AddPayor(string payorName, PayorEnums.PayorLabel label)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.Payor)} (PayorName, Label) VALUES (@payorName, @label)";
            cmd.Parameters.AddWithValue("@payorName", payorName);
            cmd.Parameters.AddWithValue("@label", label.ToString());
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            return (long)cmd.ExecuteScalar();

        }



        /// <summary>
        /// Add a header to the database
        /// </summary>
        /// <param name="headerName">Name of new header</param>
        /// <returns>Id of the new header</returns>
        private long AddHeader(string headerName, int headerOrder)
        {
            // Add entry to the header table
            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.Header)} (HeaderName, HeaderOrder) VALUES (@headerName, @headerOrder)";
                cmd.Parameters.AddWithValue("@headerName", headerName);
                cmd.Parameters.AddWithValue("@headerOrder", headerOrder);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT last_insert_rowid();";
                return (long)cmd.ExecuteScalar();
            }
        }



        /// <summary>
        /// Add a subheader to the database
        /// </summary>
        /// <param name="subheaderName">Name of new subheader</param>
        /// <param name="headerId">Id of header the subheader is being added to</param>
        /// <param name="subheaderOrder">Order of header the subheader is being added to</param>
        ///  <returns>Id of the new subheader</returns>
        /// <exception cref="ArgumentException">Throws exception if the specified table isnt <see cref="DatabaseTables.Payor"/>, <see cref="DatabaseTables.Header"/>, or <see cref="DatabaseTables.Subheader"/></exception> 
        private long AddSubheader(string subheaderName, long headerId, int subheaderOrder)
        {
            // Add entry to the subheader table
            using (SQLiteCommand cmd = _sqlConnection.CreateCommand())
            {
                cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.Subheader)} (SubheaderName, HeaderId, SubHeaderOrder) VALUES (@subheaderName, @headerId, @subHeaderOrder)";
                cmd.Parameters.AddWithValue("@subheaderName", subheaderName);
                cmd.Parameters.AddWithValue("@headerId", headerId);
                cmd.Parameters.AddWithValue("@subHeaderOrder", subheaderOrder);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT last_insert_rowid();";
                return (long)cmd.ExecuteScalar();
            }
        }



        /// <summary>
        /// Add a entry to the PayoToColumn database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        private void AddCellEntry(PayorToColumnEntry entry)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.PayorToColumn)} (SubHeaderId, PayorId, Amount, Month, Year) VALUES (@subheaderId, @payorId, @newAmount, @month, @year)";
            cmd.Parameters.AddWithValue("@newAmount", entry.Amount);
            cmd.Parameters.AddWithValue("@subheaderId", entry.SubheaderId);
            cmd.Parameters.AddWithValue("@payorId", entry.PayorId);
            cmd.Parameters.AddWithValue("@month", (int)entry.Month);
            cmd.Parameters.AddWithValue("@year", entry.Year);
            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Add a entry to the PayorComments database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        private void AddCommentEntry(PayorComments entry)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"INSERT INTO {Enum.GetName(DatabaseTables.PayorComments)} (PayorId, Comment, Month, Year) VALUES (@payorId, @newComment, @month, @year)";
            cmd.Parameters.AddWithValue("@newComment", entry.Comment);
            cmd.Parameters.AddWithValue("@payorId", entry.PayorId);
            cmd.Parameters.AddWithValue("@month", (int)entry.Month);
            cmd.Parameters.AddWithValue("@year", entry.Year);
            cmd.ExecuteNonQuery();
        }
        #endregion



        #region EditMethods
        /// <summary>
        /// Edit an existing payor
        /// </summary>
        /// <param name="payor">Payor to edit</param>
        /// /// <param name="newName">New payor name</param>
        /// /// <param name="newLabel">New payor label</param>
        private void EditPayorEntry(PayorEntry payor, string newName, PayorEnums.PayorLabel newLabel)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.Payor)} SET PayorName = @payorName, Label = @label WHERE PayorId = @payorId";
            cmd.Parameters.AddWithValue("@payorName", newName);
            cmd.Parameters.AddWithValue("@label", newLabel.ToString());
            cmd.Parameters.AddWithValue("@payorId", payor.PayorId);
            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Update a cell entry
        /// </summary>
        /// <param name="entry">New entry values</param>
        private void EditCellEntry(PayorToColumnEntry entry)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.PayorToColumn)} SET Amount = @newAmount WHERE PayorId = @payorId AND SubHeaderId = @subheaderId AND Month = @month AND Year = @year";
            cmd.Parameters.AddWithValue("@newAmount", entry.Amount);
            cmd.Parameters.AddWithValue("@subheaderId", entry.SubheaderId);
            cmd.Parameters.AddWithValue("@payorId", entry.PayorId);
            cmd.Parameters.AddWithValue("@month", (int)entry.Month);
            cmd.Parameters.AddWithValue("@year", entry.Year);
            int affectRows = cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Update a comment entry
        /// </summary>
        /// <param name="entry">New entry values</param>
        private void EditCommentEntry(PayorComments entry)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.PayorComments)} SET Comment = @newComment WHERE PayorId = @payorId AND Month = @month AND Year = @year";
            cmd.Parameters.AddWithValue("@newComment", entry.Comment);
            cmd.Parameters.AddWithValue("@payorId", entry.PayorId);
            cmd.Parameters.AddWithValue("@month", entry.Month.ToString());
            cmd.Parameters.AddWithValue("@year", entry.Year);
            int affectRows = cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Update a subheader entry
        /// </summary>
        /// <param name="subheader">Subheader being editted</param>
        /// <param name="newName">New subheader name</param>
        /// <param name="newParentHeaderId">New parent header id</param>
        /// <param name="newOrder">New subheader order</param>
        private void EditSubheaderEntry(SubheaderEntry subheader, string newName, long newParentHeaderId, int newOrder)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.Subheader)} SET HeaderID = @headerId, SubHeaderName = @subheaderName, SubHeaderOrder = @subheaderOrder WHERE SubHeaderId = @subheaderId";
            cmd.Parameters.AddWithValue("@headerId", newParentHeaderId);
            cmd.Parameters.AddWithValue("@subheaderName", newName);
            cmd.Parameters.AddWithValue("@subheaderOrder", newOrder);
            cmd.Parameters.AddWithValue("@subheaderId", subheader.Id);
            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Update a header entry
        /// </summary>
        /// <param name="header">Header being editted</param>
        /// <param name="newName">New header name</param>
        /// <param name="newOrder">New header order</param>
        private void EditHeaderEntry(HeaderEntry header, string newName, int newOrder)
        {
            using SQLiteCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = $@"UPDATE {Enum.GetName(DatabaseTables.Header)} SET HeaderName = @headerName, HeaderOrder = @headerOrder WHERE HeaderId = @headerId";
            cmd.Parameters.AddWithValue("@headerName", newName);
            cmd.Parameters.AddWithValue("@headerOrder", newOrder);
            cmd.Parameters.AddWithValue("@headerId", header.Id);
            cmd.ExecuteNonQuery();
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
        PayorToColumn,
        PayorComments
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
