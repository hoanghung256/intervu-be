using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices
{
    /// <summary>
    /// DelegatingHandler that intercepts outgoing HTTP requests and increments
    /// per-host counters in the singleton ServiceMetricsStore.
    /// </summary>
    public class ServiceMetricsHandler : DelegatingHandler
    {
        private readonly ServiceMetricsStore _store;

        public ServiceMetricsHandler(ServiceMetricsStore store)
        {
            _store = store;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var host = request.RequestUri?.Host ?? "unknown";
            _store.Increment(host);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
