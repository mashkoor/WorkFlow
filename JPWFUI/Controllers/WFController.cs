using JPWFUI.Models;
using JPWFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace JPWFUI.Controllers
{
    public class WFController : Controller
    {
        public ActionResult Index()
        {
            WFVM VM = new WFVM();
            WFFlowCollectionModel model = new WFFlowCollectionModel();
            model.WFID = 1;
            model.WFDefs = VM.GetAllWFDefs(1);
            return View(model);
        }

        public ActionResult CreateStep(int WfId)
        {
            WFFlowModel model = new WFFlowModel();
            model.WFID = WfId;
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateStep(WFFlowModel model)
        {
            WFVM VM = new WFVM();
            VM.CreateLink(model);
            return RedirectToAction("Index");
        }

        public ActionResult RemoveStep(int Lnk)
        {
            WFVM VM = new WFVM();
            VM.RemoveLink(Lnk);
            return RedirectToAction("Index");
        }
        
        public ActionResult AddCondition(int LnkId)
        {
            ConditionVM cvm = new ConditionVM();
            AddConditionModel am = new AddConditionModel();
            WFVM VM = new WFVM();
            var exist = VM.GetExistingConditionByLinkId(LnkId);
            var allC = cvm.GetAllConditions();
            foreach (var c in allC)
            {
                bool isFound = exist.Where(c1 => c1.CondnId == c.Id).Count() > 0;
                if (isFound)
                    c.Selected = true;
            }
            am.MoveConditions = allC;
            am.LnkId = LnkId;
            return View(am);
        }

        
        
        public ActionResult RemoveCondition(int LnkId,int CnId)
        {
            WFVM VM = new WFVM();
            VM.RemoveCondition(LnkId, CnId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddCondition(AddConditionModel am)
        {
            bool bb;
            WFVM VM = new WFVM();
            bb= VM.RemoveAllConditionByLinkId(am.LnkId);
            foreach (var cn in am.MoveConditions)
            {
                if (cn.Selected == true)
                    VM.AddCondition(am.LnkId, cn.Id,cn.ConditionGroup);
                    //bb = true;
                    
            }
            return RedirectToAction("Index");
        }
    }
}
