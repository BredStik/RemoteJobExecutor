using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RemoteJobExecutor.Service
{
    [DataContract]
    [Serializable]
    public class Job
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string AppName{ get; set; }
        [DataMember]
        public string JobType { get; set; }
        [DataMember]
        public string Parameters { get; set; }
        [DataMember]
        public bool Cancelled{ get; set; }
        [DataMember]
        public string Identity{ get; set; }
    }
}
