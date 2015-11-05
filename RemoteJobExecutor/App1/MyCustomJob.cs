using RemoteJobExecutor.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace App1
{
    public class MyCustomJob: MarshalByRefObject, ITask
    {
    //    public object Run(string parameters)//, CancellationToken ct)
    //    {
           

    //        for (int i = 0; i < 2; i++)
    //        {
    //            //ct.ThrowIfCancellationRequested();
    //            Thread.Sleep(5000);
    //        }

    //        return "Hello from MyCustomJob in App1";
    //    }

        public void Run(ITokenSource tokenSource)
        {
            for (int i = 0; i < 5; i++)
            {
                tokenSource.Token.ThrowIfCancellationRequested();
                Thread.Sleep(5000);
            }

            AutoMapper.Mapper.CreateMap<Class1, Class2>();
        }

        public string[] GetLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Select(x => x.FullName).ToArray();
        }

        public string GetRunningIdentity()
        {
            throw new NotImplementedException();
        }

        public System.Security.Principal.IPrincipal GetPrincipal(string login)
        {
            throw new NotImplementedException();
        }

        public System.Security.Principal.IPrincipal GetThreadPrincipal()
        {
            throw new NotImplementedException();
        }
    }

    public class Class1
    {
        public string Name { get; set; }
    }

    public class Class2
    {
        public string Name { get; set; }
    }
}
