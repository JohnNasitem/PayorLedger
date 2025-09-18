//***********************************************************************************
//Program: Logger.cs
//Description: Logs actions done by user and any exceptions
//Date: Sept 17, 2025
//Author: John Nasitem
//***********************************************************************************



using System.IO;



namespace PayorLedger.Services.Logger
{
    public class Logger : ILogger
    {
        private static readonly string _filePath = $"Logs/{DateTime.UtcNow.ToString("yyyy-MM-dd")}";



        public Logger()
        {
            // Create log file
            AddLog("User openned application.", LogType.Action);
        }



        /// <summary>
        /// Add log to the current file
        /// </summary>
        /// <param name="message">Message to add</param>
        /// <param name="type">Type of log</param>
        public void AddLog(string message, LogType type)
        {
            File.WriteAllText(_filePath, $"[{DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss")}] {type.ToString().ToUpper()}: {message + Environment.NewLine}");
        }



        /// <summary>
        /// Log type
        /// </summary>
        public enum LogType
        {
            Action = 0,
            Error = 1,
        }
    }
}
