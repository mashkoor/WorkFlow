using JPClientService.DBS;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JPClientService.Activities
{
    public class LogTrackActivity : NativeActivity
    {
        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }
        public InArgument<Guid> instanceID { get; set; }
        public InArgument<string> state { get; set; }
        public InArgument<int> stateid { get; set; }
        public InArgument<StateMachine<int, string>> sm { get; set; }
        protected override void Execute(NativeActivityContext context)
        {
            //Guid instanceID = context.WorkflowInstanceId;
            using (TestAppsEntities ent = new TestAppsEntities())
            {
                //int stid = sm.Get<StateMachine<int, string>>(context).State;
                //string stnm=ent.tblStatus.Where(c => c.Id == stid).FirstOrDefault().Status;
                WFStateTrack tr = new WFStateTrack { DateOccured = DateTime.Now, InstanceId = instanceID.Get<Guid>(context),
                                                     StateId = stateid.Get<int>(context),StateName = state.Get<string>(context) };
                ent.WFStateTracks.Add(tr);
                ent.SaveChanges();
            }
            //context.CreateBookmark();
        }
    }

    public class ActionEmailActivity : NativeActivity
    {
        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }
        public InArgument<Guid> instanceID { get; set; }
        protected override void Execute(NativeActivityContext context)
        {
            using (TestAppsEntities ent = new TestAppsEntities())
            {
                //using (ClientEntities cl = new ClientEntities())
                //{
                //    cl.uspGetDynamicDataFrmDefs()
                //}
                Guid insid = instanceID.Get<Guid>(context);
                //var Eactns = from wf in ent.WFMovements
                //             join ac in ent.WFActions on wf.Id equals ac.StepId
                //             join eac in ent.WFActionEmails on ac.Id equals eac.ActionId
                //             where wf.WFId == 1
                //             select eac;
                
            }
        }
    }
}