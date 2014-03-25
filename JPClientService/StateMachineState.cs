using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace JPClientService
{
    public class StateMachineState<TState, TTrigger>
    {
        #region Constants and Fields

        private readonly TState key;

        private readonly StateMachine<TState, TTrigger> stateMachine;

        private readonly State workflowState;

        #endregion

        #region Constructors and Destructors

        public StateMachineState(TState key, StateMachine<TState, TTrigger> stateMachine, string name = null)
        {
            this.Transitions = new List<StateMachineTransition<TState, TTrigger>>();

            this.stateMachine = stateMachine;
            this.key = key;
            this.workflowState = new State { DisplayName = name ?? key.ToString() };
        }

        #endregion

        #region Properties

        public string DisplayName
        {
            get
            {
                return this.workflowState.DisplayName;
            }
        }

        public bool InitialState { get; set; }

        public TState Key
        {
            get
            {
                return this.key;
            }
        }

        public List<StateMachineTransition<TState, TTrigger>> Transitions { get; set; }

        public State WorkflowState
        {
            get
            {
                return this.workflowState;
            }
        }

        #endregion

        #region Public Methods

        public StateMachineState<TState, TTrigger> AutoTrigger(
            TState state, string name = null, Action action = null, Func<bool> condition = null)
        {
            // if the destination state does not exist yet, this will add it.
            var destination = this.stateMachine[state];

            this.Transitions.Add(
                new StateMachineTransition<TState, TTrigger>
                {
                    Action = action,
                    AutoTrigger = true,
                    Condition = condition,
                    Source = this,
                    Destination = destination,
                    Name = name
                });

            return this;
        }

        public StateMachineState<TState, TTrigger> Entry<T1>(Action<T1> action, T1 arg1, string name = null)
        {
            this.workflowState.Entry = new WorkflowAction<T1> { Action = action, Arg1 = arg1, DisplayName = name ?? "Entry" };
            return this;
        }

        public StateMachineState<TState, TTrigger> Entry(Action action, string name = null)
        {
            this.workflowState.Entry = new WorkflowAction { Action = action, DisplayName = name ?? "Entry" };
            return this;
        }

        public StateMachineState<TState, TTrigger> Exit(Action action, string name = null)
        {
            this.workflowState.Exit = new WorkflowAction { Action = action, DisplayName = name ?? "Exit" };
            return this;
        }

        public StateMachineState<TState, TTrigger> Exit(Activity activity)
        {
            this.workflowState.Exit = activity;
            return this;
        }

        public StateMachineState<TState, TTrigger> IsFinal(bool isFinal)
        {
            this.WorkflowState.IsFinal = isFinal;
            return this;
        }

        public StateMachineState<TState, TTrigger> IsStart(bool isStart)
        {
            this.InitialState = true;
            return this;
        }

        public StateMachineState<TState, TTrigger> When(
            TTrigger trigger,
            TState gotoState,
            Func<bool> condition = null,
            Action action = null,
            string name = null
            )
        {
            // Will add the destination if it does not exist yet
            var destination = this.stateMachine[gotoState];

            this.Transitions.Add(
                new StateMachineTransition<TState, TTrigger> { Action = action, Condition = condition, Source = this, Destination = destination, Trigger = trigger, Name = name });

            return this;
        }

        #endregion
    }

    public class WorkflowCanceledException : Exception
    {
        #region Constructors and Destructors

        public WorkflowCanceledException()
        {
        }

        public WorkflowCanceledException(string message)
            : base(message)
        {
        }

        public WorkflowCanceledException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        public WorkflowCanceledException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}