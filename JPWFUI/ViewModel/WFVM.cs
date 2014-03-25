using JPWFUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JPWFUI.ViewModel
{
    public class WFVM
    {
        public List<WFFlowModel> GetAllWFDefs(int Wfid)
        {
            ConditionVM cvm = new ConditionVM();
            List<WFFlowModel> WFDefs = new List<WFFlowModel>();
            using (TestAppsEntities1 _db = new TestAppsEntities1())
            {
                var links = _db.WFMovements.Where(c => c.WFId == Wfid).ToList();
                foreach (var ln in links)
                {
                    WFFlowModel flo = new WFFlowModel { FrmStepID = ln.StatusId, ToStepID = ln.ToStatusId };
                    flo.FrmStepName = _db.tblStatus.Where(c => c.Id == ln.StatusId).FirstOrDefault().Status;
                    flo.ToStepName = _db.tblStatus.Where(c => c.Id == ln.ToStatusId).FirstOrDefault().Status;
                    flo.Id = ln.Id;
                    foreach (var cn in ln.WFConditionMovements)
                    {
                        ConditionModel cm = cvm.GetCondition(cn.CondnId);
                        cm.ConditionGroup = cn.ConditionGroup;
                        flo.Conditions.Add(cm);
                    }
                    WFDefs.Add(flo);
                }
                return WFDefs;
            }
        }
        public bool RemoveCondition(int LnkId, int CmId)
        {
            WFConditionMovement mo;
            using (TestAppsEntities1 _db = new TestAppsEntities1())
            {
                mo = _db.WFConditionMovements.Where(c => c.StepId == LnkId && c.CondnId == CmId).FirstOrDefault();
                _db.WFConditionMovements.Remove(mo);
                _db.SaveChanges();
                return true;
            }
            
        }
        public bool AddCondition(int LnkId, int CmId, string conditionGroup)
        {
            WFConditionMovement mo = new WFConditionMovement();
            using (TestAppsEntities1 _db = new TestAppsEntities1())
            {
                mo.ConditionGroup = conditionGroup;
                mo.StepId = LnkId;
                mo.CondnId = CmId;
                _db.WFConditionMovements.Add(mo);
                _db.SaveChanges();
                return true;
            }

        }

        public List<WFConditionMovement> GetExistingConditionByLinkId(int Lnk)
        {
            List<WFConditionMovement> WFDefs = new List<WFConditionMovement>();
            using (TestAppsEntities1 _db = new TestAppsEntities1())
            {
                var conds = _db.WFConditionMovements.Where(c => c.StepId == Lnk);
                foreach (var cn in conds)
                {
                    WFDefs.Add(cn);
                }
                return WFDefs;
            }
        }

        public bool RemoveAllConditionByLinkId(int Lnk)
        {
            using (TestAppsEntities1 _db = new TestAppsEntities1())
            {
                var conds = _db.WFConditionMovements.Where(c => c.StepId == Lnk);
                foreach (var cn in conds)
                {
                    _db.WFConditionMovements.Remove(cn);
                }
                _db.SaveChanges();
                return true;
            }
        }
        
        public bool CreateLink(WFFlowModel model)
        {
            using (TestAppsEntities1 _db = new TestAppsEntities1())
            {
                WFMovement mo = new WFMovement();
                mo.StatusId = model.FrmStepID;
                mo.ToStatusId = model.ToStepID;
                mo.WFId = model.WFID;
                mo.ToStatus = _db.tblStatus.Where(c => c.Id == mo.ToStatusId).FirstOrDefault().Status;
                _db.WFMovements.Add(mo);
                _db.SaveChanges();
                return true;
            }
        }

        public bool RemoveLink(int Lnk)
        {
            using (TestAppsEntities1 _db = new TestAppsEntities1())
            {
                if (RemoveAllConditionByLinkId(Lnk) == true)
                {
                    WFMovement mo = _db.WFMovements.Where(c => c.Id == Lnk).FirstOrDefault();
                    _db.WFMovements.Remove(mo);
                    _db.SaveChanges();
                }
                return true;
            }
        }
    }
}