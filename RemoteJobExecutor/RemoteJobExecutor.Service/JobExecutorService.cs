using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteJobExecutor.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class JobExecutorService : IJobExecutorService
    {
        private static ConcurrentQueue<Job> _queuedJobs = new ConcurrentQueue<Job>();
        private static ConcurrentDictionary<Guid, TaskDefinition> _startedJobs = new ConcurrentDictionary<Guid, TaskDefinition>();
        private static ConcurrentDictionary<Guid, DateTime> _completedJobsDateTimes = new ConcurrentDictionary<Guid, DateTime>();
        private static System.Timers.Timer _timer = new System.Timers.Timer(10000) { AutoReset = true };

        static JobExecutorService()
        {
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TaskDefinition td;

            if (_startedJobs.Count(x => x.Value.Task.Status == TaskStatus.Running) > 2)
                return;

            if (_queuedJobs.Count == 0)
                return;

            Job dequeuedJob;

            if (_queuedJobs.TryDequeue(out dequeuedJob))
                InternalStartJob(dequeuedJob);
        }

        private static void InternalStartJob(Job job)
        {
            TaskDefinition td;
            var tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;

            if (job.Cancelled)
                tokenSource.Cancel();

            var task = new Task<object>(() =>
            {
                ct.ThrowIfCancellationRequested();

                return new AppDomainLoader(job, ct).RunJob<object>();
            }, ct);

            _startedJobs.GetOrAdd(job.Id, new TaskDefinition(task, tokenSource));

            if (task.Status != TaskStatus.Canceled)
            {
                task.ContinueWith(t => _completedJobsDateTimes.GetOrAdd(job.Id, DateTime.Now));
                task.Start();
            }
        }


        public void StartJob(Job job)
        {
            _queuedJobs.Enqueue(job);
        }

        public void CancelJob(Guid jobId)
        {
            var queuedJob = _queuedJobs.SingleOrDefault(x => x.Id.Equals(jobId));
            if (queuedJob != null)
            {
                queuedJob.Cancelled = true;
            }

            if (_startedJobs.ContainsKey(jobId))
                _startedJobs[jobId].CancellationTokenSource.Cancel();
        }

        public JobStatus GetJobStatus(Guid jobId)
        {
            if (_queuedJobs.Any(x => x.Id.Equals(jobId)))
                return JobStatus.Queued;
            
            if (_startedJobs.ContainsKey(jobId))
            {
                switch(_startedJobs[jobId].Task.Status)
                {
                    case TaskStatus.Running:
                        return JobStatus.Running;
                    case TaskStatus.Canceled:
                        return JobStatus.Cancelled;
                    case TaskStatus.Faulted:
                        return JobStatus.Faulted;
                    case TaskStatus.RanToCompletion:
                        return JobStatus.Completed;
                    default:
                        return JobStatus.Other;
                }
            }
            throw new InvalidOperationException("Could not find the requested job status.");
        }

        public object GetJobResult(Guid jobId)
        {
            if (_startedJobs.ContainsKey(jobId) && _startedJobs[jobId].Task.Status == TaskStatus.RanToCompletion)
                return _startedJobs[jobId].Task.Result;

            return null;
        }

        public int GetJobsCount(JobStatus status)
        {
            switch (status)
            { 
                case JobStatus.Running:
                    return _startedJobs.Count(x => x.Value.Task.Status == TaskStatus.Running);
                case JobStatus.Cancelled:
                    return _startedJobs.Count(x => x.Value.Task.Status == TaskStatus.Canceled);
                case JobStatus.Completed:
                    return _startedJobs.Count(x => x.Value.Task.Status == TaskStatus.RanToCompletion);
                case JobStatus.Faulted:
                    return _startedJobs.Count(x => x.Value.Task.Status == TaskStatus.Faulted);
                case JobStatus.Queued:
                    return _queuedJobs.Count;
            }

            return 0;
            
        }
    }

    public class TaskDefinition
    {
        private readonly Task<object> _task;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TaskDefinition(Task<object> task, CancellationTokenSource cancellationTokenSource)
        {
            _task = task;   
            _cancellationTokenSource = cancellationTokenSource;
        }

        public CancellationTokenSource CancellationTokenSource
        {
            get { return _cancellationTokenSource; }
        }

        public Task<object> Task
        {
            get { return _task; }
        }
    }
    [DataContract]
    public enum JobStatus
    {
        [EnumMember]
        Queued,
        [EnumMember]
        Running,
        [EnumMember]
        Completed,
        [EnumMember]
        Cancelled,
        [EnumMember]
        Faulted,
        [EnumMember]
        Other
    }
}
