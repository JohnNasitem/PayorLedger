//***********************************************************************************
//Program: AddCommentCommand.cs
//Description: Add comment command
//Date: Sep 15, 2025
//Author: John Nasitem
//***********************************************************************************



using PayorLedger.Models;

namespace PayorLedger.Services.Actions.CommentCommands
{
    public class AddCommentCommand : CommentCommand
    {
        public AddCommentCommand(PayorComments commentEntry) : base(commentEntry) { }



        /// <summary>
        /// Execute the command to edit a comment
        /// </summary>
        public override void Execute() => AddComment();



        /// <summary>
        /// Undo the command to edit a comment
        /// </summary>
        public override void Undo() => DeleteComment();
    }
}
