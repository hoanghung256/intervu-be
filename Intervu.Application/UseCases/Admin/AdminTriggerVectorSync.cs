using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class AdminTriggerVectorSync : IAdminTriggerVectorSync
    {
        private readonly IBackgroundService _backgroundService;

        public AdminTriggerVectorSync(IBackgroundService backgroundService)
        {
            _backgroundService = backgroundService;
        }

        public Task ExecuteAsync(string @namespace)
        {
            switch (@namespace?.ToLowerInvariant())
            {
                case "coaches":
                    _backgroundService.Enqueue<ISyncCoachVectors>(x => x.ExecuteAsync());
                    break;
                case "questions":
                    _backgroundService.Enqueue<ISyncQuestionVectors>(x => x.ExecuteAsync());
                    break;
                default:
                    throw new ArgumentException($"Unknown namespace '{@namespace}'. Valid values: 'coaches', 'questions'.");
            }

            return Task.CompletedTask;
        }
    }
}
