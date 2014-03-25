using JPWFUI.Models;
using JPWFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JPWFUI.Controllers
{
    public class ConditionController : Controller
    {
        //
        // GET: /Condition/
        ConditionVM VM;
        public ActionResult Index()
        {
            VM = new ConditionVM();
            return View(VM.GetAllConditions());
        }

        public ActionResult CreateCondition()
        {
            VM = new ConditionVM();
            ConditionModel model = new ConditionModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateCondition(ConditionModel cn)
        {
            VM = new ConditionVM();
            cn.Id = 0;
            VM.SaveCondition(cn);
            return RedirectToAction("Index");
        }


        public ActionResult RemoveXMLCondition(int CnId, int XmlCnId)
        {
            VM = new ConditionVM();
            ConditionModel cn = VM.GetCondition(CnId);
            XMLConditionModel xm = cn.Conditions.Where(c=> c.Id == XmlCnId).FirstOrDefault();
            cn.Conditions.Remove(xm);
            VM.SaveCondition(cn);
            return RedirectToAction("Index");
        }

        public ActionResult AddXMLCondition(int CnId)
        {
            XMLConditionModel cn = new XMLConditionModel();
            cn.CondId = CnId;
            return View(cn);
        }

        [HttpPost]
        public ActionResult AddXMLCondition(XMLConditionModel xmlcn)
        {
            if (xmlcn.CondnType == 0)
                xmlcn.CondName = "returnvalue";
            if (xmlcn.CondnType == 1)
                xmlcn.CondName = "validatequery";
            VM = new ConditionVM();
            ConditionModel cn = VM.GetCondition(xmlcn.CondId);
            int mx;
            if (cn.Conditions.Count == 0)
                mx = 1;
            else
                mx = cn.Conditions.Max(c => c.Id);
            xmlcn.Id = mx + 1;
            cn.Conditions.Add(xmlcn);
            VM.SaveCondition(cn);
            return RedirectToAction("Index");
        }

    }
}
