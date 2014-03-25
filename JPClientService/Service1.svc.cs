using JPClientService.Activities;
using JPClientService.DBS;
using JPClientService.Helpers;
using JPClientService.Tracking;
using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.DurableInstancing;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;


namespace JPClientService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class WFDef
    {
        public int Id { get; set; }
        public string StateName { get; set; }
        public List<WFMovement> Movements { get; set; }
    }

    public class WFMovements
    {
        public int ToStatusId { get; set; }
        public string Variable { get; set; }
        public string VariableValue { get; set; }
        public string ToStatus { get; set; }
        
    }


    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service1 : IService1
    {
        //public static InstanceStore instanceStore;
        public string GetData(string value)
        {
            return string.Format("You entered: {0}", value);
        }

        public List<WFDef> BuildWFDef()
        {
            List<WFDef> defs = new List<WFDef>();
            ////Instance=1,Load=2,Plan=3,InProg=4,Comp=6,Cancel=5

            //List<WFMovements> movsL = new List<WFMovements>();
            //movsL.Add(new WFMovements { ToStatusId = 3, ToStatus = "Planning", Variable = "IsCancel", VariableValue = "0" });
            //movsL.Add(new WFMovements { ToStatusId = 5, ToStatus = "Cancel", Variable = "IsCancel", VariableValue = "1" });
            //defs.Add(new WFDef { Id = 2, StateName = "Load", Movements = movsL });

            //List<WFMovements> movsP = new List<WFMovements>();
            //movsP.Add(new WFMovements { ToStatusId = 4, ToStatus = "InProgress", Variable = "IsCancel", VariableValue = "0" });
            //movsP.Add(new WFMovements { ToStatusId = 2, ToStatus = "Load", Variable = "IsCancel", VariableValue = "0" });
            //movsP.Add(new WFMovements { ToStatusId = 5, ToStatus = "Cancel", Variable = "IsCancel", VariableValue = "1" });
            //defs.Add(new WFDef { Id = 3, StateName = "Planning", Movements = movsP });

            //List<WFMovements> movsPr = new List<WFMovements>();
            //movsPr.Add(new WFMovements { ToStatusId = 6, ToStatus = "Completed", Variable = "IsCancel", VariableValue = "0" });
            //movsPr.Add(new WFMovements { ToStatusId = 3, ToStatus = "Planning", Variable = "IsCancel", VariableValue = "0" });
            //movsPr.Add(new WFMovements { ToStatusId = 5, ToStatus = "Cancel", Variable = "IsCancel", VariableValue = "1" });
            //defs.Add(new WFDef { Id = 4, StateName = "InProgress", Movements = movsPr });

            //List<WFMovements> movsCn = new List<WFMovements>();
            //movsCn.Add(new WFMovements { ToStatusId = 2, ToStatus = "Load", Variable = "IsCancel", VariableValue = "0" });
            //movsCn.Add(new WFMovements { ToStatusId = 3, ToStatus = "Planning", Variable = "IsCancel", VariableValue = "0" });
            //defs.Add(new WFDef { Id = 5, StateName = "Cancel", Movements = movsCn });

            //List<WFMovements> movsCom = new List<WFMovements>();
            //defs.Add(new WFDef { Id = 6, StateName = "Completed", Movements = movsCom });
            string myXML = "";
            using (ClientEntities cl = new ClientEntities())
            {
                foreach (string xm in cl.WFDefs("Select Id,Status from tblstatus order by 1 ", "Select"))
                {
                    myXML += xm;
                }
            }
            XmlDocument xmlDoc = Helpers.HelperFunctions.PrepareXMLFromString(myXML);
            string xpath = "root/tblstatus";
            var nodes = xmlDoc.SelectNodes(xpath);
            using (TestAppsEntities ent = new TestAppsEntities())
            {
                foreach (XmlNode st in nodes)
                {
                    WFDef df = new WFDef { Id = int.Parse(st.Attributes[0].Value), StateName = st.Attributes[1].Value };
                    var mos = ent.WFMovements.Where(c => c.ToStatusId == df.Id);
                    foreach(WFMovement mo in mos)
                    {
                        mo.ToStatus = df.StateName;
                    }
                    ent.SaveChanges();
                    df.Movements = ent.WFMovements.Where(c => c.StatusId == df.Id).OrderBy(c => c.Id).ToList();
                    defs.Add(df);
                }
            }
            return defs;
        }

        public string CreateInstance1()
        {
            
            return "";
        }
        public string ExitSub(int st)
        {
            return "";
        }



        public StateMachine<int, string> CreateInstance(Dictionary<string,string> VarValue)
        {
            
            var target = new StateMachine<int, string>();
            TimeSpan ts =  TimeSpan.FromSeconds(10);
            AutoResetEvent instanceUnloaded = new AutoResetEvent(false);
            target.DisplayName = "JPWorkFlow";
            var wfs=BuildWFDef();
            int LastWFid = wfs.Last().Id;
            target.AddState(0).When("Init", 1, () =>
            {
                WorkflowInvoker.Invoke(new Sequence
                {
                    Activities =
                                {
                                    new LogTrackActivity { state =new InArgument<string>(wfs.FirstOrDefault().StateName), 
                                        stateid = wfs.FirstOrDefault().Id,
                                        instanceID= target.WorkflowApplication.Id },
                                    //new Persist { DisplayName ="per"},
                                },
                });
                return true;
            }).InitialState = true;
            foreach (WFDef f in wfs)
            {
                target.AddState(f.Id);
                foreach (WFMovement m in f.Movements)
                {
                    
                    target[f.Id].When(m.ToStatus.Trim(), m.ToStatusId, () =>
                        {
                            
                            //foreach (var cn in VarValue)
                            //{
                            //    if (m.Variable.Trim() == cn.Key && m.VariableValue.Trim() == cn.Value)
                            //    {

                            var wfApplication = new WorkflowApplication(new Sequence
                            {
                                Activities = 
                                            {
                                                new LogTrackActivity { state =new InArgument<string>(m.ToStatus), 
                                                    stateid = m.ToStatusId,
                                                    instanceID= target.WorkflowApplication.Id },
                                                //Execute list of Actions
                                            }

                            });

                            wfApplication.Completed = e =>
                            {
                                if (e.CompletionState == ActivityInstanceState.Faulted)
                                {

                                    //completed but faulted
                                    //handle the error
                                }
                                else
                                {
                                    if (e.CompletionState == ActivityInstanceState.Canceled)
                                    {
                                        //completed but cancelled
                                    }
                                    else
                                    {
                                        //completed
                                        // Add Actions here.
                                    }
                                }
                            };
                            wfApplication.Run();

                            //        return true;
                            //    }
                            //}
                            return true;
                        }
                    );
                }
                
            }
           
           return target;
        }
              
        

        public string Load()
        {
            //Dictionary<string, string> vars = new Dictionary<string, string>();
            //vars.Add("MHandler", "0");
            StateMachine<int, string> sm = CreateInstance(null);
            sm.Run();
            System.Threading.Thread.Sleep(2000);
            sm.Move(sm.WorkflowApplication.Id, "Init");
            return sm.WorkflowApplication.Id.ToString();
        }

        public FlowResponse MoveTo(string MHandler)
        {
            string cState = "";
            FlowResponse res = new FlowResponse();
            using (TestAppsEntities ent = new TestAppsEntities())
            {
                string lState = "";
                //string handVal=ArrHand[1].Replace(")","").Trim();
                Guid gui = Guid.Parse(MHandler);
                cState = ent.WFStateTracks.Where(c => c.InstanceId == gui).OrderByDescending(c => c.DateOccured).Take(1).SingleOrDefault().StateName;
                WFStateTrack lst = ent.WFStateTracks.Where(c => c.InstanceId == gui).OrderByDescending(c => c.DateOccured).Take(1).SingleOrDefault();
                var nxtstps = (from wf in ent.WFMovements
                               where wf.StatusId == lst.StateId
                               select wf).ToList();
                foreach(WFMovement mo in nxtstps)
                {
                    ServiceConditions cn = new ServiceConditions();
                    bool chk = cn.CkeckAllConditions(gui, lst.StateId, mo.ToStatusId);
                    if (chk == true)
                    {
                        string nxtstp = mo.ToStatus;
                        StateMachine<int, string> sm = CreateInstance(null);
                        sm.Move(gui, nxtstp.Trim());
                        cState = ent.WFStateTracks.Where(c => c.InstanceId == gui).OrderByDescending(c => c.DateOccured).Take(1).SingleOrDefault().StateName;
                        res.currentstatus = cState;
                        res.conditionssuccess = true;
                        res.Err = "";
                        return res;
                    }
                }
            }
            res.currentstatus = cState;
            res.conditionssuccess = false;
            res.Err = "Error";
            return res;
        }
     }
}
