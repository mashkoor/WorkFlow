using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace JPClientService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/GetData/{value}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string GetData(string value);
        
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/MoveTo/{MHandler}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        FlowResponse MoveTo(string MHandler);

        [OperationContract]
        [WebGet(UriTemplate = "Load", ResponseFormat = WebMessageFormat.Json)]
        string Load();

        
    }
    [DataContract]
    public class FlowResponse
    {
        //<result currentstatus=\"" + cState + "\" conditionssuccess=\"0\" err=\"\" />

        bool _conditionssuccess = false;
        string _currentstatus;
        string _err;

        [DataMember]
        public bool conditionssuccess
        {
            get { return _conditionssuccess; }
            set { _conditionssuccess = value; }
        }


        [DataMember]
        public string currentstatus
        {
            get { return _currentstatus; }
            set { _currentstatus = value; }
        }

        [DataMember]
        public string Err
        {
            get { return _err; }
            set { _err = value; }
        }
    }
}
