using JPClientService.DBS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace JPClientService.Helpers
{
    public static class HelperFunctions
    {
        public static XmlDocument PrepareXMLFromString(string inputString)
        {
            string myXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?><root>";
            myXML += inputString + "</root>";
            byte[] byteArray = Encoding.UTF8.GetBytes(myXML);
            MemoryStream stream = new MemoryStream(byteArray);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(stream);
            return xmlDoc;
        }
    }
    public class ServiceConditions
    {
        public ServiceConditions()
        {
        }

        public bool CkeckAllConditions(Guid transId,int FrmStatus,int ToStatus)
        {
            bool retVal=false;
            Dictionary<int, bool> AndDict = new Dictionary<int, bool>();
            Dictionary<int, bool> ORDict = new Dictionary<int, bool>();
            using (TestAppsEntities ent = new TestAppsEntities())
            {
                WFMovement mo = ent.WFMovements.Where(c => c.StatusId == FrmStatus && c.ToStatusId == ToStatus).FirstOrDefault();
                var Andcondns = (from cm in ent.WFConditionMovements
                              join cn in ent.WFConditions on cm.CondnId equals cn.Id
                              where cm.StepId == mo.Id && cm.ConditionGroup=="and"
                              select cn).ToList();
                var ORcondns = (from cm in ent.WFConditionMovements
                                 join cn in ent.WFConditions on cm.CondnId equals cn.Id
                                 where cm.StepId == mo.Id && cm.ConditionGroup == "or"
                                 select cn).ToList();
                if (Andcondns.Count == 0 && ORcondns.Count == 0)
                    return true;
                foreach (WFCondition cn in Andcondns)
                {
                    AndDict.Add(cn.Id, CkeckSingleConditions(transId,cn));
                }
                foreach (WFCondition cn in ORcondns)
                {
                    ORDict.Add(cn.Id, CkeckSingleConditions(transId, cn));
                }
                foreach (var kv in AndDict)
                {
                        retVal = kv.Value;
                }
                foreach (var kv in ORDict)
                {
                    if (kv.Value == true)
                        return true;
                }
                
                return retVal;
            }

        }

        public bool CkeckSingleConditions(Guid transId, WFCondition cond)
        {
            XNamespace ns = "http://schemas.microsoft.com/sharepoint/soap/";
            string docStr = "<Lists xmlns=\"http://schemas.microsoft.com/sharepoint/soap/\">";
            docStr += cond.Condns + "</Lists>";
            XElement node = XElement.Parse(docStr);
            var lists = from list in node.Descendants(ns + "param")
                        select list;
            var src = (from list in lists
                       where (string)list.Attribute("type") == "1" // validatequery
                       select (string)list.Attribute("value")).FirstOrDefault();
            var val1 = (from list in lists
                        where (string)list.Attribute("type") == "0" // retnvalue
                        select (string)list.Attribute("value")).FirstOrDefault();
            var parms = (from list in lists
                        where (string)list.Attribute("type") == "2" // parameter
                        select (string)list.Attribute("value"));
                        
            src = src.Replace("[TransactionId]", transId.ToString());
            src = src.Replace("[lt]", "<");
            src = src.Replace("[gt]", ">");
            src = src.Replace("[ne]", "<>");
            //if (typ != "0")
            //{
            //    var BracketMatchesL = Regex.Matches(val1, @"\[(.+?)\]").Cast<Match>().Select(m => m.Groups[1].Value).ToList();
            //    do
            //    {
            //        foreach (string mat in BracketMatchesL)
            //        {
            //            var val3 = (from list in lists
            //                        where (string)list.Attribute("name") == mat
            //                        select (string)list.Attribute("value")).FirstOrDefault();
            //            //var BracketMatchesI = Regex.Matches(val3, @"\[(.+?)\]").Cast<Match>().Select(m => m.Groups[1].Value).ToList();
            //            //if (BracketMatchesI.Count == 0)
            //            //{
            //                using (ClientEntities cl = new ClientEntities())
            //                {
            //                    foreach (string xm in cl.WFDefs(val3, "Select"))
            //                    {
            //                        XElement node1 = XElement.Parse(docStr + xm + "</Lists>");
            //                        var lists1 = from list in node1.Descendants()
            //                                     select list;
            //                        var val2 = (from list in lists1
            //                                    select (string)list.Attributes().FirstOrDefault()).FirstOrDefault();
            //                        val1 = src.Replace("[" + mat + "]", val2);
            //                    }
            //                }
            //            //}
            //        }
            //        BracketMatchesL = Regex.Matches(val1, @"\[(.+?)\]").Cast<Match>().Select(m => m.Groups[1].Value).ToList();
            //    } while (BracketMatchesL.Count == 0);
            //}

            var BracketMatches = Regex.Matches(src, @"\[(.+?)\]").Cast<Match>().Select(m => m.Groups[1].Value).ToList();
            docStr = "<Lists xmlns=\"http://schemas.microsoft.com/sharepoint/soap/\">";
            do
            {
                foreach (string mat in BracketMatches)
                {
                    var val3 = (from list in lists
                                where (string)list.Attribute("name") == mat
                                select (string)list.Attribute("value")).FirstOrDefault();
                    //var BracketMatchesI = Regex.Matches(val3, @"\[(.+?)\]").Cast<Match>().Select(m => m.Groups[1].Value).ToList();
                    //if (BracketMatchesI.Count == 0)
                    //{
                    val3 = val3.Replace("[TransactionId]", transId.ToString());
                    val3 = val3.Replace("[lt]", "<");
                    val3 = val3.Replace("[gt]", ">");
                    val3 = val3.Replace("[ne]", "<>");
                        using (ClientEntities cl = new ClientEntities())
                        {

                            foreach (string xm in cl.WFDefs(val3, "Select"))
                            {
                                XElement node1 = XElement.Parse(docStr + xm + "</Lists>");
                                var lists1 = from list in node1.Descendants()
                                             select list;
                                var val2 = (from list in lists1
                                            select (string)list.Attributes().FirstOrDefault()).FirstOrDefault();
                                src = src.Replace("[" + mat + "]", val2);
                            }
                        }
                   // }
                }
                BracketMatches = Regex.Matches(src, @"\[(.+?)\]").Cast<Match>().Select(m => m.Groups[1].Value).ToList();
            } while (BracketMatches.Count != 0);

            using (ClientEntities cl = new ClientEntities())
            {
                foreach (string xm in cl.WFDefs(src, "Select"))
                {
                    XElement node1 = XElement.Parse(docStr + xm + "</Lists>");
                    var lists1 = from list in node1.Descendants()
                                    select list;
                    var val2 = (from list in lists1
                                select (string)list.Attributes().FirstOrDefault()).FirstOrDefault();
                    if (val1.ToLower().Contains("select"))
                    {
                        foreach (string xmi in cl.WFDefs(val1, "Select"))
                        {
                            XElement nodeI = XElement.Parse(docStr + xm + "</Lists>");
                            var listsI = from list in node1.Descendants()
                                            select list;
                            val1 = (from list in lists1
                                        select (string)list.Attributes().FirstOrDefault()).FirstOrDefault();
                        }
                    }
                    if (val1 == val2)
                        return true;
                }
            }
            
            return false;
        }
    }
}