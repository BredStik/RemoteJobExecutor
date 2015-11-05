using RemoteJobExecutor.Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RemoteJobExecutor.Client
{
    class Program
    {
        private static ConcurrentQueue<Guid> _jobIds = new ConcurrentQueue<Guid>();

        static void Main(string[] args)
        {
            var jobId = Guid.NewGuid();
            _jobIds.Enqueue(jobId);

            ActionWithService(service => service.StartJob(new Job { Id = Guid.NewGuid(), AppName = "App1", JobType = "App1.MyCustomJob,App1" }));

            string input = string.Empty;

            while (input != "quit")
            {
                Console.WriteLine("Enter 'quit' to exit.");
                input = Console.ReadLine();

                JobStatus requestedStatus;

                if (Enum.TryParse<JobStatus>(input, out requestedStatus))
                {
                    Console.WriteLine("There are currently {0} jobs with {1} status", ActionWithService(x => x.GetJobsCount(requestedStatus)), requestedStatus);
                }

                if (input.Equals("Cancel", StringComparison.InvariantCultureIgnoreCase))
                {
                    Guid jobIdToCancel;

                    if (_jobIds.TryDequeue(out jobIdToCancel))
                    {
                        ActionWithService(x => x.CancelJob(jobIdToCancel));
                        Console.WriteLine("Cancelled job with id {0}.", jobIdToCancel);
                    }
                }

                if (input.Equals("Result", StringComparison.InvariantCultureIgnoreCase))
                {
                    Guid jobIdToGetResult;

                    if (_jobIds.TryDequeue(out jobIdToGetResult))
                    {
                        var result = ActionWithService(x => x.GetJobResult(jobIdToGetResult));
                        Console.WriteLine("result: {0}", result);
                    }
                }
            }

            return;


            var tasks = new List<Task>();

            for (int i = 0; i < 4; i++)
            {
                int i1 = i;
                tasks.Add(new Task(() =>
                {
                    var timeout = TimeSpan.FromSeconds(60);

                    var binding = new NetTcpBinding()
                    {
                        CloseTimeout = timeout,
                        SendTimeout = timeout,
                        OpenTimeout = timeout,
                        ReceiveTimeout = timeout
                    };

                    var endpoint =
                        new EndpointAddress(new Uri("net.tcp://localhost:54542/JobExecutorService"));
                    //, EndpointIdentity.CreateUpnIdentity("name@address.com"));

                    var factory = new ChannelFactory<IJobExecutorService>(binding, endpoint);

                    IClientChannel clientChannel = null;

                    try
                    {
                        var channel = factory.CreateChannel();
                        
                        clientChannel = channel as IClientChannel;

                        jobId = Guid.NewGuid();
                        Console.WriteLine(jobId);


                        channel.StartJob(new Job() { Id =jobId });
                        
                        _jobIds.Enqueue(jobId);

                        Console.WriteLine("There are currently {0} jobs running", channel.GetJobsCount(JobStatus.Running));
                        Console.WriteLine("Successully made the call to the service");
                    }

                    catch (TimeoutException exc)
                    {
                        Console.WriteLine("An timeout exception occured:");
                        Console.WriteLine();
                        Console.WriteLine(exc.ToString());
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("An exception occured:");
                        Console.WriteLine();
                        Console.WriteLine(exc.ToString());
                    }
                    finally
                    {
                        if (clientChannel != null &&
                            clientChannel.State != CommunicationState.Faulted)
                            clientChannel.Dispose();

                        var states = new[]
                                                                 {
                                                                     CommunicationState.Faulted, CommunicationState.Closing
                                                                     , CommunicationState.Closed
                                                                 };


                        //if (!states.Contains(factory.State))
                        //    factory.Close();
                    }
                }));
            }

            Parallel.ForEach(tasks, task => task.Start());

            input = string.Empty;

            while (input != "quit")
            {
                Console.WriteLine("Enter 'quit' to exit.");
                input = Console.ReadLine();

                JobStatus requestedStatus;

                if (Enum.TryParse<JobStatus>(input, out requestedStatus))
                {
                    Console.WriteLine("There are currently {0} jobs with {1} status", ActionWithService(x => x.GetJobsCount(requestedStatus)), requestedStatus);
                }

                if (input.Equals("Cancel", StringComparison.InvariantCultureIgnoreCase))
                { 
                    Guid jobIdToCancel;

                    if (_jobIds.TryDequeue(out jobIdToCancel))
                    {
                        ActionWithService(x => x.CancelJob(jobIdToCancel));
                        Console.WriteLine("Cancelled job with id {0}.", jobIdToCancel);
                    }
                }

                if (input.Equals("Result", StringComparison.InvariantCultureIgnoreCase))
                {
                    Guid jobIdToGetResult;

                    if (_jobIds.TryDequeue(out jobIdToGetResult))
                    {
                        var result = ActionWithService(x => x.GetJobResult(jobIdToGetResult));
                        Console.WriteLine("result: {0}", result);
                    }
                }
            }
        }

        private static T ActionWithService<T>(Func<IJobExecutorService, T> action)
        {
            var binding = new NetTcpBinding();

            var endpoint =
                new EndpointAddress(new Uri("net.tcp://localhost:54542/JobExecutorService"));

            var factory = new ChannelFactory<IJobExecutorService>(binding, endpoint);

            IClientChannel clientChannel = null;

            try
            {
                var channel = factory.CreateChannel();

                clientChannel = channel as IClientChannel;

                return action.Invoke(channel);
            }
            finally
            {
                clientChannel.Dispose();
            }
        }

        private static void ActionWithService(Action<IJobExecutorService> action)
        {
            var binding = new NetTcpBinding();

            var endpoint =
                new EndpointAddress(new Uri("net.tcp://localhost:54542/JobExecutorService"));

            var factory = new ChannelFactory<IJobExecutorService>(binding, endpoint);

            IClientChannel clientChannel = null;

            try
            {
                var channel = factory.CreateChannel();

                clientChannel = channel as IClientChannel;

                action.Invoke(channel);
            }
            finally
            {
                clientChannel.Dispose();
            }
        }
    }

    
}
