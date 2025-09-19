//***********************************************************************************
//Program: EditRowCommand.cs
//Description: Edit row command
//Date: Sep 18, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;

namespace PayorLedger.Services.Actions.RowCommands
{
    public class EditRowCommand : RowCommand
    {
        string _originalDate;
        string _newDate;
        string _originalComment;
        string _newComment;



        public EditRowCommand(RowEntry row, string newDate, string newComment) : base(row)
        {
            _originalDate = row.Date;
            _newDate = newDate;
            _originalComment = row.Comment;
            _newComment = newComment;
        }


        /// <summary>
        /// Execute the command to add a row
        /// </summary>
        public override void Execute() => EditRow(_newDate, _newComment);



        /// <summary>
        /// Undo the command to add a row
        /// </summary>
        public override void Undo() => EditRow(_originalDate, _originalComment);
    }
}
