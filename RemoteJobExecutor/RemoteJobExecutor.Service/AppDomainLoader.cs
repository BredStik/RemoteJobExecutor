using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteJobExecutor.Service
{
    public class AppDomainLoader
    {
        private readonly Job _job;
        private readonly CancellationToken _ct;

        public AppDomainLoader(Job job, CancellationToken ct)
        {
            _job = job;
            //_ct = ct;
        }

        public TResult RunJob<TResult>()
        {
            var fullType = _job.JobType.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var type = fullType[0].Trim();
            var assembly = fullType[1].Trim();

            // Create application domain setup information.
            var domaininfo = new AppDomainSetup
            {
                ApplicationBase = _job.AppName
            };

            AppDomain domain = null;

            try
            {
                // Create the application domain.
                domain = AppDomain.CreateDomain(_job.AppName, null, domaininfo);

                var task = domain.CreateInstanceAndUnwrap(assembly, type) as ITask;
                var tokenSource = domain.CreateInstanceAndUnwrap("RemoteJobExecutor.Service", "RemoteJobExecutor.Service.InterAppDomainCancellable") as InterAppDomainCancellable;

                if (task != null && tokenSource != null)
                {
                    //set principal to user who called service
                    //domain.SetThreadPrincipal(task.GetPrincipal("lafom5"));

                    _ct.Register(() => tokenSource.Cancel());

                    task.Run(tokenSource);

                    var assemblies = task.GetLoadedAssemblies();

                    return default(TResult);
                }

                return default(TResult);
            }
            finally
            {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }
    }
}
