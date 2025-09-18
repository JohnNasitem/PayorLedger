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
        int _originalOrNum;
        int _newOrNum;
        string _originalComment;
        string _newComment;
        long _originalPayorId;
        long _newPayorId;



        public EditRowCommand(RowEntry row, string newDate, int newOrNum, string newComment, long newPayorId) : base(row)
        {
            _originalDate = row.Date;
            _newDate = newDate;
            _originalOrNum = row.OrNum;
            _newOrNum = newOrNum;
            _originalComment = row.Comment;
            _newComment = newComment;
            _originalPayorId = row.PayorId;
            _newPayorId = newPayorId;
        }


        /// <summary>
        /// Execute the command to add a row
        /// </summary>
        public override void Execute() => EditRow(_newDate, _newOrNum, _newPayorId, _newComment);



        /// <summary>
        /// Undo the command to add a row
        /// </summary>
        public override void Undo() => EditRow(_originalDate, _originalOrNum, _originalPayorId, _originalComment);
    }
}
