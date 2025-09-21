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
        private RowEntry.RowLabel _originalLabel;
        private RowEntry.RowLabel _newLabel;
        private long _originalPayorId;
        private long _newPayorId;
        private int _originalOrNum;
        private int _newOrNum;
        private string _originalDate;
        private string _newDate;
        private string _originalComment;
        private string _newComment;



        public EditRowCommand(RowEntry row, RowEntry.RowLabel newLabel, long newPayorId, int newOrNum, string newDate, string newComment) : base(row)
        {
            _originalLabel = row.Label;
            _newLabel = newLabel;
            _originalPayorId = row.PayorId;
            _newPayorId = newPayorId;
            _originalOrNum = row.OrNum;
            _newOrNum = newOrNum;
            _originalDate = row.Date;
            _newDate = newDate;
            _originalComment = row.Comment;
            _newComment = newComment;
        }


        /// <summary>
        /// Execute the command to add a row
        /// </summary>
        public override void Execute() => EditRow(_newLabel, _newPayorId, _newOrNum, _newDate, _newComment);



        /// <summary>
        /// Undo the command to add a row
        /// </summary>
        public override void Undo() => EditRow(_originalLabel, _originalPayorId, _originalOrNum, _originalDate, _originalComment);
    }
}
