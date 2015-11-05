using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteJobExecutor.Service
{
    // I split this into a separate interface simply to make the boundary between
    // canceller and cancellee explicit, similar to CancellationTokenSource itself.
    public interface ITokenSource
    {
        CancellationToken Token { get; }
    }

    public class InterAppDomainCancellable : MarshalByRefObject,
                                            ITokenSource,
                                            IDisposable
    {
        private readonly CancellationTokenSource _cts;
        
        public InterAppDomainCancellable()
        {
            _cts = new CancellationTokenSource();
        }

        public void Cancel() { _cts.Cancel(); }

        // Explicitly implemented to make it less tempting to call Token
        // from the wrong side of the boundary.
        CancellationToken ITokenSource.Token { get { return _cts.Token; } }

        public void Dispose() { _cts.Dispose(); }

        
    }
}
