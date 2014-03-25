using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JPWFUI.Models
{
    public class WFFlowCollectionModel
    {
        public List<WFFlowModel> WFDefs { get; set; }
        public int WFID { get; set; }

        public WFFlowCollectionModel()
        {
            
        }
    }

    public class WFFlowModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please select From step ")]
        public int FrmStepID { get; set; }
        public int WFID { get; set; }

        [Required(ErrorMessage = "Please select To step ")]
        public int ToStepID { get; set; }

        public string FrmStepName { get; set; }
        public string ToStepName { get; set; }
        public WFFlowModel()
        {
            Conditions = new List<ConditionModel>();
        }

        public List<ConditionModel> Conditions { get; set; }
        
        public List<SelectListItem> FrmSteps
        {
            get
            {
                using (TestAppsEntities1 _db = new TestAppsEntities1())
                {
                    return _db.tblStatus.ToList().Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Status }).ToList();
                }
            }
        }
        public List<SelectListItem> ToSteps
        {
            get
            {
                using (TestAppsEntities1 _db = new TestAppsEntities1())
                {
                    return _db.tblStatus.ToList().Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Status }).ToList();
                }
            }
        }

        public List<SelectListItem> AvailableConditions
        {
            get
            {
                using (TestAppsEntities1 _db = new TestAppsEntities1())
                {
                    return _db.WFConditions.ToList().Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Condns }).ToList();
                }
            }
        }
    }
}