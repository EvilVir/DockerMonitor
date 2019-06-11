using System.Threading;

namespace StrangeFog.Docker.Monitor.Services
{
    public class GlobalCancellationToken
    {
        public CancellationTokenSource Source { get; set; } = new CancellationTokenSource();
        public CancellationToken Token => Source.Token;
    }
}
