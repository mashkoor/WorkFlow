using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace JPClientService
{
    public class WorkflowAction : AsyncCodeActivity
    {
        #region Properties

        public Action Action { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///   When implemented in a derived class and using the specified execution context, callback method, and user state, enqueues an asynchronous activity in a run-time workflow.
        /// </summary>
        /// <returns>
        ///   The object that saves variable information for an instance of an asynchronous activity.
        /// </returns>
        /// <param name = "context">Information that defines the execution environment for the <see
        ///    cref = "T:System.Activities.AsyncCodeActivity" />.</param>
        /// <param name = "callback">The method to be called after the asynchronous activity and completion notification have occurred.</param>
        /// <param name = "state">An object that saves variable information for an instance of an asynchronous activity.</param>
        protected override IAsyncResult BeginExecute(
            AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var task = Task.Factory.StartNew(obj => this.Action(), state);

            if (callback != null)
            {
                task.ContinueWith(res => callback(task));
            }

            return task;
        }

        /// <summary>
        ///   When implemented in a derived class and using the specified execution environment information, notifies the workflow runtime that the associated asynchronous activity operation has completed.
        /// </summary>
        /// <param name = "context">Information that defines the execution environment for the <see
        ///    cref = "T:System.Activities.AsyncCodeActivity" />.</param>
        /// <param name = "result">The implemented <see cref = "T:System.IAsyncResult" /> that returns the status of an asynchronous activity when execution ends.</param>
        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            ((Task)result).Wait();
        }

        #endregion
    }

    public class WorkflowAction<T1> : AsyncCodeActivity
    {
        #region Properties

        public Action<T1> Action { get; set; }

        public InArgument<T1> Arg1 { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///   When implemented in a derived class and using the specified execution context, callback method, and user state, enqueues an asynchronous activity in a run-time workflow.
        /// </summary>
        /// <returns>
        ///   The object that saves variable information for an instance of an asynchronous activity.
        /// </returns>
        /// <param name = "context">Information that defines the execution environment for the <see
        ///    cref = "T:System.Activities.AsyncCodeActivity" />.</param>
        /// <param name = "callback">The method to be called after the asynchronous activity and completion notification have occurred.</param>
        /// <param name = "state">An object that saves variable information for an instance of an asynchronous activity.</param>
        protected override IAsyncResult BeginExecute(
            AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var arg = this.Arg1.Get(context);
            var task = Task.Factory.StartNew(obj => this.Action(arg), state);

            if (callback != null)
            {
                task.ContinueWith(res => callback(task));
            }

            return task;
        }

        /// <summary>
        ///   When implemented in a derived class and using the specified execution environment information, notifies the workflow runtime that the associated asynchronous activity operation has completed.
        /// </summary>
        /// <param name = "context">Information that defines the execution environment for the <see
        ///    cref = "T:System.Activities.AsyncCodeActivity" />.</param>
        /// <param name = "result">The implemented <see cref = "T:System.IAsyncResult" /> that returns the status of an asynchronous activity when execution ends.</param>
        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            ((Task)result).Wait();
        }

        #endregion
    }
}