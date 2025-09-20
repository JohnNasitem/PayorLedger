//***********************************************************************************
//Program: ColumnsWindowViewModel.cs
//Description: View model for columns window
//Date: Sep 4, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Dialogs;
using PayorLedger.Models.Columns;
using PayorLedger.Services.Actions;
using PayorLedger.Services.Actions.HeaderCommands;
using PayorLedger.Services.Actions.SubheaderCommands;
using PayorLedger.Services.Logger;
using PayorLedger.Windows.Columns;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace PayorLedger.ViewModels
{
    public class ColumnsWindowViewModel : WindowViewModel
    {
        /// <summary>
        /// Window the view model is associated to
        /// </summary>
        public override Window Window { get; protected set; }



        /// <summary>
        /// Columns in the list
        /// </summary>
        public ObservableCollection<ColumnListItem> Columns { get; set; } = [];



        private readonly IUndoRedoService _undoRedoService = App.ServiceProvider.GetRequiredService<IUndoRedoService>();
        private readonly ILogger _logger = App.ServiceProvider.GetRequiredService<ILogger>();



        public ColumnsWindowViewModel()
        {
            Window = new ColumnsWindow(this);
        }



        /// <summary>
        /// Update the window ui
        /// </summary>
        public void UpdateUI()
        {
            List<HeaderEntry> headers = App.ServiceProvider.GetRequiredService<MainPageViewModel>().Headers.Where(h => h.State != Services.Database.ChangeState.Removed).OrderBy(h => h.Order).ToList();
            int lastOldIndex = Columns.Count;

            foreach (HeaderEntry header in headers)
            {
                Columns.Add(new ColumnListItem(header.Name, header));

                List<SubheaderEntry> subheaders = header.Subheaders.OrderBy(s => s.Order).ToList();

                for (int i = 0; i < subheaders.Count; i++)
                    Columns.Add(new ColumnListItem((subheaders.Count - 1 == i ? "└ " : "⊢ ") + subheaders[i].Name, subheaders[i]));
            }

            while (lastOldIndex > 0)
            {
                Columns.RemoveAt(0);
                lastOldIndex--;
            }

            ((ColumnsWindow)Window).UpdateButtonStates();
        }



        /// <summary>
        /// Prompt user to add header
        /// </summary>
        public void AddHeader()
        {
            App.ServiceProvider.GetRequiredService<MainPageViewModel>().ExecuteAddHeader();
        }



        /// <summary>
        /// Prompt user to add subheader
        /// </summary>
        public void AddSubheader()
        {
            App.ServiceProvider.GetRequiredService<MainPageViewModel>().ExecuteAddSubheader();
        }


        
        /// <summary>
        /// Prompt user to edit the column
        /// </summary>
        /// <param name="index">Index of item being editted</param>
        public void EditColumn(int index)
        {
            ColumnListItem item = Columns[index];

            if (item.Value is SubheaderEntry subheaderEntry)
            {
                AddSubheaderDialog dlg = new(subheaderEntry);

                bool? result = dlg.ShowDialog();

                if (result == true)
                {
                    _logger.AddLog($"Attempting to edit subheader. Name: \"{dlg.SubheaderName}\" - Order: \"{dlg.SubheaderOrder}\" - Parent ID: \"{dlg.ParentHeader.Id}\"", Logger.LogType.PreAction);

                    _undoRedoService.Execute(new EditSubheaderCommand(subheaderEntry, dlg.SubheaderName, dlg.ParentHeader, dlg.SubheaderOrder));
                }
            }
            else if (item.Value is HeaderEntry headerEntry)
            {
                AddHeaderDialog dlg = new(headerEntry);

                bool? result = dlg.ShowDialog();

                if (result == true)
                {
                    _logger.AddLog($"Attempting to edit subheader. Name: \"{dlg.HeaderName}\" - Order: \"{dlg.HeaderOrder}\"", Logger.LogType.PreAction);

                    _undoRedoService.Execute(new EditHeaderCommand(headerEntry, dlg.HeaderName, dlg.HeaderOrder));
                }
            }
        }



        /// <summary>
        /// Prompt user to delete the column
        /// </summary>
        /// <param name="index">Index of item being editted</param>
        public void DeleteColumn(int index)
        {
            ColumnListItem item = Columns[index];

            bool isHeader = Columns[index].Value is HeaderEntry;

            ConfirmationDialog confirmationDlg = new(
                $"Delete {(isHeader ? "Header" : "Subheader")}",
                $"Are you sure you want to delete the {(isHeader ? "Header" : "Subheader")} '{(isHeader ? ((HeaderEntry)Columns[index].Value).Name : ((SubheaderEntry)Columns[index].Value).Name)}'?{(isHeader ? "This will also delete ALL the subheaders under this header." : " ")}",
                Brushes.Red);

            bool? result = confirmationDlg.ShowDialog();

            // Delete column
            if (result == true)
            {
                _logger.AddLog($"Attempting to delete column. IsHeader: \"{isHeader}\" - Index: \"{index}\" - Columns List Count: \"{Columns.Count}\"", Logger.LogType.PreAction);

                if (isHeader)
                    _undoRedoService.Execute(new DeleteHeaderCommand((HeaderEntry)Columns[index].Value));
                else
                    _undoRedoService.Execute(new DeleteSubheaderCommand((SubheaderEntry)Columns[index].Value));
            }
        }



        public class ColumnListItem(string name, IColumn value)
        {
            public string Name { get; set; } = name;
            public IColumn Value { get; set; } = value;
        }
    }
}
