//***********************************************************************************
//Program: ILogger.cs
//Description: Logger interface
//Date: Sept 17, 2025
//Author: John Nasitem
//***********************************************************************************



using static PayorLedger.Services.Logger.Logger;



namespace PayorLedger.Services.Logger
{
    public interface ILogger
    {
        /// <summary>
        /// Add log to the current file
        /// </summary>
        /// <param name="message">Message to add</param>
        /// <param name="type">Type of log</param>
        public void AddLog(string message, LogType type);
    }
}
