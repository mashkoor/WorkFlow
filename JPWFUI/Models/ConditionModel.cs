using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace JPWFUI.Models
{
    public class ConditionModel
    {
        public List<XMLConditionModel> Conditions { get; set; }
        public int Id { get; set; }
        public bool Selected { get; set; }
        public string ConditionGroup { get; set; }
        public ConditionModel()
        {
            Conditions = new List<XMLConditionModel>();
        }
        public List<SelectListItem> AvailableConditionGroups
        {
            get
            {
                List<SelectListItem> ret = new List<SelectListItem>();
                ret.Add(new SelectListItem { Text = "And", Value = "and" });
                ret.Add(new SelectListItem { Text = "OR", Value = "or" });
                return ret;
            }
        }
    }

    public class XMLConditionModel
    {
        public int CondnType { get; set; }
        public string CondName { get; set; }
        public string CondValue { get; set; }
        public int Id { get; set; }
        public int CondId { get; set; }

        public List<SelectListItem> AvailableCondTypes
        {
            get
            {
                List<SelectListItem> ret = new List<SelectListItem>();
                ret.Add(new SelectListItem { Text = "Query", Value = "1" });
                ret.Add(new SelectListItem { Text = "Return Value", Value = "0" });
                ret.Add(new SelectListItem { Text = "Parameter", Value = "2" });
                return ret;
            }
        }
    }

    public class AddConditionModel
    {
        public List<ConditionModel> MoveConditions { get; set; }
        public int LnkId { get; set; }
    }
}