using JPWFUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace JPWFUI.ViewModel
{
    public class ConditionVM
    {
        public ConditionModel SaveCondition(ConditionModel Condn)
        {
            using (TestAppsEntities1 _db = new TestAppsEntities1())
            {
                WFCondition Con;
                if (Condn.Id == 0)
                {
                    Con = new WFCondition();
                    Con.CondType = 1;
                    _db.WFConditions.Add(Con);
                    _db.SaveChanges();
                    Condn.Id = Con.Id;
                }
                Con = _db.WFConditions.Where(c => c.Id == Condn.Id).FirstOrDefault();
                
                string xmlcndns = "";
                foreach (var cn in Condn.Conditions)
                {
                    xmlcndns += "<param name=\"" + cn.CondName + "\" type=\"" + cn.CondnType + "\" id =\"" + cn.Id.ToString() + "\" value=\"" + cn.CondValue + "\" />"; 
                }
                Con.Condns = xmlcndns;
                if (Condn.Id == 0)
                {
                    _db.WFConditions.Add(Con);
                }
                _db.SaveChanges();
            }
            return GetCondition(Condn.Id);
        }

        public List<ConditionModel> GetAllConditions()
        {
            List<ConditionModel> ret = new List<ConditionModel>();
            using (TestAppsEntities1 _db = new TestAppsEntities1())
            {
                foreach (var cn in _db.WFConditions.ToList())
                {
                    ret.Add(GetCondition(cn.Id));
                }
            }
            return ret;
        }
        public ConditionModel GetCondition(int cid)
        {
            ConditionModel mo = new ConditionModel();
            XNamespace ns = "http://schemas.microsoft.com/sharepoint/soap/";
            string docStr = "<Lists xmlns=\"http://schemas.microsoft.com/sharepoint/soap/\">";
            mo.Id = cid;
            using (TestAppsEntities1 _db = new TestAppsEntities1())
            {
                foreach (var cn in _db.WFConditions.Where(c => c.Id == cid).ToList())
                {
                    string xmlstr = docStr + cn.Condns.Replace("\r\n", "") + "</Lists>";
                    XElement node = XElement.Parse(xmlstr);
                    var lists = (from list in node.Descendants(ns + "param")
                                 select list).ToList();
                    foreach (var lst in lists)
                    {
                        XMLConditionModel xcn = new XMLConditionModel
                        {
                            CondName = (string)lst.Attribute("name"),
                            CondnType = (int)lst.Attribute("type"),
                            CondValue = (string)lst.Attribute("value"),
                            Id = int.Parse((string)lst.Attribute("id")),
                            CondId = cn.Id,

                        };
                        xcn.AvailableCondTypes[xcn.CondnType].Selected = true;
                        mo.Conditions.Add(xcn);
                    }
                }
            }
            return mo;
        }

    }
}