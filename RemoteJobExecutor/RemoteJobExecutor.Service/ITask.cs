using System.Security.Principal;

namespace RemoteJobExecutor.Service
{
    public interface ITask
    {
        void Run(ITokenSource tokenSource);
        string[] GetLoadedAssemblies();
        string GetRunningIdentity();
        IPrincipal GetPrincipal(string login);
        IPrincipal GetThreadPrincipal();
    }
}