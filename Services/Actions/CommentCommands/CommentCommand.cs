//***********************************************************************************
//Program: CommentCommand.cs
//Description: Base class for comment commands
//Date: Sep 15, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Models;
using PayorLedger.Services.Database;
using PayorLedger.ViewModels;

namespace PayorLedger.Services.Actions.CommentCommands
{
    public abstract class CommentCommand : IUndoableCommand
    {
        public PayorComments Comment { get; }

        protected static readonly MainPageViewModel _mainPageViewModel = App.ServiceProvider.GetRequiredService<MainPageViewModel>();



        public CommentCommand(PayorComments comment)
        {
            Comment = comment;
        }



        /// <summary>
        /// Add a comment to the main page
        /// </summary>
        public void AddComment()
        {
            Comment.State = ChangeState.Added;

            // Add it if it doesnt exist already
            // It shouldn't but just to be safe
            if (!_mainPageViewModel.LedgerComments.Any(e => e.PayorId == Comment.PayorId && e.Month == Comment.Month && e.Year == Comment.Year))
                _mainPageViewModel.LedgerComments.Add(Comment);

            _mainPageViewModel.UpdateTotals();
        }



        /// <summary>
        /// Mark comment as deleted
        /// </summary>
        public void DeleteComment()
        {
            Comment.State = ChangeState.Removed;

            _mainPageViewModel.UpdateTotals();
        }



        /// <summary>
        /// Edit a comment in the main page
        /// </summary>
        /// <param name="newComment">New comment</param>
        public void EditComment(string newComment)
        {
            Comment.State = ChangeState.Edited;



            _mainPageViewModel.UpdateTotals();
        }



        // IUndoableCommand methods
        public abstract void Execute();
        public abstract void Undo();
    }
}
