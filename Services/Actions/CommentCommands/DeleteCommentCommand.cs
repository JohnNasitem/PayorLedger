//***********************************************************************************
//Program: DeleteCommentCommand.cs
//Description: Delete comment command
//Date: Sep 15, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;

namespace PayorLedger.Services.Actions.CommentCommands
{
    public class DeleteCommentCommand : CommentCommand
    {
        public DeleteCommentCommand(PayorComments commentEntry) : base(commentEntry) { }



        /// <summary>
        /// Execute the command to delete a comment
        /// </summary>
        public override void Execute() => DeleteComment();



        /// <summary>
        /// Undo the command to delete a comment
        /// </summary>
        public override void Undo() => AddComment();
    }
}
