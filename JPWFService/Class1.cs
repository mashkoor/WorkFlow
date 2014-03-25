using System;
using System.Activities;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Web;

namespace JPWFService
{
    public sealed class CodeStateMachine : NativeActivity
    {
        #region Properties

        public System.Activities.Statements.StateMachine StateMachine { get; set; }

        public Dictionary<string, InArgument> Arguments { get; set; }

        public Collection<Variable> Variables { get; set; }

        #endregion

        #region Methods

        public CodeStateMachine()
        {
            this.Variables = new Collection<Variable>();
            this.DisplayName = "CodeStateMachine";
            this.Arguments = new Dictionary<string, InArgument>();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            foreach (var kvp in this.Arguments)
            {
                metadata.AddArgument(new RuntimeArgument(kvp.Key, kvp.Value.ArgumentType, kvp.Value.Direction));
            }

            metadata.SetVariablesCollection(Variables);
            metadata.AddChild(StateMachine);
        }

        protected override void Execute(NativeActivityContext context)
        {
            context.ScheduleActivity(this.StateMachine);
        }

        #endregion
    }

    public class StateChangedEventArgs<TState> : EventArgs
    {
        public TState State { get; set; }
    }
    public class StateMachine<TState, TTrigger> : TrackingParticipant
    {
        public EventHandler<StateChangedEventArgs<TState>> StateChanged;

        private readonly CodeStateMachine codeStateMachine = new CodeStateMachine();
        private readonly ManualResetEvent completedEvent = new ManualResetEvent(false);
        private readonly Dictionary<string, TState> stateKeyMap = new Dictionary<string, TState>();
        private readonly System.Activities.Statements.StateMachine stateMachine = new System.Activities.Statements.StateMachine();

    }
}