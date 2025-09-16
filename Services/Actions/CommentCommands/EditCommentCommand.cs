//***********************************************************************************
//Program: EditCommentCommand.cs
//Description: Edit comment command
//Date: July 26, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;

namespace PayorLedger.Services.Actions.CommentCommands
{
    public class EditCommentCommand : CommentCommand
    {
        private readonly string _originalComment;
        private readonly string _newComment;



        public EditCommentCommand(PayorComments commentEntry, string newComment) : base(commentEntry)
        {
            _originalComment = commentEntry.Comment;
            _newComment = newComment;
        }



        /// <summary>
        /// Execute the command to edit a comment
        /// </summary>
        public override void Execute() => EditComment(_newComment);



        /// <summary>
        /// Undo the command to edit a comment
        /// </summary>
        public override void Undo() => EditComment(_originalComment);
    }
}
