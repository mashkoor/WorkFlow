using Microsoft.Activities;
using System;
using System.Activities;
using System.Activities.DurableInstancing;
using System.Activities.Statements.Tracking;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.ServiceModel.Activities.Description;
using System.Threading;
using System.Web;
using System.Xaml;

namespace JPClientService
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
        

        IDictionary<Guid, WorkflowApplication> instances;
        static AutoResetEvent instanceUnloaded = new AutoResetEvent(false);

        public EventHandler<StateChangedEventArgs<TState>> StateChanged;

        private readonly CodeStateMachine codeStateMachine = new CodeStateMachine();
        //private readonly ManualResetEvent completedEvent = new ManualResetEvent(false);
        private readonly Dictionary<string, TState> stateKeyMap = new Dictionary<string, TState>();
        private readonly System.Activities.Statements.StateMachine stateMachine = new System.Activities.Statements.StateMachine();
        private readonly Dictionary<TState, StateMachineState<TState, TTrigger>> states =
           new Dictionary<TState, StateMachineState<TState, TTrigger>>();

        //private readonly object syncLock = new object();

        private readonly Dictionary<TTrigger, ManualResetEvent> triggerEvents =
            new Dictionary<TTrigger, ManualResetEvent>();

        private readonly Dictionary<string, StateMachineTransition<TState, TTrigger>> triggerMap = new Dictionary<string, StateMachineTransition<TState, TTrigger>>();

        private WorkflowApplication workflowApplication;

        private bool initialized;

        //private TimeSpan timeout = TimeSpan.FromSeconds(30);
        
        #region Constructors and Destructors

        public StateMachine()
        {

            this.Arguments = new WorkflowArguments();
            this.Variables = new Dictionary<string, Type>();

            this.workflowApplication = new WorkflowApplication(this.codeStateMachine, this.Arguments);
            this.workflowApplication.Extensions.Add(this);
            
            this.workflowApplication.PersistableIdle = OnIdleAndPersistable;
            this.workflowApplication.Completed = OnWorkflowCompleted;
            this.workflowApplication.Idle = OnIdle;
            this.workflowApplication.Unloaded = (e) =>
            {
                instanceUnloaded.Set();
            };
            InstanceStore instanceStore;
            //instanceStore =
            //    new SqlWorkflowInstanceStore(@"Data Source=Webserver08;Initial Catalog=TestApps;User ID=acitfluid;Password=fluid@@cit;Asynchronous Processing=True");
            instanceStore = new SqlWorkflowInstanceStore(ConfigurationManager.ConnectionStrings["FlowConnection"].ConnectionString);
            InstanceHandle handle = instanceStore.CreateInstanceHandle();
            //InstanceView view = instanceStore.Execute(handle, new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(07));
            //handle.Free();
            //instanceStore.DefaultInstanceOwner = view.InstanceOwner;
            this.WorkflowApplication.InstanceStore = instanceStore;



            SqlWorkflowInstanceStoreBehavior instanceStoreBehavior = new SqlWorkflowInstanceStoreBehavior();
            instanceStoreBehavior.HostLockRenewalPeriod = new TimeSpan(0, 0, 5);
            instanceStoreBehavior.InstanceCompletionAction = InstanceCompletionAction.DeleteNothing;
            instanceStoreBehavior.InstanceLockedExceptionAction = InstanceLockedExceptionAction.AggressiveRetry;
            instanceStoreBehavior.InstanceEncodingOption = InstanceEncodingOption.GZip;
            this.workflowApplication.Extensions.Add(instanceStoreBehavior);
            //instanceStoreBehavior.RunnableInstancesDetectionPeriod = TimeSpan.FromSeconds(2);
            //this.WorkflowApplication.InstanceStore. .Description.Behaviors.Add(instanceStoreBehavior);
        }
        public void OnIdle(WorkflowApplicationIdleEventArgs e)
        {
            //PersistableIdleAction pa = PersistableIdleAction.Unload;
            this.ResetTriggerEvents();
        }
        private void ResetTriggerEvents()
        {
            foreach (var triggerEvent in this.triggerEvents.Values)
            {
                triggerEvent.Reset();
            }
        }
        public PersistableIdleAction OnIdleAndPersistable(WorkflowApplicationIdleEventArgs e)
        {
            return PersistableIdleAction.Unload;
        }

        // executed when instance is persisted
        public void OnWorkflowCompleted(WorkflowApplicationCompletedEventArgs e)
        {
        }

        public void Run()
        {
            Initialize();
            
            this.WorkflowApplication.Run();
            //System.Threading.Thread.Sleep(7000);
            //this.WorkflowApplication.Persist();
            //System.Threading.Thread.Sleep(8000);
            //this.WorkflowApplication.Unload();
   
            //instanceUnloaded.WaitOne();
        }

        public WorkflowApplication Move(Guid instanceId,string bkmark)
        {
            // if the instance is in memory, return it
            //if (this.instances.ContainsKey(instanceId))
            //    return this.instances[instanceId];
            this.Arguments = new WorkflowArguments();
            this.Variables = new Dictionary<string, Type>();
            WorkflowApplication instance = new WorkflowApplication(this.codeStateMachine);
            Initialize();
            this.workflowApplication = instance;
            this.workflowApplication.PersistableIdle = OnIdleAndPersistable;
            this.workflowApplication.Completed = OnWorkflowCompleted;
            this.workflowApplication.Idle = OnIdle;
            this.workflowApplication.Unloaded = (e) =>
            {
                instanceUnloaded.Set();
            };
            
            InstanceStore instanceStore;
            instanceStore =
                new SqlWorkflowInstanceStore(ConfigurationManager.ConnectionStrings["FlowConnection"].ConnectionString);

            //InstanceHandle handle = instanceStore.CreateInstanceHandle();
            //InstanceView view = instanceStore.Execute(handle, new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(07));
            //handle.Free();
            //instanceStore.DefaultInstanceOwner = view.InstanceOwner;
            this.WorkflowApplication.InstanceStore = instanceStore;
            
            // add the instance to the list of running instances in the host
            instance.Load(instanceId);
            this.WorkflowApplication.ResumeBookmark(bkmark, null);
            //this.WorkflowApplication.Persist();
            System.Threading.Thread.Sleep(4000);
            //this.WorkflowApplication.Unload();
            return instance;
        }

        
        #endregion

        #region Properties

        public dynamic Arguments { get; set; }

        public string DisplayName
        {
            get { return this.WorkflowStateMachine.DisplayName; }
            set
            {
                // TODO - Throw if workflow has already started
                this.WorkflowStateMachine.DisplayName = value;
            }
        }

        public dynamic Output { get; set; }

        public TState State { get; private set; }

        public Converter<string, TState> StateFromString { get; set; }

        public IDictionary<TState, StateMachineState<TState, TTrigger>> States
        {
            get { return this.states; }
        }

       
        public TrackingParticipant Tracking { get; set; }

        public Converter<string, TTrigger> TriggerFromString { get; set; }

        public IDictionary<string, Type> Variables { get; private set; }

        public WorkflowApplication WorkflowApplication
        {
            get { return this.workflowApplication; }
        }

        public System.Activities.Statements.StateMachine WorkflowStateMachine
        {
            get { return this.stateMachine; }
        }

        public string XAML
        {
            get { return XamlServices.Save(this.codeStateMachine); }
        }

        #endregion

        #region Indexers

        public StateMachineState<TState, TTrigger> this[TState index]
        {
            get
            {
                StateMachineState<TState, TTrigger> state;

                // Add the state if it does not exist yet, otherwise return it
                return !this.States.TryGetValue(index, out state)
                           ? this.AddState(index)
                           : state;
            }
        }

        #endregion
        #region Public Methods

        public StateMachineState<TState, TTrigger> AddState(
            TState stateKey,
            string name = null)
        {
            StateMachineState<TState, TTrigger> state;

            // Note: It is not an error to try and add a state that already exists
            // Because we are trying not to force creation order, the state may
            // be implicitly created before the caller thinks it is.
            if (!this.States.TryGetValue(stateKey, out state))
            {
                state = new StateMachineState<TState, TTrigger>(stateKey, this, name);

                this.stateKeyMap.Add(state.DisplayName, stateKey);
                this.states.Add(stateKey, state);
            }

            return state;
        }

        public void AddTracking(TrackingParticipant trackingParticipant)
        {
        
            this.workflowApplication.Extensions.Add(trackingParticipant);
        }

        public InArgument<T> CreateInArgument<T>(
            string name,
            T defaultValue)
        {
            var arg = new InArgument<T>(defaultValue);
            this.codeStateMachine.Arguments.Add(name, arg);
            return arg;
        }

        public Variable CreateVariable(
            string name,
            Type type)
        {
            var variable = Variable.Create(name, type, VariableModifiers.None);
            this.codeStateMachine.Variables.Add(variable);
            return variable;
        }

       

        #endregion

        #region Methods

        /// <summary>
        ///   When implemented in a derived class, used to synchronously process the tracking record.
        /// </summary>
        /// <param name = "record">The generated tracking record.</param>
        /// <param name = "trackingTimeout">The time period after which the provider aborts the attempt.</param>
        protected override void Track(
            TrackingRecord record,
            TimeSpan trackingTimeout)
        {
            var stateMachineRecord = record as StateMachineStateRecord;

            if (stateMachineRecord == null)
            {
                return;
            }

            //Debug.WriteLine("Setting state " + stateMachineRecord.StateName);

            if (!this.TrySetState(stateMachineRecord.StateName))
            {
                throw new InvalidOperationException("Unknown state " + stateMachineRecord.StateName);
            }
        }

        private void AddTrigger(StateMachineTransition<TState, TTrigger> transition)
        {
            this.triggerMap.Add(transition.Trigger.ToString(), transition);
            this.triggerEvents.Add(transition.Trigger, new ManualResetEvent(false));
        }

        public void Initialize()
        {
            //if (this.IsRunning)
            //{
            //    throw new InvalidOperationException("Cannot initialize while StateMachine is running");
            //}

                // Return if the state machine has already been created
                if (this.initialized)
                {
                    return;
                }

                // Pass 1 resolve the states
                // Add all the states to the state machine
                foreach (var state in this.States.Values)
                {
                    this.WorkflowStateMachine.States.Add(state.WorkflowState);

                    // If the state was marked as initial state explicitly
                    if (state.InitialState)
                    {
                        this.WorkflowStateMachine.InitialState = state.WorkflowState;
                    }
                }

                // If no other state was marked as the initial state
                if (this.WorkflowStateMachine.InitialState == null)
                {
                    // The first state becomes the initial state
                    this.WorkflowStateMachine.InitialState = this.States.Values.First().WorkflowState;
                }

                // Pass 2 - resolve transitions
                foreach (var state in this.States.Values)
                {
                    foreach (var transition in state.Transitions)
                    {
                        // Create a transition
                        state.WorkflowState.Transitions.Add(transition.CreateTransition());

                        // Map triggers to strings so we can reverse the process later
                        StateMachineTransition<TState, TTrigger> trigger;
                        if (!this.triggerMap.TryGetValue(transition.Trigger.ToString(), out trigger))
                        {
                            // The trigger map enables conversion from string to TTrigger
                            // This is required when resuming bookmarks where the name of the bookmark
                            this.AddTrigger(transition);
                        }
                    }

                    // States with no transitions are marked as final
                    state.IsFinal(state.WorkflowState.Transitions.Count == 0);
                }

                this.codeStateMachine.StateMachine = this.stateMachine;
                this.initialized = true;
       }
        

        

        
        

        private bool TrySetState(string stateName)
        {
            TState state;

            if (!this.stateKeyMap.TryGetValue(stateName, out state))
            {
                return false;
            }

            // Set the current state
            this.State = state;

            if (this.StateChanged != null)
            {
                this.StateChanged(this, new StateChangedEventArgs<TState> { State = this.State });
            }
            return true;
        }

        private bool TrySetTrigger(string triggerName)
        {
            StateMachineTransition<TState, TTrigger> transition;

            return this.triggerMap.TryGetValue(triggerName, out transition) && this.triggerEvents[transition.Trigger].Set();
        }

        #endregion
    }
}