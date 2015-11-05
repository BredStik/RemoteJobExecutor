using RemoteJobExecutor.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace App2
{
    [Serializable]
    public class MyCustomJob : IJobImplementation<object>
    {
        public object Run(string parameters)//, CancellationToken ct)
        {
            for (int i = 0; i < 2; i++)
            {
                //ct.ThrowIfCancellationRequested();
                Thread.Sleep(5000);
            }

            return "Hello from MyCustomJob in App2";
        }
    }
}
