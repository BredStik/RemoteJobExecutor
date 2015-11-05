using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace RemoteJobExecutor.Service
{
    [ServiceContract]
    public interface IJobExecutorService
    {
        [OperationContract]
        void StartJob(Job job);
        [OperationContract]
        void CancelJob(Guid id);
        [OperationContract]
        JobStatus GetJobStatus(Guid jobId);
        [OperationContract]
        int GetJobsCount(JobStatus status);
        [OperationContract]
        object GetJobResult(Guid jobId);
    }
}
