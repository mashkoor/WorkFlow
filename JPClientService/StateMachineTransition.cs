using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace JPClientService
{
    public class StateMachineTransition<TState, TTrigger>
    {
        #region Properties

        public Action Action { get; set; }

        public bool AutoTrigger { get; set; }

        public Func<bool> Condition { get; set; }

        public StateMachineState<TState, TTrigger> Destination { get; set; }

        public string Name { get; set; }

        public StateMachineState<TState, TTrigger> Source { get; internal set; }

        public TTrigger Trigger { get; set; }

        public Transition WorkflowTransition { get; private set; }

        #endregion

        //public Transition Resolve(IDictionary<TState, StateMachineState<TState, TTrigger>> states)
        //{
        //    // TODO: Consider throwing another exception here if the destination is not found
        //    // this will throw KeyNotFoundException
        //    var destination = states[this.Destination];

        //    this.CreateTransition(destination);

        //    return this.WorkflowTransition;
        //}

        #region Public Methods

        public Transition CreateTransition()
        {
            // Create a new transition or use an existing one which already has a WorkflowTransition
            var foundTransition = this.Source.Transitions.Find(t => t.Trigger.Equals(this.Trigger) && t.WorkflowTransition != null && t != this);

            // If a transition was found with the same trigger, use the same trigger activity as a shared trigger
            this.WorkflowTransition = new Transition
            {
                DisplayName = this.Name ?? "To " + this.Destination.DisplayName,
                Trigger = this.AutoTrigger
                              ? null
                              : foundTransition != null
                                    ? foundTransition.WorkflowTransition.Trigger
                                    : new BookmarkTrigger { BookmarkName = this.Trigger.ToString(), DisplayName = "Trigger " + this.Trigger },
                To = this.Destination.WorkflowState,
                Action = this.Action != null
                             ? new WorkflowAction { Action = this.Action }
                             : null,
                Condition = this.Condition != null
                                ? new WorkflowCondition { Condition = this.Condition }
                                : null
            };

            return this.WorkflowTransition;
        }

        #endregion
    }

    public class BookmarkTrigger : NativeActivity
    {
        #region Properties

        public string BookmarkName { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///   When implemented in a derived class, runs the activity’s execution logic.
        /// </summary>
        /// <param name = "context">The execution context in which the activity executes.</param>
        protected override void Execute(NativeActivityContext context)
        {
            context.CreateBookmark(this.BookmarkName, BookmarkCallback);
        }

        private static void BookmarkCallback(NativeActivityContext context, Bookmark bookmark, object value)
        {
            // TODO: How can we pass the bookmark value safely to a trigger condition?
        }

        #endregion
    }

    public class WorkflowCondition : AsyncCodeActivity<bool>
    {
        #region Properties

        public Func<bool> Condition { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///   When implemented in a derived class and using the specified execution context, callback method, and user state, enqueues an asynchronous activity in a run-time workflow.
        /// </summary>
        /// <returns>
        ///   An object.
        /// </returns>
        /// <param name = "context">Information that defines the execution environment for the <see
        ///    cref = "T:System.Activities.AsyncCodeActivity" />.</param>
        /// <param name = "callback">The method to be called after the asynchronous activity and completion notification have occurred.</param>
        /// <param name = "state">An object that saves variable information for an instance of an asynchronous activity.</param>
        protected override IAsyncResult BeginExecute(
            AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var task = Task<Boolean>.Factory.StartNew(obj => this.Condition(), state);

            if (callback != null)
            {
                task.ContinueWith(res => callback(task));
            }

            return task;
        }

        /// <summary>
        ///   When implemented in a derived class and using the specified execution environment information, notifies the workflow runtime that the associated asynchronous activity operation has completed.
        /// </summary>
        /// <returns>
        ///   A generic type.
        /// </returns>
        /// <param name = "context">Information that defines the execution environment for the <see
        ///    cref = "T:System.Activities.AsyncCodeActivity" />.</param>
        /// <param name = "result">The implemented <see cref = "T:System.IAsyncResult" /> that returns the status of an asynchronous activity when execution ends.</param>
        protected override bool EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            return ((Task<Boolean>)result).Result;
        }

        #endregion
    }
}