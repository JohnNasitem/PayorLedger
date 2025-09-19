//***********************************************************************************
//Program: AddRowCommand.cs
//Description: Add row command
//Date: Sep 18, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.ViewModels;

namespace PayorLedger.Services.Actions.RowCommands
{
    public class AddRowCommand : RowCommand
    {
        public AddRowCommand(RowEntry row) : base(row) { }


        /// <summary>
        /// Execute the command to add a row
        /// </summary>
        public override void Execute() => AddRow();



        /// <summary>
        /// Undo the command to add a row
        /// </summary>
        public override void Undo() => DeleteRow();
    }
}
