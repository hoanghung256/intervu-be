using System.Collections.Concurrent;

namespace Intervu.Infrastructure.ExternalServices
{
    /// <summary>
    /// Singleton in-memory store for outgoing HTTP request counts per service host.
    /// Resets on application restart — no persistence needed for MVP.
    /// </summary>
    public class ServiceMetricsStore
    {
        private readonly ConcurrentDictionary<string, long> _requestCounts = new(StringComparer.OrdinalIgnoreCase);

        public void Increment(string host) =>
            _requestCounts.AddOrUpdate(host, 1, (_, count) => count + 1);

        public long GetCount(string host) =>
            _requestCounts.TryGetValue(host, out var count) ? count : 0;

        public long GetCountByPartialHost(string partialHost)
        {
            long total = 0;
            foreach (var kvp in _requestCounts)
            {
                if (kvp.Key.Contains(partialHost, StringComparison.OrdinalIgnoreCase))
                    total += kvp.Value;
            }
            return total;
        }
    }
}
