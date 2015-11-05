using RemoteJobExecutor.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace RemoteJobExecutor.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            const string netTcpAddress = "net.tcp://localhost:54542/JobExecutorService";

            var serviceHost = new ServiceHost(typeof(JobExecutorService), new Uri(netTcpAddress));
            serviceHost.AddServiceEndpoint(typeof(IJobExecutorService), new NetTcpBinding(), netTcpAddress);

            //var throttlingBehavior = new ServiceThrottlingBehavior() { MaxConcurrentInstances = 1 };
            //serviceHost.Description.Behaviors.Add(throttlingBehavior);

            var behavior = new ServiceMetadataBehavior();
            serviceHost.Description.Behaviors.Add(behavior);
            serviceHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

            serviceHost.Open();

            Console.WriteLine("Service now hosted at {0}", serviceHost.BaseAddresses[0]);

            Console.Read();
        }
    }
}
