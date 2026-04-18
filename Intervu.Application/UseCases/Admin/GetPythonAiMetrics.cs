using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetPythonAiMetrics : IGetPythonAiMetrics
    {
        private readonly IAiTrafficLogRepository _aiTrafficLogRepository;

        public GetPythonAiMetrics(IAiTrafficLogRepository aiTrafficLogRepository)
        {
            _aiTrafficLogRepository = aiTrafficLogRepository;
        }

        public async Task<PythonAiMetricsDto> ExecuteAsync(PythonAiMetricsQueryDto query)
        {
            query ??= new PythonAiMetricsQueryDto();
            var now = DateTime.UtcNow;

            DateTime from;
            DateTime to;
            if (query.From.HasValue || query.To.HasValue)
            {
                from = (query.From ?? now.AddDays(-30)).ToUniversalTime();
                to = (query.To ?? now).ToUniversalTime();
                if (to < from)
                {
                    (from, to) = (to, from);
                }
            }
            else
            {
                to = now;
                from = (query.Timeframe ?? "24h") switch
                {
                    "24h" => now.AddHours(-24),
                    "7d" => now.AddDays(-7),
                    "30d" => now.AddDays(-30),
                    _ => now.AddHours(-24)
                };
            }

            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 50 : query.PageSize;

            var (logs, totalCount) = await _aiTrafficLogRepository.QueryAsync(
                from, to, query.Provider, query.Endpoint, query.UseCase, page, pageSize);

            var items = logs.Select(l => new AiTrafficLogItemDto
            {
                Id = l.Id,
                Timestamp = l.Timestamp,
                EndpointName = l.EndpointName,
                UseCase = l.UseCase,
                Provider = l.Provider,
                PromptTokens = l.PromptTokens,
                CompletionTokens = l.CompletionTokens,
                TotalTokens = l.PromptTokens + l.CompletionTokens,
                LatencyMs = l.LatencyMs,
            }).ToList();

            var providers = await _aiTrafficLogRepository.GetDistinctProvidersAsync();
            var endpoints = await _aiTrafficLogRepository.GetDistinctEndpointsAsync();
            var useCases = await _aiTrafficLogRepository.GetDistinctUseCasesAsync();

            var seriesLogs = await _aiTrafficLogRepository.GetByTimeframeAsync(from, to);
            var (epSeries, seriesEndpoints, bucketUnit) = BuildEndpointSeries(seriesLogs, from, to, query.Endpoint, query.Provider, query.UseCase);
            var (ucSeries, seriesUseCases, _) = BuildUseCaseSeries(seriesLogs, from, to, query.Endpoint, query.Provider, query.UseCase);

            return new PythonAiMetricsDto
            {
                TotalRequests = totalCount,
                TotalPromptTokens = items.Sum(x => (long)x.PromptTokens),
                TotalCompletionTokens = items.Sum(x => (long)x.CompletionTokens),
                TotalTokens = items.Sum(x => (long)x.TotalTokens),
                AverageLatencyMs = items.Count > 0 ? items.Average(x => (double)x.LatencyMs) : 0,
                ServiceCount = items.Select(x => x.Provider).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                AvailableProviders = providers.ToList(),
                AvailableEndpoints = endpoints.ToList(),
                AvailableUseCases = useCases.ToList(),
                Logs = items,
                EndpointSeries = epSeries,
                SeriesEndpoints = seriesEndpoints,
                UseCaseSeries = ucSeries,
                SeriesUseCases = seriesUseCases,
                SeriesBucket = bucketUnit,
            };
        }

        private static (List<DateTime> buckets, TimeSpan step, Func<DateTime, DateTime> truncate, string bucketUnit)
            BuildBuckets(DateTime from, DateTime to)
        {
            var totalSpan = to - from;
            string bucketUnit;
            TimeSpan step;
            Func<DateTime, DateTime> truncate;

            if (totalSpan.TotalHours <= 48)
            {
                bucketUnit = "hour";
                step = TimeSpan.FromHours(1);
                truncate = dt => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, DateTimeKind.Utc);
            }
            else
            {
                bucketUnit = "day";
                step = TimeSpan.FromDays(1);
                truncate = dt => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc);
            }

            var buckets = new List<DateTime>();
            var cursor = truncate(from);
            var end = truncate(to);
            while (cursor <= end)
            {
                buckets.Add(cursor);
                cursor = cursor.Add(step);
            }
            return (buckets, step, truncate, bucketUnit);
        }

        private static IEnumerable<Intervu.Domain.Entities.AiTrafficLog> ApplyFilters(
            IEnumerable<Intervu.Domain.Entities.AiTrafficLog> logs,
            string? endpointFilter,
            string? providerFilter,
            string? useCaseFilter)
        {
            if (!string.IsNullOrWhiteSpace(providerFilter))
            {
                logs = logs.Where(l => string.Equals(l.Provider, providerFilter, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(endpointFilter))
            {
                logs = logs.Where(l => l.EndpointName != null &&
                    l.EndpointName.Contains(endpointFilter, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(useCaseFilter))
            {
                logs = logs.Where(l => string.Equals(l.UseCase, useCaseFilter, StringComparison.OrdinalIgnoreCase));
            }
            return logs;
        }

        private static (List<AiEndpointSeriesPointDto> series, List<string> seriesEndpoints, string bucketUnit)
            BuildEndpointSeries(
                IEnumerable<Intervu.Domain.Entities.AiTrafficLog> logs,
                DateTime from,
                DateTime to,
                string? endpointFilter,
                string? providerFilter,
                string? useCaseFilter)
        {
            var (buckets, _, truncate, bucketUnit) = BuildBuckets(from, to);
            var materialized = ApplyFilters(logs, endpointFilter, providerFilter, useCaseFilter).ToList();

            var topEndpoints = materialized
                .GroupBy(l => l.EndpointName ?? string.Empty)
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .OrderByDescending(g => g.Count())
                .Take(6)
                .Select(g => g.Key)
                .ToList();

            var grouped = materialized
                .Where(l => topEndpoints.Contains(l.EndpointName ?? string.Empty))
                .GroupBy(l => new { Bucket = truncate(l.Timestamp.ToUniversalTime()), l.EndpointName })
                .ToDictionary(g => (g.Key.Bucket, g.Key.EndpointName ?? string.Empty), g => g.Count());

            var series = buckets.Select(b =>
            {
                var point = new AiEndpointSeriesPointDto { Bucket = b };
                foreach (var ep in topEndpoints)
                {
                    point.CountByEndpoint[ep] = grouped.TryGetValue((b, ep), out var c) ? c : 0;
                }
                return point;
            }).ToList();

            return (series, topEndpoints, bucketUnit);
        }

        private static (List<AiUseCaseSeriesPointDto> series, List<string> seriesUseCases, string bucketUnit)
            BuildUseCaseSeries(
                IEnumerable<Intervu.Domain.Entities.AiTrafficLog> logs,
                DateTime from,
                DateTime to,
                string? endpointFilter,
                string? providerFilter,
                string? useCaseFilter)
        {
            var (buckets, _, truncate, bucketUnit) = BuildBuckets(from, to);
            var materialized = ApplyFilters(logs, endpointFilter, providerFilter, useCaseFilter).ToList();

            var topUseCases = materialized
                .GroupBy(l => l.UseCase ?? string.Empty)
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .OrderByDescending(g => g.Count())
                .Take(8)
                .Select(g => g.Key)
                .ToList();

            var grouped = materialized
                .Where(l => topUseCases.Contains(l.UseCase ?? string.Empty))
                .GroupBy(l => new { Bucket = truncate(l.Timestamp.ToUniversalTime()), l.UseCase })
                .ToDictionary(g => (g.Key.Bucket, g.Key.UseCase ?? string.Empty), g => g.Count());

            var series = buckets.Select(b =>
            {
                var point = new AiUseCaseSeriesPointDto { Bucket = b };
                foreach (var uc in topUseCases)
                {
                    point.CountByUseCase[uc] = grouped.TryGetValue((b, uc), out var c) ? c : 0;
                }
                return point;
            }).ToList();

            return (series, topUseCases, bucketUnit);
        }
    }
}
