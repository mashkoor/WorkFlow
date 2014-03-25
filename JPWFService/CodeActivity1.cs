using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

namespace JPWFService
{

    public class GetWorkflowIDActivity : CodeActivity
    {
        //public InArgument<string> OrderName { get; set; }
        public OutArgument<Guid> ID { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            
            Guid instanceID = context.WorkflowInstanceId;
            context.SetValue<Guid>(this.ID, instanceID);
            //string name = OrderName.Get<string>(context);
            //Common.Orders.Add(instanceID, new OrderModel() { OrderID = instanceID, OrderStatus = Common.ORDER_STATUS.NEW_ORDER, OrderName = name });
        }
    }

    public class LogTrackActivity : CodeActivity
    {
        public InArgument<string> state { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            Guid instanceID = context.WorkflowInstanceId;
            TestAppsEntities ent = new TestAppsEntities();
            StateTrack tr = new StateTrack { DateOccured=DateTime.Now, InstanceId = instanceID, StateName = state.Get<string>(context) };
            ent.StateTracks.Add(tr);
            ent.SaveChanges();
        }
    }
    
    public class Activity_Loaded : CodeActivity
    {
        public OutArgument<string> CurrentState { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            
            Guid instanceID = context.WorkflowInstanceId;
            //context.SetValue<string>(CurrentState, "Loaded");

        }
    }
    public class Activity_Planning : CodeActivity
    {
        public OutArgument<string> CurrentState { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            Guid instanceID = context.WorkflowInstanceId;
            context.SetValue<string>(CurrentState, "Planning");
        }
    }
    public class Activity_InProgress : CodeActivity
    {
        public OutArgument<string> CurrentState { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            Guid instanceID = context.WorkflowInstanceId;
            context.SetValue<string>(CurrentState, "InProgress");
        }
    }
    public class Activity_Completed : CodeActivity
    {
        public OutArgument<string> CurrentState { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            Guid instanceID = context.WorkflowInstanceId;
            context.SetValue<string>(CurrentState, "Completed");
        }
    }
    public class Activity_Cancelled : CodeActivity
    {
        public OutArgument<string> CurrentState { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            Guid instanceID = context.WorkflowInstanceId;
            context.SetValue<string>(CurrentState, "Cancelled");
        }
    }

    
}
